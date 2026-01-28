using Microsoft.Extensions.AI;
using System.Text;
using static UI.Constants;

namespace UI.Utility {
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

			StringBuilder prompt = new(_mistral3TokenStartTurn);

			// TODO
			//bool isFirstUserMessage = true;

			foreach (ChatMessage msg in messages) {
				// For Mistral3 assume the instruction [INST] is the 'system' prompt.

				if (msg.Role == ChatRole.System) {
					prompt.Append($"{_mistral3TokenStartTurn}{_mistral3InstructStart}{_ws}{msg.Text}{_ws}");
				}

				if (msg.Role == ChatRole.User) {
					prompt.Append($"{msg.Text}{_ws}{_mistral3InstructEnd}");

					// TODO chat history and contextualize 'first' vs non-first user messages
					// Per Mistral docs, prepend system prompt to the FIRST user message
					//if (isFirstUserMessage && !string.IsNullOrEmpty(systemPrompt)) {
					//	content = $"{systemPrompt}\n\n{content}";
					//	isFirstUserMessage = false;
					//}

					// Space after [INST] and before [/INST]

				}
				//else if (msg.Role == ChatRole.Assistant) {
				//	// Note: Add a space before assistant content and close with EOS </s>
				//	prompt.Append($" {msg.Text} {_mistral3TokenStop}");
				//}
			}

			// The model is now primed to respond after the last [/INST]
			return prompt.ToString();
		}
	}
}
