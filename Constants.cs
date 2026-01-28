namespace UI {
	internal static class Constants {
		// nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4
		internal static string DebugModelPath => $"{AppContext.BaseDirectory}..\\..\\..\\Mistral-7B";

		internal const string _mistral3TokenStartTurn = @"<s>";
		internal const string _mistral3TokenStop = @"</s>";
		internal const string _mistral3InstructStart = @"[INST]";
		internal const string _mistral3InstructEnd = @"[/INST]";

		//internal const string _n = @"\n";
		internal const string _ws = @" ";

		internal const string _mistral3DefaultInstruction = "You are a helpful assistant.";
		internal const string _userFriendlyErrorResponse = "I'm sorry, something went wrong. I cannot respond.";
		internal const string _userFriendlyStoppedResponse = "(Stopped.)";

		internal const string _twoNewLinesVerbatimNoReturn = @"\n\n";

		internal const string _userFriendlyModelDirectoryErrorResponse =
			"Model files not found. Ensure that the model files exist at the specified: ";

		internal const string _appContextSwitchForSelectionBrush =
			"Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering";
	}
}