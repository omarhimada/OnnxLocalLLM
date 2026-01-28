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

			if (!Directory.Exists(DebugModelPath)) {
				MessageBox.Show($"Model files not found. Ensure that the model files exist at the specified: {DebugModelPath}");
				return;
			}

			using Config config = new(DebugModelPath);
			using Model model = new(config);

			using OnnxRuntimeGenAIChatClient onnxChatClient = new(model);

			// TODO config option constructor for 'codeMode'  
			MainWindow mainWindow = new(onnxChatClient);
			mainWindow.Show();
		}
	}
}




// Expects genai_config.json, model.onnx, model.onnx_data, special_tokens_map.json, and tokenizer_config.json
// See huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main/
