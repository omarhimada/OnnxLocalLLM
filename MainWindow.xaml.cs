using OLLM.Interact;
using OLLM.Memory;
using OLLM.State;
using System.Windows;
using System.Windows.Controls;

namespace OLLM {
	internal partial class MainWindow : Window {
		#region Fields & Properties
		internal Remember? Memories;

		internal ModelState? ModelState;
		internal EmbedderState? EmbedderState;
		internal MiniEmbedder? MiniEmbedder;
		internal LinearCommunication? LinearCommunication;

		#region Disabled until text-to-speech implementation
		//private readonly DispatcherTimer _timer;
		//private void _talkTok(object sender, EventArgs e) {
		//	const char _qm = '?';
		//	if (UserInputText.Text.Contains(_qm)) {
		//		_interact();
		//		UserInputText.Text = string.Empty;
		//	}
		//}
		//protected override void OnInitialized(EventArgs e) {
		//	base.OnInitialized(e);
		//	_timer.Start();
		// ...
		//_timer = new DispatcherTimer {
		//	Interval = TimeSpan.FromSeconds(12)
		//};
		// ...
		//_timer.Tick += _talkTok!;
		//}
		//protected override void OnClosed(EventArgs e) {
		//	base.OnClosed(e);
		//	_timer.Stop();
		//}
		#endregion
		#endregion

		#region Initialization
		internal void Initialize(ModelState modelState, EmbedderState embedderState, MiniEmbedder miniEmbedder, LinearCommunication linearCommunication) {
			ModelState = modelState;
			EmbedderState = embedderState;
			MiniEmbedder = miniEmbedder;
			LinearCommunication = linearCommunication;

			try {
				Memories = new Remember(MiniEmbedder!);
			} catch (Exception) {
				// TODO ""Vector property 'Vector' has type 'GeneratedEmbeddings<Embedding<float>>' which isn't supported by your provider, and no embedding generator is configured. Configure a generator that supports converting 'GeneratedEmbeddings<Embedding<float>>' to vector type supported by your provider.""
				// MessageBox.Show(exception.Message);
				// Fail silently, continue
			}
		}

		internal MainWindow() {
			InitializeComponent();
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
		}
		#endregion

		internal async void ChatButtonClick(object sender, RoutedEventArgs e) => await LinearCommunication!._interact(UserInputText, TheirResponse, ChatButton);

		internal async void InterruptButtonClick(object sender, RoutedEventArgs e) => await LinearCommunication!._interrupt(TheirResponse, ChatButton);

		internal void CloseButtonClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		private void CodeModeToggled(object sender, RoutedEventArgs e) {
			if (sender is not CheckBox checkBox) {
				return;
			}

			ModelState!.ExpectingCodeResponse = checkBox.IsChecked ?? false;
		}
	}
}