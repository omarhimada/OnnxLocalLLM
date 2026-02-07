using Microsoft.Extensions.AI;

namespace OLLM.Utility {
	internal static class ConstructMessages {
		public static string AsFormattedString(string? userPrompt, bool codeMode, string? instruction = $"{Constants._mistral3DefaultInstruction}{Constants._art}{Constants._algorithms}") {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}

			List<ChatMessage> messages = [
				new(ChatRole.System, instruction),
				new(ChatRole.User, userPrompt.Trim())
			];

			return FormatPrompt.Mistral3(messages, codeMode);
		}
	}
}
