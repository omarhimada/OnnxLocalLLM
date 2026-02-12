using Microsoft.ML.OnnxRuntimeGenAI;
using OLLM.State;
using OLLM.Utility.ModelSpecific;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static OLLM.Constants;

namespace OLLM.Interact {
	internal class LinearCommunication(ModelState modelState) {
		private readonly CancellationTokenSource _cts = new();
		private bool InterruptButtonEnabled { get; set; } = true;

		/// <summary>
		/// Represents the user's interaction with the model. Currently, it is only text, the plan is to eventually
		/// create accessibility-enhanced interaction features involving text-speech, etc.
		/// </summary>
		internal async Task _interact(TextBox userInputText, TextBox theirResponse, Button chatButton) {
			try {
				ToggleInterruptButton();
				await SendMessage(userInputText.Text, theirResponse);
			} catch (Exception exception) {
				SomethingWentWrong(theirResponse, false, exception.Message);
			} finally {
				AllowUserInputEntry(chatButton);
			}
		}

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
				SomethingWentWrong(theirResponse, false);
			} finally {
				chatButton.IsEnabled = true;
			}
		}

		private async Task SendMessage(string userInputText, TextBox theirResponse) {
			string systemAndUserMessage = string.Empty;
			try {
				systemAndUserMessage = Phi4.AsFormattedString(userInputText);
			} catch (Exception) {
				SomethingWentWrong(theirResponse, true);
			}
			await ChatWithModelAsync(systemAndUserMessage, theirResponse);
		}

		private async Task ChatWithModelAsync(string systemAndUserMessage, TextBox theirResponse) {
			CancellationToken ct = _cts.Token;

			await Task.Run(() => {
				// Clear UI on the correct thread
				Application.Current.Dispatcher.Invoke(() => theirResponse.Text = string.Empty);

				using OgaHandle ogaHandle = new();
				using Sequences sequences = modelState.Tokenizer!.Encode(systemAndUserMessage);

				modelState.SetGeneratorParameterSearchOptions();
				modelState.RefreshGenerator();
				modelState.Generator!.AppendTokenSequences(sequences);

				// Create one stream for the entire response
				using TokenizerStream ts = modelState.Tokenizer!.CreateStream();

				while (!modelState.Generator.IsDone() && !ct.IsCancellationRequested) {
					modelState.Generator.GenerateNextToken();

					// Decode on the background thread to a string
					string piece = ts.Decode(modelState.Generator.GetSequence(0)[^1]);

					// Append the words as they're generated on the UI thread
					Application.Current.Dispatcher.InvokeAsync(() => {
						theirResponse.AppendText(piece);
						theirResponse.ScrollToEnd();
					}, DispatcherPriority.Normal, ct);
				}
			}, ct);
			// Debugging
			//await Remember.MemorizeDiscussionAsync(theirResponse.Text, ct);
		}

		private void SomethingWentWrong(TextBox theirResponse, bool? couldNotParseUserInput = false, string? exceptionMessage = null) {
			theirResponse.Text += $"{_userFriendlyErrorResponse}\n";

			if (couldNotParseUserInput!.Value) {
				theirResponse.Text += $"{_userFriendlyParsingUserInputToMessageException}\n";
			}

			if (exceptionMessage != null) {
				MessageBox.Show(exceptionMessage);
			}
		}

		private void AllowUserInputEntry(Button chatButton) {
			ToggleInterruptButton();
			chatButton.IsEnabled = true;
		}

		private void ToggleInterruptButton() => InterruptButtonEnabled = !InterruptButtonEnabled;
	}
}
