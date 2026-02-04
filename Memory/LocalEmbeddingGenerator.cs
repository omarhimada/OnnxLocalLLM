using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using UI.Initialization;
using static UI.Constants;

namespace UI.Memory {
	internal sealed partial class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>> {

		[GeneratedRegex(@"\w+|[^\s\w]", RegexOptions.Compiled)]
		private static partial Regex WhiteSpaceAndPunctuationRegex();

		#region Initialize these once
		internal int _padId { get; set; }
		internal int _clsId { get; set; }
		internal int _unkId { get; set; }
		internal int _sepId { get; set; }

		internal string _vocabularyPath = string.Empty;
		internal Dictionary<string, int> _vocabulary = [];
		#endregion

		private readonly InferenceSession _session;

		internal LocalEmbeddingGenerator(string embedModelPath) {
			// Optimization: Use SessionOptions to enable GPU if available
			SessionOptions options = new();

			#region Try DML, then CUDA, and finally fallback to CPU
			try {
				options.AppendExecutionProvider_DML();
			} catch (NotSupportedException) {
				MessageBox.Show(_userFriendlyErrorOccurredLoadingDMLProvider);
				try {
					options.AppendExecutionProvider_CUDA();
				} catch (Exception) {
					MessageBox.Show(_userFriendlyErrorOccurredLoadingCUDAProvider);
					options.AppendExecutionProvider_CPU();
				}
			} catch (Exception) {
				options.AppendExecutionProvider_CPU();
			}
			#endregion

			_session = new InferenceSession(embedModelPath, options);
			_vocabularyPath = Vocabulary.GetRequiredTextDocument(_debugModeVocabTextPath) ?? string.Empty;

			if (!string.IsNullOrEmpty(_vocabularyPath)) {
				_vocabulary = new(StringComparer.Ordinal);
				IEnumerable<(string?, int i)> allLines = File.ReadLines(_vocabularyPath).Select((l, i) => (l?.Trim(), i));

				foreach ((string? line, int index) in allLines) {
					if (string.IsNullOrEmpty(line)) {
						continue;
					}

					_vocabulary.TryAdd(line, index);
				}

				int GetIdOrDefault(string token, int fallback) => _vocabulary.GetValueOrDefault(token, fallback);

				_padId = GetIdOrDefault(_pad, 0);
				_unkId = GetIdOrDefault(_unk, 100);
				_clsId = GetIdOrDefault(_cls, GetIdOrDefault(_mistral3TokenStartTurn, 101));
				_sepId = GetIdOrDefault(_sep, GetIdOrDefault(_mistral3TokenStop, 102));
			}
		}

		public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
			IEnumerable<string> values,
			EmbeddingGenerationOptions? options = null,
			CancellationToken cancellationToken = default) {

			List<Embedding<float>> results = [];

			foreach (string text in values) {
				(DenseTensor<long> inputIds, DenseTensor<long> Mask) tokens = Tokenize(text);

				// Prepare
				List<NamedOnnxValue> inputs = [
					NamedOnnxValue.CreateFromTensor(_inputIds, tokens.inputIds),
					NamedOnnxValue.CreateFromTensor(_attentionMask, tokens.Mask)
				];

				// Inference
				using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> output = _session.Run(inputs);
				float[] vector = output[0].AsEnumerable<float>().ToArray();
				results.Add(new Embedding<float>(vector));
			}

			return [.. results];
		}

		private (DenseTensor<long> inputIds, DenseTensor<long> Mask) Tokenize(string text) {
			string normalized = text.Normalize(NormalizationForm.FormKC);
			normalized = normalized.ToLowerInvariant();

			List<string> words =
				WhiteSpaceAndPunctuationRegex()
					.Matches(normalized)
					.Select(m => m.Value)
					.ToList();

			List<int> tokenIds = [_clsId];

			foreach (string word in words) {
				int start = 0;
				bool isBad = false;
				List<int> subTokensForWord = [];

				while (start < word.Length) {
					int end = word.Length;
					string? curSubstr = null;
					int foundId = -1;

					while (end > start) {
						string piece = word.Substring(start, end - start);
						if (start > 0)
							piece = _poundItTwice + piece;
						if (_vocabulary.TryGetValue(piece, out foundId)) {
							curSubstr = piece;
							break;
						}
						end--;
					}

					if (curSubstr == null) {
						isBad = true;
						break;
					}

					subTokensForWord.Add(foundId);
					start = end;
				}

				if (isBad) {
					tokenIds.Add(_unkId);
				} else {
					tokenIds.AddRange(subTokensForWord);
				}
			}

			tokenIds.Add(_sepId);
			if (tokenIds.Count > _maxTokenLength) {
				// simple truncation (keep CLS at start and SEP at end)
				tokenIds = tokenIds.Take(_maxTokenLength - 1).Concat([_sepId]).ToList();
			}

			// Build DenseTensor<long> with shape [1, maxLen]
			DenseTensor<long> inputIds = new([1, _maxTokenLength]);
			DenseTensor<long> mask = new([1, _maxTokenLength]);

			// Fill with padId and 0 mask
			for (int i = 0; i < _maxTokenLength; i++) {
				inputIds[0, i] = _padId;
				mask[0, i] = 0;
			}

			// Copy tokens and set mask
			for (int i = 0; i < tokenIds.Count && i < _maxTokenLength; i++) {
				inputIds[0, i] = tokenIds[i];
				mask[0, i] = 1;
			}

			return (inputIds, mask);
		}

		public void Dispose() => _session.Dispose();
		public object? GetService(Type serviceType, object? serviceKey = null) => null;
	}
}
