using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Text.Json;
using System.Windows;
using UI.Utility;
using static UI.Constants;

namespace UI {
	public partial class MainWindow : Window {
		private readonly IChatClient _client;
		private readonly Tokenizer _tokenizer;
		private readonly bool _expectingCodeResponse = true;
		private readonly ChatOptions _chatOptions;
		private readonly CancellationTokenSource _cts = new();
		private float _getTemperature() => _expectingCodeResponse ? 0.115f : 0.7f;

		private bool InterruptButtonEnabled { get; set; } = true;

		public MainWindow(IChatClient client, Tokenizer tokenizer, bool? codeMode = true) {
			_client = client;
			_tokenizer = tokenizer;

			if (!(codeMode ?? true)) {
				_expectingCodeResponse = false;
			}

			_chatOptions = new() {
				Temperature = _getTemperature(),
				TopK = 51,
				TopP = 0.95f,
				FrequencyPenalty = 1.12f
			};

			InitializeComponent();

			ToggleInterruptButton();
		}

		internal void ToggleInterruptButton() {
			InterruptButtonEnabled = !InterruptButtonEnabled;
		}

		internal async void ChatButtonClick(object sender, RoutedEventArgs e) {
			try {
				TheirResponse.Text = string.Empty;
				ToggleInterruptButton();
				ChatButton.IsEnabled = false;
				string formattedMessagesAsOneString = ConstructMessages.AsFormattedString(MessageText.Text);

				TokenizerConfigMistral? tokenizerConfigMistral = null;
				string chatTemplate = string.Empty;
				#region Deserialize tokenizer_config.json
				try {
					// Attempt to deserialize the tokenizer_config.json for the model
					tokenizerConfigMistral =
						JsonSerializer.Deserialize<TokenizerConfigMistral>(_pathToTokenizerJson);

					// Retrieve the 'chat_template' string 
					chatTemplate = tokenizerConfigMistral?.ChatTemplate ?? string.Empty;

				} catch (Exception exception) {
					MessageBox.Show($"{_userFriendlyExceptionCaughtWhileAttemptingToDeserializeResponse}{exception.Message}");
				}
				#endregion

				// Warn user chatTemplate is defaulting to empty string.
				if (string.IsNullOrEmpty(chatTemplate)) {
					MessageBox.Show($"{_userFriendlyWarningChatTemplateIsAttemptingToDefault}");
				}

				if (tokenizerConfigMistral != null) {
					string formattedPrompt = _tokenizer.ApplyChatTemplate(
						chatTemplate,
						formattedMessagesAsOneString,
						null,
						false);

					await ChatWithModelAsync(formattedPrompt);
				} else {
					SomethingWentWrong();
				}
			} catch (Exception) {
				SomethingWentWrong();
			} finally {
				AllowUserInputEntry();
			}
		}

		private void SomethingWentWrong() {
			TheirResponse.Text = _userFriendlyErrorResponse;
		}

		private void AllowUserInputEntry() {
			ToggleInterruptButton();
			ChatButton.IsEnabled = true;
		}

		internal async void InterruptButtonClick(object sender, RoutedEventArgs e) {
			if (!InterruptButtonEnabled) {
				return;
			}

			try {
				await _cts.CancelAsync();
				TheirResponse.Text = _userFriendlyStoppedResponse;

				ChatButton.IsEnabled = true;
				ToggleInterruptButton();

			} catch (Exception) {
				TheirResponse.Text = _userFriendlyErrorResponse;
			} finally {
				ChatButton.IsEnabled = true;
			}
		}

		internal async Task ChatWithModelAsync(string userInput) {
			CancellationToken ct = _cts.Token;

			await foreach (ChatResponseUpdate update in _client.GetStreamingResponseAsync(userInput, _chatOptions, ct)) {
				TheirResponse.Text += update.Text;
			}
		}

		internal void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
	}
}