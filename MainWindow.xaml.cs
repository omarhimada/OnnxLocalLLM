using Microsoft.ML.OnnxRuntimeGenAI;
using System.Windows;
using UI.Utility;
using static UI.Constants;

namespace UI {
	public partial class MainWindow : Window {
		/// <summary>
		/// Re-initialize the generator after each response.
		/// Better to re-initialize after the response as opposed to before your next input is tokenized.
		/// (e.g.: user reads initial output of the model and then by the time they comprehend, the generator is re-initialized)
		/// </summary>
		internal Generator? _generator;

		private readonly Model _model;
		private readonly Tokenizer _tokenizer;
		private readonly GeneratorParams _generatorParams;

		private readonly bool _expectingCodeResponse = true;
		private readonly CancellationTokenSource _cts = new();

		private float _getTemperature() => _expectingCodeResponse ? 0.115f : 0.7f;

		private bool InterruptButtonEnabled { get; set; } = true;

		public MainWindow(Model model, Tokenizer tokenizer, GeneratorParams generatorParams, bool? codeMode = true) {
			_model = model;
			_tokenizer = tokenizer;
			_generatorParams = generatorParams;

			if (!(codeMode ?? true)) {
				_expectingCodeResponse = false;
			}

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
				string systemAndUserMessage = string.Empty;
				try {
					systemAndUserMessage = ConstructMessages.AsFormattedString(UserInputText.Text);
				} catch (Exception promptConstructionException) {
					SomethingWentWrong(true);
					DebugWrite.Line(nameof(ConstructMessages.AsFormattedString), promptConstructionException);
				}
				await ChatWithModelAsync(systemAndUserMessage);
			} catch (Exception chatButtonClickException) {
				SomethingWentWrong();
				DebugWrite.Line(nameof(ChatButtonClick), chatButtonClickException);
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

		internal void InitializeGeneratorOrReInitialize() {
			_generator?.Dispose();
			_generator = new(_model, _generatorParams);
		}

		internal void SetGeneratorParameterSearchOptions() {
			#region Set generator parameters
			_generatorParams.SetSearchOption(_maxLengthParameter, 8192); // 16384 // 32768
			_generatorParams.SetSearchOption(_doSample, true);
			_generatorParams.SetSearchOption(_temperature, _getTemperature());
			_generatorParams.SetSearchOption(_topK, 51);
			_generatorParams.SetSearchOption(_topP, 0.9f);
			_generatorParams.SetSearchOption(_repetitionPenalty, 1.12f);
			#endregion
		}

		internal async Task ChatWithModelAsync(string systemAndUserMessage) {
			CancellationToken ct = _cts.Token;

			Sequences sequences = _tokenizer.Encode(systemAndUserMessage);

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
		}

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

		internal void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
	}
}