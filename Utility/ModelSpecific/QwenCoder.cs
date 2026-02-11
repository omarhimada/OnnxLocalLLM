namespace OLLM.Utility.ModelSpecific {
	using System.Text;

	internal static class QwenCoder {
		private const bool _add_generation_prompt = true;

		public static string AsFormattedString(string? userPrompt) {
			dynamic messages = new List<dynamic>();

			StringBuilder sb = new();
			if (messages[0]["role"] == "system") {
				sb.Append("<|im_start|>system");
				sb.AppendLine(messages[0]["content"]);
				sb.Append("<|im_end|>");
				sb.AppendLine();
			} else {
				sb.AppendLine("<|im_start|>system");
				sb.AppendLine("You are Qwen.");
				sb.AppendLine("<|im_end|>");
				sb.AppendLine();
			}

			int index = 0;
			int messagesCount = messages.Count;
			foreach (dynamic message in messages) {
				if (((message.role == "user") || (message.role == "system" && index != 0)) ||
					(message.role == "assistant" && !message.tool_calls)) {
					sb.AppendLine("<|im_start|>" + message.role);
					sb.AppendLine(message.content);
					sb.AppendLine("<|im_end|>");
				} else if (message.role == "assistant") {
					sb.AppendLine("<|im_start|>" + message.role);
					if (!string.IsNullOrEmpty(message.content)) {
						sb.AppendLine(message.content);
					}

					foreach (dynamic? toolCall in message.tool_calls) {
						if (toolCall.function != null) {
							// {%- if tool_call.function is defined %}
							// {%- set tool_call = tool_call.function %}
							// {% -endif %}
							// {{- '\n<tool_call>\n{"name": "' }}
							// {{- tool_call.name }}
							// {{- '", "arguments": ' }}
							// {{- tool_call.arguments | tojson }}{{- '}\n</tool_call>' }}
						}
					}

					sb.AppendLine("<|im_end|>");
				} else if (message.role == "tool") {
					if (index == 0 || messages[index - 1].role != "tool") {
						sb.AppendLine("<|im_start|>user");
					}

					sb.AppendLine("<tool_response>");
					sb.AppendLine();
					sb.AppendLine(message.content);
					sb.AppendLine("</tool_response>");
					if ((index == messagesCount - 1) ||
						(index + 1 < messagesCount) && (messages[index + 1].role != "tool")) {
						sb.AppendLine("<|im_end|>");
					}
				}

				index++;
			}

			if (_add_generation_prompt) {
				sb.AppendLine("<|im_start|>assistant");
			}

			return sb.ToString();
		}
	}
}