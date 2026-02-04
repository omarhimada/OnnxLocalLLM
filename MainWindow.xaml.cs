using Microsoft.ML.OnnxRuntimeGenAI;
using System.Windows;
using UI.Memory;
using UI.Utility;
using static UI.Constants;

namespace UI {
	internal partial class MainWindow : Window {
		internal Model? _model;
		internal LocalEmbeddingGenerator? _localEmbeddingGenerator;
		internal Tokenizer? _tokenizer;

		internal Generator? _generator;
		internal GeneratorParams? _generatorParams;

		internal bool _expectingCodeResponse = true;
		internal CancellationTokenSource _cts = new();

		internal Remember? _remember;

		#region Initialization
		protected override void OnInitialized(EventArgs e) {
			base.OnInitialized(e);

			App.FinishedInitializing();
		}

		internal void Initialize(Model model,
			LocalEmbeddingGenerator localEmbeddingGenerator,
			Tokenizer tokenizer,
			GeneratorParams generatorParams,
			bool? codeMode = true) {

			_model = model;
			_localEmbeddingGenerator = localEmbeddingGenerator;

			_tokenizer = tokenizer!;
			_generatorParams = generatorParams;

			if (!(codeMode ?? true)) {
				_expectingCodeResponse = false;
			}

			// Load memories. They should remember what we spoke about yesterday, a week ago, maybe even years.
			// This should initialize their memories.db if it does not already exist
			_remember = new Remember(_localEmbeddingGenerator);

			ToggleInterruptButton();
		}

		internal MainWindow() {
			InitializeComponent();

			// Make sure the 'x' button in the top right actually closes the process.
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
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

		internal void ToggleInterruptButton() {
			InterruptButtonEnabled = !InterruptButtonEnabled;
		}

		#region Code-mode temperature toggler event handlers
		private void CodeModeEnabled_Checked(object sender, RoutedEventArgs e) {
			_expectingCodeResponse = true;
		}

		private void CodeModeEnabled_Unchecked(object sender, RoutedEventArgs e) {
			_expectingCodeResponse = false;
		}
		#endregion
		#endregion

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

		private void AllowUserInputEntry() {
			ToggleInterruptButton();
			ChatButton.IsEnabled = true;
		}

		private float _getTemperature() => _expectingCodeResponse ? 0.225f : 0.7f;

		private bool InterruptButtonEnabled { get; set; } = true;

		internal void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		#region Interact
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
		#endregion
	}
}