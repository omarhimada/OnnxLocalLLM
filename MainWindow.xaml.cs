using Microsoft.Extensions.AI;
using System.Windows;
using UI.Utility;
using static UI.Constants;

namespace UI {
	public partial class MainWindow : Window {
		private readonly IChatClient _client;

		private readonly bool _expectingCodeResponse = true;
		private readonly ChatOptions _chatOptions;
		private readonly CancellationTokenSource _cts = new();
		private float _getTemperature() => _expectingCodeResponse ? 0.115f : 0.7f;

		private bool InterruptButtonEnabled { get; set; } = true;

		public MainWindow(IChatClient client, bool? codeMode = true) {
			_client = client;

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

				await ChatWithModelAsync(ConstructMessages.AsFormattedString(MessageText.Text));

			} catch (Exception) {
				TheirResponse.Text = _userFriendlyErrorResponse;
			} finally {
				ToggleInterruptButton();
				ChatButton.IsEnabled = true;
			}
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