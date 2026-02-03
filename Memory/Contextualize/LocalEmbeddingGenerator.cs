using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Windows;

namespace UI.Memory.Contextualize {
	internal sealed class LocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>> {
		internal const string _inputIds = "input_ids";
		internal const string _attentionMask = "attention_mask";

		private readonly InferenceSession _session;
		internal LocalEmbeddingGenerator(string embedModelPath) {
			// Optimization: Use SessionOptions to enable GPU if available
			SessionOptions options = new();
			try {
				options.AppendExecutionProvider_DML();
			} catch (NotSupportedException) {
				MessageBox.Show("DML execution provider is unavailable. Attempting to use CUDA.");
				try {
					options.AppendExecutionProvider_CUDA();
				} catch (Exception) {
					MessageBox.Show("CUDA execution provider is unavailable. Falling back to use CPU");
					options.AppendExecutionProvider_CPU();
				}
			} catch (Exception) {
				options.AppendExecutionProvider_CPU();
			}

			_session = new InferenceSession(embedModelPath, options);
		}

		internal async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
			IEnumerable<string> values,
			EmbeddingGenerationOptions? options = null,
			CancellationToken cancellationToken = default) {

			List<Embedding<float>> results = [];

			foreach (string text in values) {
				// Tokenize (TODO: use Microsoft.ML.Tokenizers)
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

		// Nomic typically requires a specific prefix (e.g., search_document: )
		private (DenseTensor<long> inputIds, DenseTensor<long> Mask) Tokenize(string text) {
			// TODO Use Microsoft.ML.Tokenizers with the nomic vocab file here
			throw new NotImplementedException("Integrate a BERT-compatible tokenizer.");
		}

		public void Dispose() => _session.Dispose();
		public object? GetService(Type serviceType, object? serviceKey = null) => null;

		// TODO refactor this
		Task<GeneratedEmbeddings<Embedding<float>>> IEmbeddingGenerator<string, Embedding<float>>.GenerateAsync(
			IEnumerable<string> values,
			EmbeddingGenerationOptions? options,
			CancellationToken cancellationToken) => GenerateAsync(values, options, cancellationToken);
	}
}
