using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;
using static OLLM.Constants;

namespace OLLM.State {
	internal class ModelState {
		#region Fields, properties, expressions
		internal InferenceSession? Session { get; set; }

		internal Model? Model;
		internal GeneratorParams? GeneratorParams;
		internal Tokenizer? Tokenizer;
		internal Generator? Generator;

		internal string? ModelDirectory { get; set; }

		internal bool ExpectingCodeResponse = false;
		internal float _getTemperature() => ExpectingCodeResponse ? 0.225f : 0.7f;
		#endregion

		internal ModelState(string modelDirectory) {
			ModelDirectory = modelDirectory;
			SessionOptions modelInferenceSessionOptions = new();


			#region Try DML firstm then CUDA, and finally fallback to CPU
			try {
				modelInferenceSessionOptions.AppendExecutionProvider_DML();
			} catch (NotSupportedException) {
				try {
					MessageBox.Show(_userFriendlyErrorOccurredLoadingDMLProvider);
					modelInferenceSessionOptions.AppendExecutionProvider_CUDA();
				} catch (Exception) {
					MessageBox.Show(_userFriendlyErrorOccurredLoadingCUDAProvider);
					modelInferenceSessionOptions.AppendExecutionProvider_CPU();
				}
			}
			#endregion

			#region Point to the direct ONNX model itself to instantiate the inference session
			string? modelFilePath = Directory.GetFiles(modelDirectory, _onnxSearch).FirstOrDefault();
			if (string.IsNullOrEmpty(modelFilePath)) {
				MessageBox.Show(_userFriendlyErrorOccurredDuringInitialization);
				Application.Current.Shutdown();
			}
			#endregion

			Session = new InferenceSession(modelFilePath, modelInferenceSessionOptions);

			Config config = new(ModelDirectory);
			Model = new(config);
			Tokenizer = new(Model);
			GeneratorParams = new(Model);
		}

		/// <summary>
		/// Re-initialize the generator after each response as opposed to before your next input is tokenized.
		/// (i.e.: user reads initial output of the model and then by the time they comprehend, the generator is re-initialized)
		/// </summary>
		internal void RefreshGenerator() {
			Generator?.Dispose();
			try {
				Generator = new(Model, GeneratorParams);
			} catch (Exception exception) {
				if (exception.InnerException != null) {
					Console.WriteLine(Environment.NewLine);
					Console.WriteLine(exception.InnerException!.Message);
				}
			}
		}

		internal void SetGeneratorParameterSearchOptions() {
			#region Set generator parameters
			//GeneratorParams?.SetSearchOption(_maxLengthParameter, 8192);
			//GeneratorParams?.SetSearchOption(_doSample, false);
			GeneratorParams?.SetSearchOption(_temperature, _getTemperature());
			GeneratorParams?.SetSearchOption(_topK, 51);
			GeneratorParams?.SetSearchOption(_topP, 0.9f);
			GeneratorParams?.SetSearchOption(_repetitionPenalty, 1.12f);
			#endregion
		}
	}
}
