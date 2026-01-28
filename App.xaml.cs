using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;

namespace UI {
	using static Constants;
	public partial class App : Application {
		public App() {
			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);
		}

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			// Show loading screen while model attempts to load into memory
			LoadingWindow splash = new();
			splash.Show();
			splash.Activate();

			try {
				if (!Directory.Exists(DebugModelPath)) {
					MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{DebugModelPath}");
					return;
				}

				#region Loading: 2-3 seconds of loading the model into RAM before the window appears...
				using Config config = new(DebugModelPath);
				using Model model = new(config);
				using Tokenizer tokenizer = new(model);

				using OnnxRuntimeGenAIChatClient onnxChatClient = new(model);

				// TODO config option constructor for 'codeMode'  
				MainWindow mainWindow = new(onnxChatClient, tokenizer);
				#endregion

				mainWindow.Show();
			} catch (Exception) {
				MessageBox.Show(_userFriendlyErrorOccurredDuringInitialization);
				Shutdown();
			} finally {
				splash.Hide();
			}
		}
	}
}

// Expects genai_config.json, model.onnx, model.onnx_data, special_tokens_map.json, and tokenizer_config.json
// See huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main/
