namespace JinjaToCSharp;

using AbstractSyntaxTree;
using LexicalAndParsingModels.Jinja;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Constants;

/// <summary>
/// Provides utility for generating C# source code from Jinja 'chat_template' definition found in a model tokenizer configuration file.
/// You should only have to call Converter.WriteOutput("/path/to/my/model/tokenizer_config.json") and it will output C# code that you can use.
/// </summary>
/// <remarks>
/// The intention is to programmatically dictate the flow of communication with LLM models.
/// </remarks>
public static partial class Converter {
	const string _tokenizerConfigJson = "tokenizer_config.json";
	const string _chatTemplateKey = "chat_template";

	/// <summary>
	/// Generates C# source code from the Jinja 'chat_template' from a model's tokenizer_config JSON.
	/// </summary>
	/// <remarks>
	/// The generated C# code is created in the current working directory a name resembling the model itself e.g.: 'Phi-4.cs', 'Qwen-2-5-Coder.cs', etc.
	/// </remarks>
	/// <param name="tokenizerConfigJsonPath">
	/// The full path to the tokenizer configuration JSON file containing the chat template. Cannot be null or empty.
	/// </param>
	public static void WriteOutput(string tokenizerConfigJsonPath) {
		if (string.IsNullOrEmpty(tokenizerConfigJsonPath)) {
			ArgumentNullException ane = new(nameof(tokenizerConfigJsonPath),
				$"Missing path to {_tokenizerConfigJson}") {
				HelpLink = _exceptionHelpLinkString,
				HResult = 0,
				Source = null
			};

			throw ane;
		}

		// Get the 'chat_template' from the provided 'tokenizer_config.json' and the name of the directory that it is in
		(string jinjaStringFromChatTemplate, string directoryName) = ReadChatTemplateFromTokenizerConfig(tokenizerConfigJsonPath);

		// e.g.: "/Ministral-3-14B-2512/tokenizer_config.json"
		// the output will be Ministral314B2512.cs (you should probably rename it afterward)
		string className = _alphanumericHyphensAndUnderscores().Replace(directoryName, string.Empty);
		string outputName = $"{className}.cs";

		// The Parser's Lexer tokenizes the input Jinja string in order to build the AST (abstract syntax tree)
		Parser parser = new(jinjaStringFromChatTemplate);
		RootNode abstractSyntaxTree = parser.ParseTemplate();

		// Use the AST to generate C# code representative of the Jinja template logic
		GenerateCode codeGenerator = new();
		string cs = codeGenerator.Generate(abstractSyntaxTree, className: className);
		string outPath = Path.Combine(Environment.CurrentDirectory, outputName);

		CreateCSharpDocument(cs, outPath);
	}

	private static void CreateCSharpDocument(string cs, string outPath) {
		File.WriteAllText(outPath, cs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		Console.Error.WriteLine($"Wrote: {outPath}");
	}

	/// <summary>
	/// Reads the 'chat_template' string from a 'tokenizer_config.json'.
	/// </summary>
	/// <param name="tokenizerConfigPath">
	/// The full path to the tokenizer configuration file (typically named 'tokenizer_config.json').
	/// The file must exist and be a valid JSON file containing a 'chat_template' property.
	/// </param>
	/// <returns>The value of the 'chat_template' property as a string.</returns>
	/// <exception cref="FileNotFoundException">Thrown if the file specified by tokenizerConfigPath does not exist.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the configuration file does not contain a 'chat_template' property, or if the 'chat_template' property is not a string. </exception>
	private static (string, string) ReadChatTemplateFromTokenizerConfig(string tokenizerConfigPath) {
		if (!File.Exists(tokenizerConfigPath)) {
			throw new FileNotFoundException($"{_tokenizerConfigJson} not found at the specified location");
		}

		using FileStream stream = File.OpenRead(tokenizerConfigPath);
		using JsonDocument doc = JsonDocument.Parse(stream);

		// e.g.: 'Phi-4', 'Ministral-3-14B-2512', 'Qwen2.5-Coder-3B-Instruct', etc.
		string parentDirectoryName = Directory.GetParent(tokenizerConfigPath.TrimEnd(_tokenizerConfigJson).ToString())?.Name ?? "GeneratedTemplate";

		return
			!doc.RootElement.TryGetProperty(_chatTemplateKey, out JsonElement chatTemplateProp)
				? throw new InvalidOperationException($"{_tokenizerConfigJson} does not contain a '{_chatTemplateKey}' property")
					: chatTemplateProp.ValueKind != JsonValueKind.String
						? throw new InvalidOperationException($"'{_chatTemplateKey}' exists but it is not a string. Unable to parse it.")
							: (chatTemplateProp.GetString()!, parentDirectoryName);
	}

	[GeneratedRegex(@"[^a-zA-Z0-9_-]")]
	private static partial Regex _alphanumericHyphensAndUnderscores();
}