using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Text;
using System.Windows;
using UI.Memory.Contextualize;
using static UI.Constants;

namespace UI {
	internal partial class App : Application {
		internal Model? _model;
		internal Tokenizer? _tokenizer;
		internal GeneratorParams? _generatorParams;
		internal LocalEmbeddingGenerator? _localEmbeddingGenerator;

		internal static readonly LoadingWindow _splashWindow = new();

		protected override async void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);

			// Show loading screen while model attempts to load into memory
			_splashWindow.Show();
			_splashWindow.Activate();

			try {
				// (Mistral-7B or Mistral-14B) and nomic-embed-text-1-5
				(string? modelPath, string? embedModelPath) = EnsureRequiredModelsArePresent();

				if (modelPath == null || embedModelPath == null) {
					// Previous method already displayed the friendly user message and provided some guidance.
					Current.Shutdown();
					return;
				}

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
				#endregion

				// TODO config option constructor for 'codeMode'  
				MainWindow mainWindow = new();
				await mainWindow.InitializeAsync(_model, _localEmbeddingGenerator, _tokenizer, _generatorParams);

				mainWindow.Show();

			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			} finally {
				//FinishedInitializing();
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
				potentialFriendlyUserErrorMessage.AppendLine(
					$"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{modelPathToUse}");
			}

			// Attempt to retrieve the embedding model ONNX
			if (!TryRequiredModelIsPresent(_debugModeEmbedModelPath, out string? embedModelPathToUse) ||
				embedModelPathToUse == null) {
				potentialFriendlyUserErrorMessage.AppendLine(
					$"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{embedModelPathToUse}");
			}

			if (modelPathToReturn == null && embedModelPathToUse == null) {
				MessageBox.Show(potentialFriendlyUserErrorMessage.ToString(),
					"Please refer to the README.md or post an issue at https://github.com/OnnxLocalLLM");
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

		/// <summary>
		/// Allow the main window's constructor to close the application's loading
		/// window after it has initialized its components and memory store,
		/// and show the main window at the same time.
		/// </summary>
		internal static void FinishedInitializing() {
			_splashWindow.Hide();
		}
	}
}