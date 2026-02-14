using Microsoft.Extensions.AI;
using static OLLM.Constants;
namespace OLLM.Utility.ModelSpecific {
	internal static class Mistral {
		/// <summary>
		/// Ministral-3-14B-2512 formatting
		/// </summary>
		public static string AsFormattedString(string? userPrompt) {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}
			// (constructed system prompt)
			const string constructedRootSystemPrompt = $"{_defaultInstruction}{_art}{_algorithms}";
			// <s>[SYSTEM_PROMPT](constructed system prompt)[/SYSTEM_PROMPT]
			const string constructedSystemPrompt = $"{_mistral3TokenStartTurn}{_ministral314SystemPromptStart}{constructedRootSystemPrompt}{_ministral314SystemPromptEnd}";
			List<ChatMessage> messages = [
				new(ChatRole.System, $"{constructedSystemPrompt}"),
				new(ChatRole.User, $"{_mistral3InstructStart}{userPrompt.Trim()}{_mistral3InstructEnd}{_ws}")
			];
			// <s>[SYSTEM_PROMPT](constructed system prompt)[/SYSTEM_PROMPT][INST]something interesting, maybe[/INST] "
			return string.Join(string.Empty, messages);
		}
	}
}