using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Management;
using System.Windows;
using static OLLM.Constants;
namespace OLLM.State;

internal class ModelState {
	#region Fields, properties, expressions
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
		Config config = new(ModelDirectory);
		config.AppendProvider(_dml);
		//config.ClearProviders();
		//#region Try CUDA first, fallback to CPU
		// Query the Win32_VideoController WMI class
		//const string _findAny = "SELECT * FROM Win32_VideoController"; // This will likely find your integrated graphics only
		//const string _findNvidia = "SELECT * FROM Win32_PnPEntity WHERE Service='nvlddmkm'";
		//ManagementObjectSearcher searcher = new(_findNvidia);
		//foreach (ManagementBaseObject? obj in searcher.Get()) {
		//	if (obj is null) {
		//		continue;
		//	}
		//	// 0x10DE is the registered PCI Vendor ID for NVIDIA
		//	string? pnpId = obj["PNPDeviceID"]?.ToString();
		//	bool isNvidia = pnpId?.Contains("VEN_10DE", StringComparison.OrdinalIgnoreCase) ?? false;
		//	if (isNvidia) {
		//		config.AppendProvider(_cuda);
		//	}
		//}
		//#endregion
		#region Point to the direct ONNX model itself to instantiate the inference session
		string? modelFilePath = Directory.GetFiles(modelDirectory, _onnxSearch).FirstOrDefault();
		if (string.IsNullOrEmpty(modelFilePath)) {
			MessageBox.Show(_userFriendlyErrorOccurredDuringInitialization);
			Application.Current.Shutdown();
		}
		#endregion
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
		Generator = new(Model, GeneratorParams);
	}
	internal void SetGeneratorParameterSearchOptions() {
		#region Set generator parameters
		GeneratorParams?.SetSearchOption(_maxLengthParameter, 8192);
		//GeneratorParams?.SetSearchOption(_maxNewLengthParameter, 1024);
		GeneratorParams?.SetSearchOption(_doSample, true);
		GeneratorParams?.SetSearchOption(_temperature, _getTemperature());
		GeneratorParams?.SetSearchOption(_topK, 51);
		GeneratorParams?.SetSearchOption(_topP, 0.9f);
		GeneratorParams?.SetSearchOption(_repetitionPenalty, 1.12f);
		#endregion
	}
}