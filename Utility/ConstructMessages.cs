using Microsoft.Extensions.AI;

namespace UI.Utility {
	internal static class ConstructMessages {
		public static string AsFormattedString(string? userPrompt, string? instruction = Constants._mistral3DefaultInstruction) {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}

			List<ChatMessage> messages = [
				new(ChatRole.System, instruction),
				new(ChatRole.User, userPrompt.Trim())
			];

			return FormatPrompt.Mistral3(messages);
		}
	}
}
