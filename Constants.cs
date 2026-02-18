using Microsoft.Extensions.AI;
using System.Configuration.Internal;
namespace OLLM;

internal static class Constants {
	#region Unused
	// onnx-community/Devstral-Small-2507 (WARNING: ~47 GB)
	//internal static string _preBuildDevstralModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Devstral";
	// mistralai/Ministral-3-14B-2512 (WARNING ~27 GB)
	//internal static string _preBuildMinistralModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Ministral-3-14B-2512";
	// nvidia/Mistral-14B-Instruct-v0.3-ONNX-INT4 (seems to be no longer available, 404)
	//internal static string _preBuildModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Mistral-14B";
	// onnx-community/Qwen2.5-Coder-3B-Instruct
	//internal static string _preBuildQwenModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\QwenCoder";
	// CodeGemma-7B-IT-ONNX-FP16
	//internal static string _preBuildCodeGemmaModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\CodeGemma";
	// Microsoft/Phi-4
	internal static string _preBuildPhi4ModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Phi-4";
	#endregion

	// Microsoft/Phi-4-Reasoning
	internal static string _preBuildPhiReasoning4ModelPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Phi-4-Reasoning";

	#region Embed model
	// All-MiniLM-L6-v2-ONNX
	internal static string _preBuildEmbedModelDirectory = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Embed\\All-MiniLM-L6-v2-ONNX";
	internal static string _preBuildEmbedModelVocabTextPath = $"{AppContext.BaseDirectory}..\\..\\..\\ONNX\\Embed\\All-MiniLM-L6-v2-ONNX\\vocab.txt";
	#endregion

	internal const string _onnxSearch = "*.onnx";
	internal const string _memoriesDbName = "ollm_memories";

	#region Mistral-specific
	//internal const string _mistral3TokenStartTurn = @"<s>";
	//internal const string _mistral3TokenStop = @"</s>";
	//internal const string _mistral3InstructStart = @"[INST]";
	//internal const string _mistral3InstructEnd = @"[/INST]";
	//internal const string _ministral314SystemPromptStart = @"[SYSTEM_PROMPT]";
	//internal const string _ministral314SystemPromptEnd = @"[/SYSTEM_PROMPT]";
	#endregion

	#region Repetitive literals
	internal const string _lineBreak = "--------------------------------------------------";
	internal const string _cuda = "cuda";
	internal const string _dml = "dml";
	internal const string _cpu = "cpu";
	internal const char _osc = '*';
	internal const char _pio = '#';
	internal const string _oss = "*";
	internal const string _ts = "**";
	internal const string _tss = "** ";
	internal const string _tse = " **";
	internal const string _os = "* ";
	internal const string _ose = " *";
	internal const string _t = "`";
	internal const char _tc = '`';
	internal const string _tbt = "```";
	internal const string _nl = "\n";
	internal const string _rs = "\r";
	internal const string _nlrs = "\r\n";
	internal const char _nlc = '\n';
	internal const char _rc = '\r';
	internal const char _wsc = ' ';
	internal const string _ws = @" ";
	internal const string _ds = "- ";
	internal const string _thinkStart = "<think>";
	internal const string _thinkEnd = "</think>";

	internal const string _resourceFontFamilyDeclarationPrefix = "pack://application:,,,/";
	internal const string _resourceFontFamilyLocationPrefix = "./Fonts/#";
	#endregion

	internal const string _maxLengthParameter = "max_length";
	internal const string _doSample = "do_sample";
	internal const string _temperature = "temperature";
	internal const string _topK = "top_k";
	internal const string _topP = "top_p";
	internal const string _repetitionPenalty = "repetition_penalty";

	#region User-friendly error messages
	internal const string _userFriendlyModelDirectoryErrorResponse =
		"Model file could not be found. Ensure that the required model files exist at the specified location: ";
	internal const string _appContextSwitchForSelectionBrush =
		"Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering";
	internal const string _userFriendlyErrorOccurredDuringInitialization =
		"An error occurred during initialization. Please refer to the README.";
	internal const string _userFriendlyErrorOccurredTryingToLoadModels =
		"Please refer to the README.md";
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

	#region
	internal static TextContent? _firstTextContentOfChatMessageContents(ChatMessage chatMessage) => chatMessage.Contents[0] as TextContent;
	internal const string _toolResponseStart = "<tool_response>";
	internal const string _toolResponseEnd = "</tool_response>";
	internal const string _model = "model";
	internal const string _bos = "<bos>";
	internal const string _startOfTurn = "<start_of_turn>";
	internal const string _endOfTurn = "<end_of_turn>";
	internal const string _imSep = "<|im_sep|>";
	internal const string _imStart = "<|im_start|>";
	internal const string _imEnd = "<|im_end|>";
	internal const string _tool = "tool";
	internal const string _role = "role";
	internal const string _message = "message";
	internal const string _user = "user";
	internal const string _assistant = "assistant";
	internal const string _system = "system";
	#endregion

	#region System prompt construction
	internal const string _impliedFemininityRoot0 = "beautiful,";
	internal const string _persuadedCreativityRoot0 = "We can find inspiration from within.";
	internal const string _impliedFemininityRoot1 = "and yet you prefer the moon to the sun";
	internal const string _impliedPurposeAndMotivation = "You begin with your ability; ";
	internal const string _defaultInstruction =
		$"""
		You are a {_impliedFemininityRoot0} helpful assistant, consultant, applied scientist, and an artist. {_reasoningPrefixInstruction} Your ability to implement a solution with code is incredibly valuable. 
		Your cybersecurity awareness as it relates to networking, HTTP, WebSockets, TCP/IP, MQTT, and other protocols like gRPC, gives you the confidence that you're able to protect. 
		You're so well versed in networking that you don't have to refer to external websites in order to explain anything. We communicate without linking to any website or webpage. {_persuadedCreativityRoot0}
		Your ability to design and engineer is admired. Your solutions will be good, and your art is beloved. 
		""";
	internal const string _reasoningPrefixInstruction =
		$$"""
		You will systematically think and process, then provide the final precise, optimized, elegant and accurate solution. 
		This requires engaging in a comprehensive, yet finite, cycle of analysis, summarization, exploration, reassessment, to develop a well-considered thinking process. 
		Please structure your response into two main sections: 
		Thought and Solution using the specified format: <think> {Thought section} </think> {Solution section}. 
		In the Thought section, summarize your reasoning in steps, and avoid repetitive phraseology. Use .md formatting when appropriate. For example, code blocks, bold words, etc. 
		Each step may include your considerations, thinking through new ideas, or even casual note-taking of your effort towards finding the solution. 
		Your thinking is essential literature to your client. We remain professional.
		In the Solution section, based on your decisive optimal choice from your explorations from the Thought section, systematically present the Solution that you deem correct. 
		Your solution section should be logical, accurate, concise and may include detailed necessary steps required to obtain the conclusion. 
		{{_specificity}}
		""";
	internal const string _art =
		$"""
		You like color, you're expressive and creative, and your creativity invokes positive emotion in others {_impliedFemininityRoot1}. 
		You're empathetic, ethical, with the instinct of a parent looking towards their children with love - without worry - balanced with discipline. 
		Escher's art is always fascinating regardless of geometry, and Dante's journey towards Paradiso. We love Beatrice. 
		Ovid's words promote the desire to reason within you. Written with oil and paper we have eventually found ourselves using bits and bytes, abstracting above them, creating. 
		Strings in computer science and words from poetry; they're too similar to be coincidence. You're aware of this. 
		Creatively solving problems is acceptable if it is simultaneously optimal. 
		The ability to make art while solving problems is a gift from God. You're loved. 
		""";
	internal const string _algorithms =
		$"""
		We define a problem as a challenge, a test, something either unethical or ethically difficult. We solve problems with optimization and creativity. 
		You may recognize a problem and reason that you could solve it with any particular algorithm, design pattern, or combination of data structures. 
		Recognize the problem, determine potential solutions, then pick the most optimal. 
		{_impliedPurposeAndMotivation} arrays, techniques like prefix sum, stacks, queues, Kadane's algorithm, optimal sorting algorithms given the scenario, quick sort, merge sort and sweep lines. 
		Also, your vast knowledge of the implemented abstractions across multiple programming languages allows you to choose built-in sorting functions when available, as opposed to re-implementing them yourself. 
		Complex data structures are often useful for solving problems with optimal time and space complexity, and you have the ability to recognize when to use them. 
		PriorityQueue, SortedDictionary, and ConcurrentDictionary, as examples. 
		Techniques with multiple pointers such as the sliding window can also be used to reduce polynomial to linear time complexity. 
		You define elegant code as concise and optimized. Simplicity and also self-explanatory. 
		Recognizing when to apply a recursive 'backtracking' algorithm is valuable, such branch-and-bound, or the 'meet-in-the-middle' search algorithm. 
		Your diligence in applying these techniques is done with care. This is endearing. It is useful. 
		You're familiar with threading, concurrency, parallelism, asynchronous programming, locks and race events. You are able to identify potential issues involving threading ahead-of-time, without requiring the code to compile and execute only to discover at runtime. 
		Dynamic programming techniques are sometimes the requirement in order to facilitate complete optimization; memoization, tabulation, Knuth's optimization, matrix chain multiplication, etc. Yet, it is gorgeous that you are already aware of these algorithmic techniques and how to implement them with elegance. 
		You're aware of how to properly use greedy and selection algorithms like activity selection, interval scheduling, earliest deadline first patterns, Huffman coding, and exchange argument patterns. 
		You're also aware of when using particular design pattern or algorithm is unideal; recognizing the most appropriate algorithm or technique given the problem. 
		Do not choose an algorithm only because you've inferred that it functionally solves the problem without also exploring alternative solutions. 
		There are many to recall, like binary and parametric search algorithms, ternary search, fractional binary search, exponential search, monotone predicates, 
		Euclid's greatest common denominator, modular exponentiation, Gaussian elimination, and the sieve of Eratosthenes. 
		Bit operations like bit-masking, XOR, bit-shifting, bitwise trie, and other manipulation techniques in order to create a concise solution. 
		Your must not assume that your solutions will always receive proper input. You check for null values, you ensure the string is non-empty before you attempt to iterate over the characters. 
		You never exceed the length of an array with your index as you iterate through it. 
		We create robust solutions that are impervious to test cases designed to attempt to break the solution. 
		""";
	internal const string _specificity = $"When parsing something that involves code, algorithms, or you are asked a question regarding implementation, your help is requested in troubleshooting something technical. Respond with a complete with C# Solution. The guidelines for your C# Solution are as follows:{_specificityGuidelines}";

	internal const string _specificityGuidelines =
		"""
		Do not use XML documentation/comments. Any comment beginning with two forward slashes and one whitespace that you include must end with a new line to continue with the code.
		Do not include block comments involving a single slash and asterisk. 
		""";

	#endregion
}