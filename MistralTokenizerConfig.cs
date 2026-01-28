using System.Text.Json.Serialization;

namespace UI {
	/// <summary>
	/// Represents the configuration settings for a Mistral tokenizer, including token definitions, special token handling,
	/// and model-specific options - deserialized 'tokenizer_config.json'
	/// </summary>
	internal class TokenizerConfigMistral {
		[JsonPropertyName("add_bos_token")]
		public bool AddBosToken { get; set; }

		[JsonPropertyName("add_eos_token")]
		public bool AddEosToken { get; set; }

		[JsonPropertyName("add_prefix_space")]
		public bool AddPrefixSpace { get; set; }

		[JsonPropertyName("added_tokens_decoder")]
		public Dictionary<int, AddedToken> AddedTokensDecoder { get; init; } = [];

		[JsonPropertyName("bos_token")]
		public required string BosToken { get; set; }

		[JsonPropertyName("chat_template")]
		public required string ChatTemplate { get; set; }

		[JsonPropertyName("clean_up_tokenization_spaces")]
		public bool CleanUpTokenizationSpaces { get; set; }

		[JsonPropertyName("eos_token")]
		public required string EosToken { get; set; }

		[JsonPropertyName("legacy")]
		public bool Legacy { get; set; }

		[JsonPropertyName("model_max_length")]
		public required string ModelMaxLength { get; set; }

		[JsonPropertyName("pad_token")]
		public object? PadToken { get; set; }

		[JsonPropertyName("sp_model_kwargs")]
		public required SpModelKwargs SpModelKwargs { get; set; }

		[JsonPropertyName("spaces_between_special_tokens")]
		public bool SpacesBetweenSpecialTokens { get; set; }

		[JsonPropertyName("tokenizer_class")]
		public required string TokenizerClass { get; set; }

		[JsonPropertyName("unk_token")]
		public required string UnkToken { get; set; }

		[JsonPropertyName("use_default_system_prompt")]
		public bool UseDefaultSystemPrompt { get; set; }
	}

	internal class AddedToken {
		[JsonPropertyName("content")]
		public required string Content { get; set; }

		[JsonPropertyName("lstrip")]
		public bool Lstrip { get; set; }

		[JsonPropertyName("normalized")]
		public bool Normalized { get; set; }

		[JsonPropertyName("rstrip")]
		public bool Rstrip { get; set; }

		[JsonPropertyName("single_word")]
		public bool SingleWord { get; set; }

		[JsonPropertyName("special")]
		public bool Special { get; set; }
	}

	public class SpModelKwargs {
	}
}
