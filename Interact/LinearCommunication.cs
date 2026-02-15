using Microsoft.ML.OnnxRuntimeGenAI;

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace OLLM.Interact;

using State;
using Utility;
using Utility.ModelSpecific;
using static Constants;
using static Utility.MdFd;

internal partial class LinearCommunication(ModelState modelState) {
#pragma warning disable IDE0051
	private readonly OgaHandle _ogaHandle = new();
#pragma warning restore IDE0051

	private readonly CancellationTokenSource _cts = new();
	private bool InterruptButtonEnabled { get; set; } = true;

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
			theirResponse.Document = new FlowDocument();
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
		await Application.Current.Dispatcher.InvokeAsync(() => { theirResponse.Document = new FlowDocument(); }, DispatcherPriority.Normal, ct);
		await Task.Run(() => {
			using Sequences sequences = modelState.Tokenizer!.Encode(systemAndUserMessage);
			modelState.SetGeneratorParameterSearchOptions();
			modelState.RefreshGenerator();
			modelState.Generator!.AppendTokenSequences(sequences);
			using TokenizerStream ts = modelState.Tokenizer!.CreateStream();
			while (!modelState.Generator.IsDone() && !ct.IsCancellationRequested) {
				modelState.Generator.GenerateNextToken();
				string piece = ts.Decode(modelState.Generator.GetSequence(0)[^1]);
				Application.Current.Dispatcher.InvokeAsync(() => {
					theirResponse.AppendText(piece);
					theirResponse.ScrollToEnd();
				}, DispatcherPriority.Normal, ct);
			}
		}, ct);

		List<Block> blocks = Md.Parse(GetPlainText(theirResponse));
		theirResponse.Document = Fd.Render(blocks);
		theirResponse.ScrollToEnd();

		// Debugging
		//await Remember.MemorizeDiscussionAsync(theirResponse.Text, ct);
		return;
		#region Local functions
		string GetPlainText(RichTextBox rtb) {
			TextRange range = new(
				rtb.Document.ContentStart,
				rtb.Document.ContentEnd);

			return range.Text;
		}
		#endregion
	}

	internal static IEnumerable<Run> ParseMd(IEnumerable<string> tokens) {
		bool inBold = false;
		bool inItalic = false;
		bool inCode = false;

		foreach (string token in tokens) {
			switch (token) {
				case _tss:
				case _tse:
					inBold = !inBold;
					continue;
				case _os:
				case _ose:
					inItalic = !inItalic;
					continue;
				case _t:
					inCode = !inCode;
					continue;
			}

			string remaining = token;
			while (remaining.Length > 0) {
				int nextBold = remaining.IndexOf(_tss, StringComparison.Ordinal);
				int nextItalic = remaining.IndexOf(_os, StringComparison.Ordinal);
				int nextCode = remaining.IndexOf(_t, StringComparison.Ordinal);
				int nextPos = int.MaxValue;
				string? mdIndicator = null;
				if (nextBold >= 0 && nextBold < nextPos) { nextPos = nextBold; mdIndicator = _tss; }
				if (nextItalic >= 0 && nextItalic < nextPos) { nextPos = nextItalic; mdIndicator = _os; }
				if (nextCode >= 0 && nextCode < nextPos) { nextPos = nextCode; mdIndicator = _t; }
				if (nextPos == int.MaxValue) {
					yield return CreateRun(remaining);
					break;
				}
				if (nextPos > 0) {
					yield return CreateRun(remaining[..nextPos]);
				}
				switch (mdIndicator) {
					case _ts:
						inBold = !inBold;
						break;
					case _oss:
						inItalic = !inItalic;
						break;
					case _t:
						inCode = !inCode;
						break;
				}
				remaining = remaining[(nextPos + mdIndicator?.Length ?? 1)..];
			}
		}

		yield break;
		#region Local functions
		Run CreateRun(string text) {
			Run run = new(text);
			if (inBold) {
				run.FontWeight = FontWeights.Bold;
			}
			if (inItalic) {
				run.FontStyle = FontStyles.Italic;
			}
			if (inCode) {
				run.FontFamily = _fontFamily0x;
				run.Background = _owd;
			}
			return run;
		}
		#endregion
	}

	private static IEnumerable<Inline> ParseInline(string line) {
		string[] tokens = TokensRegex().Split(line);
		foreach (Run run in ParseMd(tokens)) {
			yield return run;
		}
	}

	private static void SomethingWentWrong(RichTextBox theirResponse, bool? couldNotParseUserInput = false, string? exceptionMessage = null) {
		theirResponse.Document = new FlowDocument();
		if (exceptionMessage != null) {
			MessageBox.Show(exceptionMessage);
		}
	}

	private void AllowUserInputEntry(Button chatButton) {
		ToggleInterruptButton();
		chatButton.IsEnabled = true;
	}

	private void ToggleInterruptButton() => InterruptButtonEnabled = !InterruptButtonEnabled;

	[GeneratedRegex(@"(\\*\\*[^*]+\\*\\*|\\*[^*]+\\*|`[^`]+\`)", RegexOptions.Singleline)]
	private static partial Regex TokensRegex();

	[GeneratedRegex(@"^(#{1,6})\s+(.*)")]
	private static partial Regex HeadingsRegex();
}