using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace UI.Memory {
	/// <summary>
	/// Provides functionality to generate text embeddings using a locally hosted MiniLM ONNX model and vocabulary file.
	/// </summary>
	/// <remarks>This class loads a MiniLM model and its associated vocabulary from local files, enabling efficient
	/// embedding generation for input text. Embeddings are produced by tokenizing input strings, encoding them as tensors,
	/// and running inference through the ONNX model. The class supports asynchronous embedding generation and ensures that
	/// output vectors are L2-normalized for consistency. Instances of this class are disposable and should be disposed to
	/// release model resources when no longer needed.</remarks>
	public sealed partial class LocalMiniLmEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>, IDisposable {
		internal readonly InferenceSession _session;
		internal readonly string _inputIdsName;
		internal readonly string _attentionMaskName;
		internal readonly string _preferredOutputName;
		internal readonly int _maxTokenLength;

		internal readonly Dictionary<string, int> _vocab;
		internal readonly string _vocabularyPath;

		private readonly int _padId, _clsId, _sepId, _unkId;
		private static readonly Regex TokenRegex = WhitespaceRegex();

		[GeneratedRegex(@"\w+|[^\s\w]", RegexOptions.Compiled)]
		private static partial Regex WhitespaceRegex();

		/// <summary>
		/// Retrieves the identifier associated with the specified token, or returns the fallback value if the token is not
		/// found.
		/// </summary>
		private int GetIdOrDefault(string token, int fallback) => _vocab.GetValueOrDefault(token, fallback);

		public LocalMiniLmEmbeddingGenerator(string onnxPath, string vocabularyPath, int maxTokenLength = 128, SessionOptions? options = null) {
			options ??= new SessionOptions();

			_session = new InferenceSession(onnxPath, options);
			_vocabularyPath = vocabularyPath;

			List<string> outputs = _session.OutputMetadata.Keys.ToList();
			_preferredOutputName = outputs.FirstOrDefault(n => n.Contains(Constants._pooled, StringComparison.OrdinalIgnoreCase))
								 ?? outputs.FirstOrDefault(n => n.Contains(Constants._hidden, StringComparison.OrdinalIgnoreCase))
								 ?? outputs.First();

			_inputIdsName =
				_session.InputMetadata.Keys
					.FirstOrDefault(key =>
						key.Contains(Constants._inputIds, StringComparison.OrdinalIgnoreCase))
								?? _session.InputMetadata.Keys.First();

			_attentionMaskName =
				_session.InputMetadata.Keys
					.FirstOrDefault(key =>
						key.Contains(Constants._attentionMask, StringComparison.OrdinalIgnoreCase))
								?? _session.InputMetadata.Keys.Skip(1).FirstOrDefault()
							?? _inputIdsName;

			_maxTokenLength = maxTokenLength;

			_vocab = LoadVocab(vocabularyPath);
			_padId = GetIdOrDefault(Constants._pad, 0);
			_clsId = GetIdOrDefault(Constants._cls, 1);
			_sepId = GetIdOrDefault(Constants._sep, 2);
			_unkId = GetIdOrDefault(Constants._unk, 3);
		}

		/// <summary>
		/// Loads a vocabulary from a text file, mapping each non-empty line to a unique integer index.
		/// </summary>
		/// <remarks>Empty or whitespace-only lines in the file are ignored. Token comparison is case-sensitive and
		/// uses ordinal string comparison.</remarks>
		/// <param name="path">The path to the text file containing the vocabulary. Each line in the file should represent a distinct token.</param>
		/// <returns>A dictionary that maps each token from the file to its corresponding zero-based index. Tokens are assigned indices
		/// in the order they appear in the file.</returns>
		private static Dictionary<string, int> LoadVocab(string path) {
			Dictionary<string, int> dict = new(StringComparer.Ordinal);
			int i = 0;
			foreach (string line in File.ReadLines(path)) {
				string? token = line?.Trim();
				if (string.IsNullOrEmpty(token)) {
					continue;
				}
				dict[token] = i++;
			}
			return dict;
		}

		/// <summary>
		/// Generates embeddings for the specified collection of input text(s) asynchronously.
		/// </summary>
		/// <remarks>Embeddings are generated sequentially for each input text. The method normalizes each embedding
		/// vector using L2 normalization before returning. If the operation is canceled via the provided cancellation token,
		/// an OperationCanceledException is thrown.</remarks>
		/// <param name="values">The collection of input strings for which embeddings will be generated. Each string represents a separate text to
		/// process.</param>
		/// <param name="options">Optional configuration settings that influence embedding generation, such as model parameters or output format. If
		/// null, default options are used.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains a collection of generated embeddings
		/// corresponding to each input string.</returns>
		public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) {
			// We'll process sequentially here; you can batch by building larger tensors.
			GeneratedEmbeddings<Embedding<float>> result = [];

			foreach (string text in values) {
				cancellationToken.ThrowIfCancellationRequested();

				(DenseTensor<long>? inputIds, DenseTensor<long>? attentionMask) = TokenizeToTensors(text);

				List<NamedOnnxValue> inputs = [
					NamedOnnxValue.CreateFromTensor(_inputIdsName, inputIds),
				NamedOnnxValue.CreateFromTensor(_attentionMaskName, attentionMask)
				];

				using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs = _session.Run(inputs);

				// pick the output we detected earlier
				DisposableNamedOnnxValue named = outputs.FirstOrDefault(o => string.Equals(o.Name, _preferredOutputName, StringComparison.OrdinalIgnoreCase)) ?? outputs.First();
				if (named.Value is DenseTensor<float> tensor) {
					float[] vector = ExtractVectorFromTensor(tensor, attentionMask);
					NormalizeL2(vector);
					result.Add(new Embedding<float>(vector));
				} else {
					MessageBox.Show(Constants._userFriendlyONNXFloat32TensorError);
				}
			}

			return await Task.FromResult(result);
		}

		/// <summary>
		/// Converts the specified input text into token ID and attention mask tensors suitable for model inference.
		/// </summary>
		/// <remarks>The returned tensors are padded or truncated to a fixed length as required by the model. Unknown
		/// words are replaced with a special token. The method applies Unicode normalization and lowercasing to the input
		/// text before tokenization.</remarks>
		/// <param name="text">The input text to tokenize and encode into tensors. The text is normalized and split into tokens before
		/// conversion.</param>
		/// <returns>A tuple containing a tensor of token IDs and a tensor of attention masks. The token ID tensor represents the
		/// encoded input sequence, and the attention mask tensor indicates which positions are valid tokens.</returns>
		private (DenseTensor<long> inputIds, DenseTensor<long> attentionMask) TokenizeToTensors(string text) {
			// Mini word piece tokenizer: split into tokens and greedily match vocabulary pieces.
			string normalized = text.Normalize(System.Text.NormalizationForm.FormKC).ToLowerInvariant();
			List<string> words = TokenRegex.Matches(normalized).Select(m => m.Value).ToList();

			List<int> tokenIds = [_clsId];

			foreach (string word in words) {
				int start = 0;
				List<int> subTokens = [];
				bool bad = false;
				while (start < word.Length) {
					int end = word.Length;
					string? found = null;
					int foundId = -1;
					while (end > start) {
						string piece = word.Substring(start, end - start);
						if (start > 0)
							piece = Constants._poundItTwice + piece; // wordpiece continuation marker
						if (_vocab.TryGetValue(piece, out foundId)) {
							found = piece;
							break;
						}
						end--;
					}
					if (found == null) {
						bad = true;
						break;
					}
					subTokens.Add(foundId);
					start = end;
				}

				if (bad) {
					tokenIds.Add(_unkId);
				} else {
					tokenIds.AddRange(subTokens);
				}
			}

			tokenIds.Add(_sepId);

			if (tokenIds.Count > _maxTokenLength) {
				tokenIds = tokenIds.Take(_maxTokenLength - 1).Concat([_sepId]).ToList();
			}

			DenseTensor<long> inputIds = new(new[] { 1, _maxTokenLength });
			DenseTensor<long> mask = new(new[] { 1, _maxTokenLength });

			for (int i = 0; i < _maxTokenLength; i++) {
				inputIds[0, i] = i < tokenIds.Count ? tokenIds[i] : _padId;
				mask[0, i] = i < tokenIds.Count ? 1L : 0L;
			}
			return (inputIds, mask);
		}

		/// <summary>
		/// Extracts a feature vector from the specified tensor, optionally applying mean pooling over a sequence dimension
		/// using the provided attention mask.
		/// </summary>
		/// <remarks>When the input tensor has rank 3, mean pooling is performed over the sequence dimension,
		/// considering only positions where the attention mask is nonzero. If no valid positions are found, the method
		/// returns a vector of zeros.</remarks>
		/// <param name="tensor">The input tensor containing embedding data. Must have a rank of 2 or 3, where rank 2 represents a single embedding
		/// and rank 3 represents a sequence of embeddings.</param>
		/// <param name="attentionMask">The attention mask tensor used to indicate valid sequence positions for mean pooling when the input tensor has
		/// rank 3. Positions with a value of 0 are ignored during pooling.</param>
		/// <returns>A float array containing the extracted feature vector. If mean pooling is applied, the vector represents the
		/// average over valid sequence positions.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the input tensor does not have a rank of 2 or 3.</exception>
		private static float[] ExtractVectorFromTensor(DenseTensor<float> tensor, DenseTensor<long> attentionMask) {
			switch (tensor.Rank) {
				case 2: {
						// [1, hidden]
						int hidden = tensor.Dimensions[1];
						float[] vector = new float[hidden];
						for (int i = 0; i < hidden; i++) {
							vector[i] = tensor[0, i];
						}
						return vector;
					}
				case 3: {
						// [1, seq_len, hidden] => mean pooling with mask
						int seqLen = tensor.Dimensions[1];
						int hidden = tensor.Dimensions[2];
						float[] vector = new float[hidden];
						long valid = 0;
						for (int i = 0; i < seqLen; i++) {
							if (attentionMask[0, i] == 0) {
								continue;
							}
							valid++;
							for (int h = 0; h < hidden; h++) {
								vector[h] += tensor[0, i, h];
							}
						}
						if (valid == 0) {
							valid = 1;
						}
						for (int h = 0; h < hidden; h++) {
							vector[h] /= valid;
						}
						return vector;
					}
				default:
					throw new InvalidOperationException($"Unexpected tensor rank {tensor.Rank} for embedding extraction.");
			}
		}

		/// <summary>
		/// Normalizes the specified vector in place using the L2 (Euclidean) norm.
		/// </summary>
		/// <remarks>If the L2 norm of the vector is less than or equal to 1e-12, the method leaves the vector
		/// unchanged. This method does not allocate a new array; the input array is modified directly.</remarks>
		/// <param name="vector">The array of single-precision floating-point values to normalize. The array is modified in place to have unit L2
		/// norm unless its norm is zero or negligibly small.</param>
		private static void NormalizeL2(float[] vector) {
			double sum = 0;
			foreach (float val in vector) {
				sum += (double)val * val;
			}
			double norm = Math.Sqrt(sum);
			if (norm <= 1e-12) {
				return;
			}
			for (int i = 0; i < vector.Length; i++) {
				vector[i] = (float)(vector[i] / norm);
			}
		}

		public void Dispose() => _session?.Dispose();
		public object? GetService(Type serviceType, object? serviceKey = null) => throw new NotImplementedException();
	}
}