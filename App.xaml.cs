using Microsoft.ML.OnnxRuntimeGenAI;
using System.Windows;
using UI.Memory;
using static UI.Constants;
using static UI.Initialization.EnsureModelsArePresent;

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

				MainWindow mainWindow = new();
				mainWindow.Initialize(_model, _localEmbeddingGenerator, _tokenizer, _generatorParams);

				mainWindow.Show();

			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			}
		}

		/// <summary>
		/// Allow the main window's constructor to
		/// close the loading window after it has initialized its components and memory store.
		/// </summary>
		internal static void FinishedInitializing() {
			_splashWindow.Hide();
		}
	}
}