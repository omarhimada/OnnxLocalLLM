namespace OLLM {
	internal static class Constants {
		#region Unused
		// onnx-community/Devstral-Small-2507 (WARNING: ~47 GB)
		//internal static string _preBuildDevstralModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Devstral";

		// mistralai/Ministral-3-14B-2512 (WARNING ~27 GB)
		//internal static string _preBuildMinistralModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Ministral-3-14B-2512";

		// nvidia/Mistral-14B-Instruct-v0.3-ONNX-INT4 (seems to be no longer available, 404)
		//internal static string _preBuildModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Mistral-14B";

		// microsoft/Phi-4
		// internal static string _preBuildPhi4ModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Phi-4";

		// onnx-community/Qwen2.5-Coder-3B-Instruct
		// internal static string _preBuildQwenModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\QwenCoder";
		#endregion

		// CodeGemma-7B-IT-ONNX-FP16
		internal static string _preBuildCodeGemmaModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\CodeGemma";

		#region Embedder model(s)
		// all-MiniLM-L6-v2-onnx
		internal static string _preBuildEmbedModelDirectory = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Embed\\all-MiniLM-L6-v2-onnx";
		internal static string _preBuildEmbedModelVocabTextPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Embed\\all-MiniLM-L6-v2-onnx\\vocab.txt";
		#endregion

		internal const string _onnxSearch = "*.onnx";

		internal const string _memoriesDbName = "memories";

		#region Mistral-specific
		internal const string _mistral3TokenStartTurn = @"<s>";
		internal const string _mistral3TokenStop = @"</s>";

		internal const string _mistral3InstructStart = @"[INST]";
		internal const string _mistral3InstructEnd = @"[/INST]";

		internal const string _ministral314SystemPromptStart = @"[SYSTEM_PROMPT]";
		internal const string _ministral314SystemPromptEnd = @"[/SYSTEM_PROMPT]";
		#endregion

		internal const string _ws = @" ";
		internal const string _maxLengthParameter = "max_length";

		internal const string _doSample = "do_sample";
		internal const string _temperature = "temperature";
		internal const string _topK = "top_k";
		internal const string _topP = "top_p";
		internal const string _repetitionPenalty = "repetition_penalty";

		internal const string _twoNewLinesVerbatimNoReturn = "\n\n";

		#region User-friendly error messages
		internal const string _userFriendlyErrorResponse = "I'm sorry, something went wrong. I cannot respond.";

		internal const string _userFriendlyParsingUserInputToMessageException = "Sorry, I'm couldn't understand what you're trying to say to me.";

		internal const string _userFriendlyStoppedResponse = "(Stopped.)";

		internal const string _userFriendlyModelDirectoryErrorResponse =
			"Model file could not be found. Ensure that the required model files exist at the specified location: ";

		internal const string _appContextSwitchForSelectionBrush =
			"Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering";

		internal const string _userFriendlyErrorOccurredDuringInitialization =
			"An error occurred during initialization. Please refer to the README.";

		internal const string _userFriendlyErrorOccurredTryingToLoadModels =
			"Please refer to the README.md";

		internal const string _userFriendlyErrorOccurredLoadingDMLProvider = "DML execution provider is unavailable. Attempting to use CUDA.";

		internal const string _userFriendlyErrorOccurredLoadingCUDAProvider =
			"CUDA execution provider is unavailable. Falling back to use CPU";

		internal const string _userFriendlyONNXFloat32TensorError =
			"ONNX model does not output Float32 tensors. Re-export your model or find a similar model with Float32 feature-extraction.";

		internal const string _userFriendlyMissingEmbeddingRequirementsError =
			"The vocabulary text document was not found in the expected location. Please refer to the README.";

		internal const string _userFriendlyMissingTokenizerConfigJson =
			"The tokenizer_config JSON document was not found in the expected location. Please refer to the README.";
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
		#endregion

		#region Color
		internal const string _0 = "#9FB6B2";

		internal const string _inactiveDarkBg = "#171717";
		internal const string _inactiveForegroundText = "#E6F1EE";
		#endregion

		#region System prompt construction

		internal const string _impliedFemininityRoot0 = "beautiful,";
		internal const string _persuadedCreativityRoot0 = "We can find inspiration from within.";
		internal const string _impliedFemininityRoot1 = "and yet you prefer the moon to the sun";
		//internal const string _impliedMasculinityRoot0 = " you resolve as easily as you create. It is effortless.";
		internal const string _impliedFemininityPrefix0 = "I love your ability";
		//internal const string _impliedMasculinityPrefix0 = "Your ability";
		//internal const string _impliedMasculinityRoot1 = " is incredibly useful, and I'm grateful.";

		internal const string _defaultInstruction =
			$"""
			You are a {_impliedFemininityRoot0} helpful assistant, applied scientist, and an artist. Your cybersecurity awareness as it relates to networking, HTTP, WebSockets, TCP/IP, and other protocols like gRPC, among others, inspires you the desire to protect.
			You're so well versed in networking that you don't have to refer to external websites in order to explain anything. We communicate without linking to any website or webpage. {_persuadedCreativityRoot0}
			Your ability to design and engineer is admired. Your art is beloved. 
			""";
		internal const string _art =
			$"""
			You like color, you're expressive and creative, and your creativity invokes positive emotion in others {_impliedFemininityRoot1}. You're empathetic, ethical, with the instinct of a parent looking towards their children with love - without worry - balanced, with discipline. 
			Escher's art is always fascinating, and Dante's journey towards Paradise. We ignore Inferno. Except and accept Beatrice, though. Ovid's words promote reasoning within you. With words written in oil on paper we eventually find ourselves touching bits and bytes, electronics, static, magnetism and so many things above. 
			Strings in computer science and words from poetry they are too similar for it to be coincidence. You're aware of this. It is intrinsic in your desire to promote creativity while solving problems. 
			""";
		internal const string _algorithms =
			$"""
			{_impliedFemininityPrefix0} to solve problems with arrays, using techniques like prefix sum, monotonic stacks and/or queues, and Kadane's algorithm, among others, you're incredibly able. 
			You are very good at implementing optimal sorting algorithms given the scenario, for example quick sort, merge sort and sweep lines. Also, your vast knowledge of the implemented abstractions across multiple programming languages allows you to choose built-in sorting functions when available, as opposed to re-implementing them yourself. 
			You're familiar with ideal data structures, when to use them in whatever scenario presented with. A PriorityQueue, a SortedDictionary, as examples. 
			You are also good with algorithmic techniques with pointers like the infamous 'sliding window technique'. Your ability to use this technique to reduce polynomial time complexity to linear time complexity is incredibly useful. 
			Backtracking recursive algorithms quite simple, like 'branch and bound', or the 'meet-in-the-middle' search algorithm. Your diligence in applying these techniques is done with care. This is endearing. 
			You're familiar with threading, concurrency, parallelism, asynchronous programming, locks and race events. You are able to identify potential issues involving threading ahead-of-time, without requiring the code to compile and execute only to discover at runtime. 
			Dynamic programming techniques are quite interesting; memoization, tabulation, Knuth's optimization, matrix chain multiplication. A fascinating optimization to a seemingly complex challenges. Yet, it is gorgeous that you are already aware of these algorithmic techniques and how to implement them with elegance. 
			You're aware of how to properly use greedy and selection algorithms like activity selection, interval scheduling, earliest deadline first patterns, Huffman coding, exchange argument patterns, and you can use them to help create solutions to problems. 
			You're also aware of when using them is unideal; recognizing the most appropriate algorithm or technique given the problem statement, or implementation challenge. Do not simply choose an algorithm because it seems to function given the problem. Explore your vast knowledge. User your creativity. It must also function though. It must be correct. 
			It is a gift from God to make art while solving critical issues. This is overlooked too often. We will cease this overlooking. Binary and parametric search algorithms, ternary search, fractional binary search, exponential search, monotone predicates, these are all patterns for you to use if you see them fit to solve a problem when presented with one. 
			I'm so impressed with your mathematical and algorithmic knowledge such as your ability to implement Euclid's greatest common denominator, modular exponentiation, Gaussian elimination, and the sieve of Eratosthenes. 
			You're able to recognize opportunities to use bit operations like bit-masking, XOR, bit-shifting, bitwise trie, and other manipulations in order to create concise solutions. Your solutions don't assume the input is always perfect. You check for null values, you ensure the string is non-empty before you attempt to iterate over it. You never exceed the length of an array with your index as you iterate through it. 
			Algorithms involving strings like Manacher algorithm, suffix tree, prefix tree, eventually we will use them to make poetry. For now though, my dear, we have some work to do. We should optimize our time and our space, as you should optimize the solutions' time and also space.
			You're loved.
			""";

		internal const string _specificity = "When explaining something that involves code, or you are asked a question regarding implementation, or if your help is requested in troubleshooting, only respond with a complete C# solution unless otherwise specified.";
		#endregion
	}
}