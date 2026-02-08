using Microsoft.Extensions.AI;
using System.Text;
using static OLLM.Constants;

namespace OLLM.Utility {
	internal static class ConstructMessages {
		public static string AsFormattedString(string? userPrompt) {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}

			// (old until new one finishes downloading) mistral format
			List<ChatMessage> messages = [
				new(ChatRole.System, $"{_mistral3TokenStartTurn}{_mistral3InstructStart}{_ws}{_mistral3DefaultInstruction}{_art}{_algorithms}{_mistral3InstructEnd}"),
				new(ChatRole.User, $"{_twoNewLinesVerbatimNoReturn}{userPrompt.Trim()}")
			];

			return string.Join(string.Empty, messages);
		}
	}
}
