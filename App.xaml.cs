using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;

namespace UI {
	using static Constants;
	public partial class App : Application {

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			if (!Directory.Exists(DebugModelPath)) {
				MessageBox.Show($"Model files not found. Ensure that the model files exist at the specified: {DebugModelPath}");
				return;
			}

			using Config config = new(DebugModelPath);
			using Model model = new(config);
			using OnnxRuntimeGenAIChatClient onnxChatClient = new(model);

			MainWindow mainWindow = new(onnxChatClient);
			mainWindow.Show();
		}
	}
}




// Expects genai_config.json, model.onnx, model.onnx_data, special_tokens_map.json, and tokenizer.json
// See huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/blob/main/
