using Microsoft.Extensions.AI;
namespace OLLM.Utility.ModelSpecific {
	using System.Text;
	using static Constants;
	internal static class QwenCoder {
		private const bool _add_generation_prompt = true;
		public static string AsFormattedString(string? userPrompt) {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}
			const string constructedRootSystemPrompt = $"{_defaultInstruction}{_art}{_algorithms}{_specificity}";
			List<ChatMessage> messages = [
				new (ChatRole.System, constructedRootSystemPrompt),
				new (ChatRole.User, userPrompt)
			];
			StringBuilder sb = new();
			foreach (ChatMessage message in messages) {
				if (messages[0].Role == ChatRole.System) {
					sb.Append($"{_imStart}{_system}");
					sb.AppendLine(_firstTextContentOfChatMessageContents(messages[0])?.ToString());
				} else {
					sb.AppendLine($"{_imStart}{_system}");
					sb.AppendLine(constructedRootSystemPrompt);
				}
				sb.AppendLine(_imEnd);
				sb.AppendLine();
			}
			#region TODO Unused
			//int index = 0;
			//int messagesCount = messages.Count;
			//foreach (ChatMessage message in messages) {
			//	if (((message.Role == _user) || (message.Role == _system && index != 0)) ||
			//		(message.Role == _assistant && !message.)) {
			//		sb.AppendLine($"{_imStart}" + message.Role);
			//		sb.AppendLine(message.content);
			//		sb.AppendLine(_imEnd);
			//	} else if (message.Role == _assistant) {
			//		sb.AppendLine($"{_imStart}" + message.Role);
			//		if (!string.IsNullOrEmpty(message.content)) {
			//			sb.AppendLine(message.content);
			//		}
			//		foreach (dynamic? toolCall in message.tool_calls) {
			//			if (toolCall.function != null) {
			//				// {%- if tool_call.function is defined %}
			//				// {%- set tool_call = tool_call.function %}
			//				// {% -endif %}
			//				// {{- '\n<tool_call>\n{"name": "' }}
			//				// {{- tool_call.name }}
			//				// {{- '", "arguments": ' }}
			//				// {{- tool_call.arguments | tojson }}{{- '}\n</tool_call>' }}
			//			}
			//		}
			//		sb.AppendLine(_imEnd);
			//	} else if (message.role == _tool) {
			//		if (index == 0 || messages[index - 1].Role != ChatRole.Tool) {
			//			sb.AppendLine($"{_imStart}{_user}");
			//		}
			//		sb.AppendLine(_toolResponseStart);
			//		sb.AppendLine();
			//		sb.AppendLine(message.content);
			//		sb.AppendLine(_toolResponseEnd);
			//		if ((index == messagesCount - 1) ||
			//			(index + 1 < messagesCount) && (messages[index + 1].Role != ChatRole.Tool)) {
			//			sb.AppendLine(_imEnd);
			//		}
			//	}
			//	index++;
			//}
			#endregion
			if (_add_generation_prompt) {
				sb.AppendLine($"{_imStart}{_assistant}");
			}
			return sb.ToString();
		}
	}
}