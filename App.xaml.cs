using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Text;
using System.Windows;
using UI.Memory.Contextualize;

namespace UI {
	using static Constants;

	public partial class App : Application {
		private readonly Model? _model;
		//private readonly Model? _embedModel; TODO
		private readonly Tokenizer? _tokenizer;
		private readonly GeneratorParams? _generatorParams;
		private readonly LocalEmbeddingGenerator _localEmbeddingGenerator;

		public App() {
			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);

			// Show loading screen while model attempts to load into memory
			LoadingWindow splash = new();
			splash.Show();
			splash.Activate();

			try {
				// (Mistral-7B or Mistral-14B) and nomic-embed-text-1-5
				(string? modelPath, string? embedModelPath) = EnsureRequiredModelsArePresent();

				if (modelPath == null || embedModelPath == null) {
					// Previous method already displayed the friendly user message and provided some guidance.
					Current.Shutdown();
					return;
				}

				// Embed model

				// Initialize embedding generator locally
				_localEmbeddingGenerator = new(embedModelPath);

				#region Loading: 2-3 seconds of loading the model into RAM before the window appears...
				Config config = new(modelPath);
				//config.AppendProvider("dml");

				// ~ 5.01 seconds
				_model = new(config);

				// ~ 0.0508 seconds
				_tokenizer = new(_model);

				// ~ 0.0002 seconds
				_generatorParams = new(_model);

				// TODO config option constructor for 'codeMode'  
				MainWindow mainWindow = new(_model, _localEmbeddingGenerator, _tokenizer, _generatorParams);
				#endregion

				mainWindow.Show();
			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			} finally {
				splash.Hide();
			}
		}

		/// <summary>
		/// Checks whether all required models (Mistral3 and nomic-embed-text) are present and accessible.
		/// </summary>
		private static (string? modelPath, string? embedModelPath) EnsureRequiredModelsArePresent() {
			string? modelPathToReturn = null;

			// Construct a detailed message to show the user if neither of the required models could be found.
			StringBuilder potentialFriendlyUserErrorMessage = new();

			// Attempt to retrieve the Mistral model ONNX
			if (!TryRequiredModelIsPresent(_debugModeModelPath, out string? modelPathToUse) && modelPathToUse == null) {
				potentialFriendlyUserErrorMessage.AppendLine($"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{modelPathToUse}");
			}

			// Attempt to retrieve the embedding model ONNX
			if (!TryRequiredModelIsPresent(_debugModeEmbedModelPath, out string? embedModelPathToUse) || embedModelPathToUse == null) {
				potentialFriendlyUserErrorMessage.AppendLine($"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{embedModelPathToUse}");
			}

			if (modelPathToReturn == null && embedModelPathToUse == null) {
				MessageBox.Show(potentialFriendlyUserErrorMessage.ToString(), "Please refer to the README.md or post an issue at https://github.com/OnnxLocalLLM");
			}

			return (modelPathToUse!, embedModelPathToUse!);
		}

		/// <summary>
		/// Checks whether the required model directories are present at either the specified debug path or its published location.
		/// </summary>
		private static bool TryRequiredModelIsPresent(string debugPath, out string? pathToUse) {
			pathToUse = null;
			if (debugPath == _debugModeEmbedModelPath) {
				#region Verify embed model is present 
				if (!File.Exists(debugPath)) {
					// Try the embed model from the published directory instead of the debugging directory:
					pathToUse = debugPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();
					if (!File.Exists(pathToUse)) {
						// If we still cannot find the required model(s) then show the user a friendly message and return false.
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{pathToUse}");
						return false;
					}
				} else {
					pathToUse = debugPath;
				}
				#endregion
			} else {
				#region Verify LLM is present first assume the user is debugging the application first
				if (!Directory.Exists(debugPath)) {
					// Assume the program is published and the directory is close to the executable.
					string publishedLocation = debugPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();

					if (!Directory.Exists(publishedLocation)) {
						// If we still cannot find the required model(s) then show the user a friendly message and return false.
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{publishedLocation}");
						return false;
					}
				} else {
					pathToUse = debugPath;
				}
				#endregion
			}
			return pathToUse != null;
		}
	}
}

// Expects genai_config.json, model.onnx, model.onnx_data, special_tokens_map.json, and tokenizer_config.json
// See huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main/
