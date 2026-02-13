using Microsoft.ML.OnnxRuntimeGenAI;
using OLLM.State;
using OLLM.Utility.ModelSpecific;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using static OLLM.Constants;

namespace OLLM.Interact {
	internal class LinearCommunication(ModelState modelState) {
		private readonly OgaHandle _ogaHandle = new();

		private readonly CancellationTokenSource _cts = new();
		private bool InterruptButtonEnabled { get; set; } = true;

		/// <summary>
		/// Represents the user's interaction with the model. Currently, it is only text, the plan is to eventually
		/// create accessibility-enhanced interaction features involving text-speech, etc.
		/// </summary>
		internal async Task _interact(TextBox userInputText, RichTextBox theirResponse, Button chatButton) {
			try {
				ToggleInterruptButton();
				await SendMessage(userInputText.Text, theirResponse);
			} catch (Exception exception) {
				SomethingWentWrong(theirResponse, false, exception.Message);
			} finally {
				AllowUserInputEntry(chatButton);
			}
		}

		internal async Task _interrupt(RichTextBox theirResponse, Button chatButton) {
			if (!InterruptButtonEnabled) {
				return;
			}
			try {
				await _cts.CancelAsync();
				theirResponse.Document = new FlowDocument(); //.Text = _userFriendlyStoppedResponse;
				chatButton.IsEnabled = true;
				ToggleInterruptButton();
			} catch (Exception) {
				SomethingWentWrong(theirResponse, false);
			} finally {
				chatButton.IsEnabled = true;
			}
		}

		private async Task SendMessage(string userInputText, RichTextBox theirResponse) {
			string systemAndUserMessage = string.Empty;
			try {
				systemAndUserMessage = Phi4.AsFormattedString(userInputText);
			} catch (Exception) {
				SomethingWentWrong(theirResponse, true);
			}
			await ChatWithModelAsync(systemAndUserMessage, theirResponse);
		}

		private async Task ChatWithModelAsync(string systemAndUserMessage, RichTextBox theirResponse) {
			CancellationToken ct = _cts.Token;

			await Task.Run(() => {
				// Clear UI on the correct thread

				FlowDocument fd = new();
				Application.Current.Dispatcher.Invoke(() => {
					fd = new FlowDocument();
					return theirResponse.Document = new FlowDocument();
				});

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

		private void SomethingWentWrong(RichTextBox theirResponse, bool? couldNotParseUserInput = false, string? exceptionMessage = null) {
			//theirResponse.Document += $"{_userFriendlyErrorResponse}\n";

			//if (couldNotParseUserInput!.Value) {
			//	theirResponse.Text += $"{_userFriendlyParsingUserInputToMessageException}\n";
			//}

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
