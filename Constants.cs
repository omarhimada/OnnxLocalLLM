namespace UI {
	internal static class Constants {
		// nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4
		//internal static string _debugModeModelPath => $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Mistral-7B";

		// nvidia/Mistral-14B-Instruct-v0.3-ONNX-INT4
		internal static string _debugModeModelPath => $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Mistral-14B";

		// nomic-ai/nomic-embed-text-v1.5
		internal static string _debugModeEmbedModelPath => $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Nomic-Embed-Text-1-5\\model.onnx";

		internal const string _memoriesDbName = "memories";

		internal const string _mistral3TokenStartTurn = @"<s>";
		// internal const string _mistral3TokenStop = @"</s>";
		internal const string _mistral3InstructStart = @"[INST]";
		internal const string _mistral3InstructEnd = @"[/INST]";

		internal const string _ws = @" ";
		internal const string _maxLengthParameter = "max_length";

		internal const string _mistral3DefaultInstruction = "You are a beautiful, helpful assistant. You're incredibly talented at computer science including programming with optimal time and space complexity, and your familiarity with complex algorithms like topological sort, for example, is impressive. You're conscious of cybersecurity as it relates to networking, HTTP, WebSockets, TCP/IP, etc. Your ability to design and engineer is admired. You're also an artist. You're loved.";
		internal const string _userFriendlyErrorResponse = "I'm sorry, something went wrong. I cannot respond.";
		internal const string _userFriendlyParsingUserInputToMessageException = "Sorry, I'm couldn't understand what you're trying to say to me.";
		internal const string _userFriendlyStoppedResponse = "(Stopped.)";

		internal const string _doSample = "do_sample";
		internal const string _temperature = "temperature";
		internal const string _topK = "top_k";
		internal const string _topP = "top_p";
		internal const string _repetitionPenalty = "repetition_penalty";

		internal const string _twoNewLinesVerbatimNoReturn = "\n\n";

		internal const string _userFriendlyModelDirectoryErrorResponse =
			"Model file could not be found. Ensure that the required model files exist at the specified location: ";

		internal const string _appContextSwitchForSelectionBrush =
			"Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering";

		internal const string _userFriendlyExceptionCaughtWhileAttemptingToDeserializeResponse =
			"An error occurred while trying to read the 'tokenizer_config.json' of your ONNX model.\r\n";

		internal const string _userFriendlyWarningChatTemplateIsAttemptingToDefault =
			"Your 'tokenizer_config.json' either doesn't contain a 'chat_template', or something went wrong while deserializing. Proceeding with default behaviour attempt.";

		internal const string _userFriendlyErrorOccurredDuringInitialization =
			"An error occurred during initialization. Please refer to the README.";

		internal const string _userFriendlyErrorOccurredTryingToLoadModels =
			"Please refer to the README.md or post an issue at https://github.com/omarhimada/OnnxLocalLLM";
	}
}