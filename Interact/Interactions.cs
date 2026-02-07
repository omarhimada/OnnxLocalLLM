using Microsoft.ML.OnnxRuntimeGenAI;
using OLLM.Memory;
using OLLM.State;
using OLLM.Utility;
using System.Windows;
using System.Windows.Controls;
using static OLLM.Constants;

namespace OLLM.Interact {
	internal class LinearCommunication(ModelState modelState) {
		private readonly CancellationTokenSource _cts = new();

		private bool InterruptButtonEnabled { get; set; } = true;

		internal async Task _interrupt(TextBox theirResponse, Button chatButton) {
			if (!InterruptButtonEnabled) {
				return;
			}

			try {
				await _cts.CancelAsync();
				theirResponse.Text = _userFriendlyStoppedResponse;

				chatButton.IsEnabled = true;
				ToggleInterruptButton();

			} catch (Exception) {
				SomethingWentWrong(theirResponse);
			} finally {
				chatButton.IsEnabled = true;
			}
		}

		/// <summary>
		/// Represents the user's interaction with the model. Currently, it is only text, the plan is to eventually
		/// create accessibility-enhanced interaction features involving text-speech, etc.
		/// </summary>
		internal async Task _interact(TextBox userInputText, TextBox theirResponse, Button chatButton) {
			await _preInteractThinking(theirResponse);
			try {
				ToggleInterruptButton();
				await SendMessage(userInputText.Text, theirResponse);
			} catch (Exception) {
				SomethingWentWrong(theirResponse);
			} finally {
				AllowUserInputEntry(chatButton);
			}
		}

		private async Task SendMessage(string userInputText, TextBox theirResponse) {
			string systemAndUserMessage = string.Empty;
			try {
				systemAndUserMessage = ConstructMessages.AsFormattedString(userInputText);
			} catch (Exception) {
				SomethingWentWrong(theirResponse, true);
			}
			await ChatWithModelAsync(systemAndUserMessage, theirResponse);
		}

		private async Task ChatWithModelAsync(string systemAndUserMessage, TextBox theirResponse) {
			CancellationToken ct = _cts.Token;

			Sequences sequences = modelState.Tokenizer!.Encode(systemAndUserMessage);

			modelState.SetGeneratorParameterSearchOptions();
			modelState.RefreshGenerator();
			modelState.Generator!.AppendTokenSequences(sequences);

			// Clear the "..." thinking text before the actual response
			theirResponse.Text = string.Empty;

			await Task.Run(() => {
				while (!modelState.Generator.IsDone()) {
					modelState.Generator.GenerateNextToken();

					// Get the newly generated token and decode it
					int nextToken = modelState.Generator.GetSequence(0)[^1];
					IEnumerable<char> outputPiece = modelState.Tokenizer.Decode(new[] { nextToken });

					if (outputPiece != null) {
						Application.Current.Dispatcher.Invoke(() => {
							foreach (char c in outputPiece) {
								theirResponse.Text += c;
							}
						});
					}
				}
			}, ct);

			await Remember.MemorizeDiscussionAsync(theirResponse.Text, ct);
		}

		/// <summary>
		/// Small 'thinking' animation with ellipsis 
		/// </summary>
		/// <returns></returns>
		private async Task _preInteractThinking(TextBox theirResponse) {
			const char _d = '.';
			theirResponse.Text = string.Empty;
			for (int i = 0; i < 3; i++) {
				theirResponse.Text += _d;
				await Task.Delay(15);
			}
		}

		private void SomethingWentWrong(TextBox theirResponse, bool couldNotParseUserInput = false) {
			theirResponse.Text += $"{_userFriendlyErrorResponse}\n";

			if (couldNotParseUserInput) {
				theirResponse.Text += $"{_userFriendlyParsingUserInputToMessageException}\n";
			}
		}

		private void AllowUserInputEntry(Button chatButton) {
			ToggleInterruptButton();
			chatButton.IsEnabled = true;
		}

		private void ToggleInterruptButton() => InterruptButtonEnabled = !InterruptButtonEnabled;
	}
}
