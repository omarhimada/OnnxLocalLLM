using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;

namespace UI {
	using static Constants;
	public partial class App : Application {
		private readonly Model? _model;
		private readonly Tokenizer? _tokenizer;
		private readonly GeneratorParams? _generatorParams;
		private readonly Generator? _generator;

		public App() {
			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);

			// Show loading screen while model attempts to load into memory
			LoadingWindow splash = new();
			splash.Show();
			splash.Activate();

			string modelPath = DebugModelPath;

			try {
				if (!Directory.Exists(DebugModelPath)) {
					// Published
					modelPath = DebugModelPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();

					if (!Directory.Exists(modelPath)) {
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{modelPath}");
						return;
					}
				}

				#region Loading: 2-3 seconds of loading the model into RAM before the window appears...
				Config config = new(modelPath);
				//config.AppendProvider(_dml);

				// ~ 5.01 seconds
				_model = new(config);

				// ~ 0.0508 seconds
				_tokenizer = new(_model);

				// ~ 0.0002 seconds
				_generatorParams = new(_model);

				// TODO config option constructor for 'codeMode'  
				MainWindow mainWindow = new(_model, _tokenizer, _generatorParams);
				#endregion

				mainWindow.Show();
			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			} finally {
				splash.Hide();
			}
		}
	}
}

// Expects genai_config.json, model.onnx, model.onnx_data, special_tokens_map.json, and tokenizer_config.json
// See huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main/
