using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
using static OLLM.Constants;

namespace OLLM.Utility.ModelSpecific {
	internal static class Phi4 {
		private const bool _addGenerationPrompt = true;

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
				switch (message.Role.ToString()) {
					case _system:
						sb.Append($"{_imStart}{_system}{_imSep}{_firstTextContentOfChatMessageContents(message)}{_imEnd}");
						break;
					case _user:
						sb.Append($"{_imStart}{_user}{_imSep}{_firstTextContentOfChatMessageContents(message)}{_imEnd}{_imStart}{_assistant}{_imSep}");
						break;
					case _assistant:
						sb.Append($"{_firstTextContentOfChatMessageContents(message)}{_imEnd}");
						break;
				}
			}

			return sb.ToString();
		}
	}
}
