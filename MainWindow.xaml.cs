using Microsoft.ML.OnnxRuntimeGenAI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UI.Memory;
using UI.Utility;
using static UI.Constants;

namespace UI {
	internal partial class MainWindow : Window {
		#region Fields & Properties
		internal Model? _model;
		internal LocalMiniLmEmbeddingGenerator? _localMiniLmEmbeddingGenerator;
		internal Tokenizer? _tokenizer;
		internal Generator? _generator;
		internal GeneratorParams? _generatorParams;
		internal CancellationTokenSource _cts = new();
		internal Remember? _remember;
		internal bool _expectingCodeResponse = true;
		private bool InterruptButtonEnabled { get; set; } = true;
		private float _getTemperature() => _expectingCodeResponse ? 0.225f : 0.7f;
		private readonly DispatcherTimer _timer;
		#endregion

		#region Initialization
		protected override void OnInitialized(EventArgs e) {
			base.OnInitialized(e);

			App.FinishedInitializing();
		}

		internal void Initialize(
			Model model,
			Tokenizer tokenizer,
			GeneratorParams generatorParams,
			bool? codeMode = true) {

			_model = model;

			_tokenizer = tokenizer!;
			_generatorParams = generatorParams;

			if (!(codeMode ?? true)) {
				_expectingCodeResponse = false;
			}

			ToggleInterruptButton();

			_timer.Start();
		}

		internal void PostInitialize(LocalMiniLmEmbeddingGenerator localMiniLmEmbeddingGenerator) {
			_localMiniLmEmbeddingGenerator = localMiniLmEmbeddingGenerator;

			// Load memories. They should remember what we spoke about yesterday, a week ago, maybe even years.
			// This should initialize their memories.db if it does not already exist
			_remember = new Remember(_localMiniLmEmbeddingGenerator);
		}

		internal MainWindow() {
			InitializeComponent();

			_timer = new DispatcherTimer {
				Interval = TimeSpan.FromSeconds(12)
			};

			_timer.Tick += _talkTok!;

			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
		}

		private void _talkTok(object sender, EventArgs e) {
			const char _qm = '?';
			if (UserInputText.Text.Contains(_qm)) {
				_interact();
				UserInputText.Text = string.Empty;
			}
		}

		// Stop the timer when the window is closed
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			_timer.Stop();
		}
		#endregion

		#region Configuration
		/// <summary>
		/// Re-initialize the generator after each response as opposed to before your next input is tokenized.
		/// (i.e.: user reads initial output of the model and then by the time they comprehend, the generator is re-initialized)
		/// </summary>
		internal void InitializeGeneratorOrReInitialize() {
			_generator?.Dispose();
			_generator = new(_model, _generatorParams);
		}

		internal void SetGeneratorParameterSearchOptions() {
			#region Set generator parameters
			_generatorParams!.SetSearchOption(_maxLengthParameter, 32768); // 8192 // 16384
			_generatorParams!.SetSearchOption(_doSample, true);
			_generatorParams!.SetSearchOption(_temperature, _getTemperature());
			_generatorParams!.SetSearchOption(_topK, 51);
			_generatorParams!.SetSearchOption(_topP, 0.9f);
			_generatorParams!.SetSearchOption(_repetitionPenalty, 1.12f);
			#endregion
		}
		#endregion

		#region Component Interactions
		internal async void ChatButtonClick(object sender, RoutedEventArgs e) {
			await _interact();
		}

		/// <summary>
		/// Handles the click event for the interrupt button, attempting to cancel the current operation and update the user
		/// interface accordingly.
		/// </summary>
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
				SomethingWentWrong();
			} finally {
				ChatButton.IsEnabled = true;
			}
		}
		internal void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
		internal void ToggleInterruptButton() {
			InterruptButtonEnabled = !InterruptButtonEnabled;
		}

		#region Code-mode temperature toggler event handlers
		private void CodeModeEnabled_Changed(object sender, RoutedEventArgs e) {
			if (sender is not CheckBox checkBox) {
				return;
			}

			_expectingCodeResponse = checkBox.IsChecked ?? false;
		}
		#endregion
		#endregion

		#region Interact
		internal async Task ChatWithModelAsync(string systemAndUserMessage) {
			CancellationToken ct = _cts.Token;

			Sequences sequences = _tokenizer!.Encode(systemAndUserMessage);

			SetGeneratorParameterSearchOptions();
			InitializeGeneratorOrReInitialize();

			_generator!.AppendTokenSequences(sequences);

			await Task.Run(() => {
				while (!_generator.IsDone()) {
					_generator.GenerateNextToken();

					// Get the newly generated token and decode it
					int nextToken = _generator.GetSequence(0)[^1];
					IEnumerable<char> outputPiece = _tokenizer.Decode(new[] { nextToken });

					if (outputPiece != null) {
						// TODO batch instead of char-by-char (although currently it is incredibly fast)
						// Update UI from background thread
						Dispatcher.Invoke(() => {
							foreach (char c in outputPiece) {
								TheirResponse.Text += c;
							}
						});
					}
				}
			}, ct);

			await Remember.MemorizeDiscussionAsync(TheirResponse.Text, ct);
		}

		private async Task _interact() {
			try {
				TheirResponse.Text = string.Empty;
				ToggleInterruptButton();
				ChatButton.IsEnabled = false;

				await SendMessage(UserInputText.Text);

			} catch (Exception) {
				SomethingWentWrong();
			} finally {
				AllowUserInputEntry();
			}
		}
		internal async Task SendMessage(string userInputText) {
			string systemAndUserMessage = string.Empty;
			try {
				systemAndUserMessage = ConstructMessages.AsFormattedString(userInputText);
			} catch (Exception) {
				SomethingWentWrong(true);
			}
			await ChatWithModelAsync(systemAndUserMessage);
		}
		#endregion

		#region Reaction
		private void SomethingWentWrong(bool couldNotParseUserInput = false) {
			TheirResponse.Text += $"{_userFriendlyErrorResponse}\n";

			if (couldNotParseUserInput) {
				TheirResponse.Text += $"{_userFriendlyParsingUserInputToMessageException}\n";
			}
		}
		private void AllowUserInputEntry() {
			ToggleInterruptButton();
			ChatButton.IsEnabled = true;
		}
		#endregion
	}
}