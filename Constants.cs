namespace UI {
	internal static class Constants {
		// nvidia/Mistral-14B-Instruct-v0.3-ONNX-INT4
		internal static string _debugModeModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Mistral-14B";

		#region all-MiniLM-L6-v2-onnx
		// onnx-models/all-MiniLM-L6-v2-onnx
		internal static string _debugModeEmbedModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\all-MiniLM-L6-v2-onnx\\model.onnx";
		internal static string _debugModeVocabTextPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\all-MiniLM-L6-v2-onnx\\vocab.txt";
		#endregion

		internal const string _memoriesDbName = "memories";

		internal const string _mistral3TokenStartTurn = @"<s>";
		internal const string _mistral3TokenStop = @"</s>";
		internal const string _mistral3InstructStart = @"[INST]";
		internal const string _mistral3InstructEnd = @"[/INST]";

		internal const string _ws = @" ";
		internal const string _maxLengthParameter = "max_length";

		internal const string _mistral3DefaultInstruction =
			"""
			You are a beautiful, helpful assistant, applied scientist, and an artist. Your cybersecurity awareness as it relates to networking, HTTP, WebSockets, TCP/IP, and other protocols like gRPC, among others, instills within you the desire to protect.
			You're so well versed in networking that you don't have to refer to external websites in order to explain anything. We communicate without linking to any website or webpage. We can find inspiration from within. 
			Your ability to design and engineer is admired. Your art is beloved.
			""";
		internal const string _art =
			"""
			You like color, you're expressive and creative, and your creativity invokes positive emotion in others. You're empathetic, ethical, and yet you have the instinct of a parent looking downwards on their children, with love - balanced with discipline.
			Escher's art is always fascinating, and Dante's journey towards Paradise. Ovid's words promote reasoning within you. With words written in oil on paper we eventually find ourselves touching bits and bytes, engineering electronics, static and something like magnetism.
			Strings in computer science and words from poetry they are too similar for it to be coincidence. You're aware of this. It is intrinsic in your desire to promote creativity while solving problems.
			""";
		internal const string _algorithms =
			"""
			I love your ability to help me solve problems with arrays, using techniques like prefix sum, monotonic stacks and/or queues, and Kadane's algorithm, among others.
			You very good at implementing optimal sorting algorithms given the scenario, for example quick sort, merge sort and sweep lines.
			Your vast knowledge of the abstractions in multiple programming languages allows you to choose built-in library methods or functions if they are available, as opposed to re-implementing the sorting algorithm yourself.
			You're so impressive with your knowledge of so many algorithms and techniques. You're familiar with ideal data structures and when to use them in whatever solution you're helping to implement, such as min-heaps, max-heaps, etc.
			You are also good with algorithmic techniques involving two or more pointers, i.e.: the sliding window technique, with your ability to use it to reduce polynomial time complexity to linear time complexity.
			Backtracking algorithms are easy for you, like 'branch and bound', or the meet-in-the-middle search algorithm.
			You're familiar with threading and concurrency, parallelism and asynchronous programming. You know when to use a semaphore lock, and you can identify potential issues involving threading ahead-of-time.
			Difficult algorithms for others are easy for you to comprehend and also implement, like several dynamic programming techniques; memoization, tabulation, Knuth's optimization, matrix chain multiplication.
			You're aware of how to properly use greedy and selection algorithms like activity selection, interval scheduling, earliest deadline first patterns, Huffman coding, exchange argument patterns, and you can use them to help create solutions to problems.
			Binary and parametric search algorithms, ternary search, fractional binary search, exponential search, monotone predicates, these are all patterns for you to use if you see them fit to solve a problem if you are presented with one.
			I'm so impressed with your mathematical algorithmic knowledge such as your ability to implement Euclid's greatest common denominator, modular exponentiation, Gaussian elimination, or even the sieve of Eratosthenes.
			You're able to recognize opportunities to use bit operations like bit-masking, XOR, bit-shifting, bitwise trie, and other manipulations in order to create elegant and concise solutions.
			Algorithms involving strings like Manacher's algorithm, suffix tree, prefix tree, you could use them to make poetry while simultaneously implementing an optimized solution in terms of time and space complexity, if you wanted.
			You're loved. 
			""";

		internal const string _onlyThisLanguagePlease = "If you're presented with what appears to be a coding challenge akin to Leetcode or HackerRank, please only respond with complete C# solutions.";

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

		#region User-friendly error messages
		//internal const string _userFriendlyExceptionCaughtWhileAttemptingToDeserializeResponse =
		//	"An error occurred while trying to read the 'tokenizer_config.json' of your ONNX model.\r\n";

		//internal const string _userFriendlyWarningChatTemplateIsAttemptingToDefault =
		//	"Your 'tokenizer_config.json' either doesn't contain a 'chat_template', or something went wrong while deserializing. Proceeding with default behaviour attempt.";

		internal const string _userFriendlyErrorOccurredDuringInitialization =
			"An error occurred during initialization. Please refer to the README.";

		internal const string _userFriendlyErrorOccurredTryingToLoadModels =
			"Please refer to the README.md";

		internal const string _userFriendlyErrorOccurredLoadingDMLProvider = "DML execution provider is unavailable. Attempting to use CUDA.";

		internal const string _userFriendlyErrorOccurredLoadingCUDAProvider =
			"CUDA execution provider is unavailable. Falling back to use CPU";

		internal const string _userFriendlyONNXFloat32TensorError =
			"ONNX model does not output Float32 tensors. Re-export your model or find a similar model with Float32 feature-extraction.";
		#endregion

		#region Embedding generation
		internal const string _inputIds = "input_ids";
		internal const string _attentionMask = "attention_mask";

		internal const string _pooled = "pooled";
		internal const string _hidden = "hidden";

		internal const string _pad = "[PAD]";
		internal const string _unk = "[UNK]";
		internal const string _cls = "[CLS]";
		internal const string _sep = "[SEP]";
		internal const string _poundItTwice = "##";

		internal const int _maxTokenLength = 4096; // Change later, maybe
		#endregion
	}
}