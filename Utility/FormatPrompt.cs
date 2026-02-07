using Microsoft.Extensions.AI;
using System.Text;
using static OLLM.Constants;

namespace OLLM.Utility {
	internal static class FormatPrompt {
		/// <summary>
		/// Formats a list of chat messages into a prompt string compatible with the Mistral V3 model input format.
		/// For example: nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4
		/// </summary>
		/// <param name="messages">The list of chat messages to include in the prompt.</param>
		/// <returns>A string containing the formatted prompt for the Mistral V3 model.</returns>
		public static string Mistral3(List<ChatMessage>? messages = null) {
			if (messages == null || messages.Count == 0) {
				return string.Empty;
			}

			StringBuilder prompt = new();

			foreach (ChatMessage msg in messages) {
				// For Mistral3 assume the instruction [INST] is the 'system' prompt.

				if (msg.Role == ChatRole.System) {
					prompt.Append($"{_mistral3TokenStartTurn}{_mistral3InstructStart}{_ws}{msg.Text}{_ws}{_ws}{_specificity}");
				}

				if (msg.Role == ChatRole.User) {
					prompt.Append(_twoNewLinesVerbatimNoReturn);
					prompt.Append($"{msg.Text}{_ws}{_mistral3InstructEnd}");
				}
			}

			// The model is now primed to respond after the last [/INST]
			return prompt.ToString();
		}
	}
}
