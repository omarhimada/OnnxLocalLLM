using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;

namespace UI {
	using static Constants;
	public partial class App : Application {
		private Model _model;
		private Tokenizer _tokenizer;
		private GeneratorParams _generatorParams;
		private Generator _generator;

		public App() {
			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);
		}

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

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

				_model = new(config);
				_tokenizer = new(_model);
				_generatorParams = new(_model);
				_generator = new(_model, _generatorParams);

				// TODO config option constructor for 'codeMode'  
				MainWindow mainWindow = new(_tokenizer, _generatorParams, _generator);
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
