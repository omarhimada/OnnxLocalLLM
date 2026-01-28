using Microsoft.Extensions.AI;
using System.Windows;

namespace UI {
	public partial class MainWindow : Window {
		private readonly IChatClient _client;

		public MainWindow(IChatClient client) {
			_client = client;

			InitializeComponent();
		}

		internal async Task ChatWithModelAsync(string userInput) {
			await foreach (ChatResponseUpdate update in _client.GetStreamingResponseAsync(userInput)) {
				TheirResponse.Text += update.Text;
			}
		}

		internal async void Button_Click(object sender, RoutedEventArgs e) {
			try {
				TheirResponse.Text = string.Empty;

				await ChatWithModelAsync(MessageText.Text.Trim());
			} catch (Exception) {
				TheirResponse.Text = "I'm sorry, something went wrong. I cannot respond.";
			}
		}
	}
}