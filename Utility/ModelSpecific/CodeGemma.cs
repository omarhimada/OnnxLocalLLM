namespace OLLM.Utility.ModelSpecific {
	using Microsoft.Extensions.AI;
	using System.Text;
	using static Constants;
	internal static class CodeGemma {
		// Not included in current Microsoft.Extensions.AI - CodeGemma specific ('model', not 'assistant')
		internal static readonly ChatRole _modelChatRole = new(_model);
		private const bool _addGenerationPrompt = true;
		/// <summary>
		/// rank_0_codegemma-7b-it_decoder_merged_model_fp16.onnx
		/// </summary>
		public static string AsFormattedString(string? userPrompt) {
			if (string.IsNullOrEmpty(userPrompt)) {
				return string.Empty;
			}
			const string constructedRootSystemPrompt = $"{_defaultInstruction}{_art}{_algorithms}";
			const string constructedSystemPrompt = $"{constructedRootSystemPrompt}";
			List<ChatMessage> messages = [
				new(_modelChatRole, $"{constructedSystemPrompt}"),
				new(ChatRole.User, $"{userPrompt.Trim()}")
			];
			StringBuilder sb = new();
			sb.AppendLine(_bos);
			int index = 0;
			foreach (ChatMessage message in messages) {
				if ((message.Role == ChatRole.User) == (index % 2 == 0)) {
					// {{ raise_exception('Conversation roles must alternate user/model/user/model/...') }}
				}
				string toTrim = $"{_startOfTurn}{message.Role}{Environment.NewLine}{message.Text}".Trim();
				sb.Append(toTrim);
				sb.Append(_endOfTurn);
				sb.AppendLine();
				index++;
			}
			if (_addGenerationPrompt) {
				sb.Append($"{_startOfTurn}{_model}");
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
}