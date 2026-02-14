using Microsoft.ML.OnnxRuntimeGenAI;
using OLLM.State;
using OLLM.Utility;
using OLLM.Utility.ModelSpecific;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using static OLLM.Constants;
using static OLLM.Utility.MdFd;
namespace OLLM.Interact {
	internal partial class LinearCommunication(ModelState modelState) {
		private readonly OgaHandle _ogaHandle = new();
		private static readonly FontFamily _fontFamily = new("Cascadia Mono ExtraLight");
		private static readonly FontFamily _fontFamilyBold = new("Cascadia Mono SemiSBold");
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
			// New FlowDocument and a mutable md buffer
			await Application.Current.Dispatcher.InvokeAsync(() => { theirResponse.Document = new FlowDocument(); },
				DispatcherPriority.Normal, ct);
			//StringBuilder mdB = new();
			//IncrementalParserState parserState = new();
			// TODO extend `AppendParsedDelta` with additional pattern checks and create the corresponding WPF elements(`List`, `BlockUIContainer`, etc.) while still using the same `state` object to keep track of multi‑line constructs.
			await Task.Run(() => {
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
					// mdB.Append(piece);
					// Append the words as they're generated on the UI thread
					Application.Current.Dispatcher.InvokeAsync(() => {
						//AppendParsedDelta(theirResponse, piece, parserState);
						theirResponse.AppendText(piece);
						theirResponse.ScrollToEnd();
					}, DispatcherPriority.Normal, ct);
				}
			}, ct);

			string GetPlainText(RichTextBox rtb) {
				TextRange range = new(
					rtb.Document.ContentStart,
					rtb.Document.ContentEnd);

				return range.Text;
			}

			List<MdFd.Block> blocks = SimpleChatMarkdown.Parse(GetPlainText(theirResponse));
			theirResponse.Document = FlowDocRenderer.Render(blocks);
			theirResponse.ScrollToEnd();
			// Debugging
			//await Remember.MemorizeDiscussionAsync(theirResponse.Text, ct);
		}
		internal class IncrementalParserState {
			public bool InCodeBlock { get; set; } = false;
			public Paragraph CurrentCodeParagraph { get; set; } = null!;
		}
		internal static void AppendParsedDelta(RichTextBox rtb, string delta, IncrementalParserState state) {
			if (string.IsNullOrEmpty(delta)) {
				return;
			}
			// Split the delta into lines – a line may be incomplete, but that’s fine;
			// the next chunk will continue it.
			string[] lines = delta.IndexOf(_nlc) == -1 ? [delta] : delta.Split(_nlc);
			foreach (string rawLine in lines) {
				string line = rawLine.TrimEnd(_rc); // strip Windows CR
				if (line.StartsWith(_tbt)) {
					if (state.InCodeBlock) {
						// close the block – add the paragraph to the document
						rtb.Document.Blocks.Add(state.CurrentCodeParagraph);
						state.CurrentCodeParagraph = null!;
						state.InCodeBlock = false;
					} else {
						// open a new block
						state.InCodeBlock = true;
						state.CurrentCodeParagraph = new Paragraph();
					}
					continue; // the back‑tick line itself is not displayed
				}
				if (state.InCodeBlock) {
					// Inside a code block – preserve whitespace and style
					Run run = new(line + _nlc) {
						FontFamily = _fontFamily,
						Background = Brushes.LightGray,
						Foreground = Brushes.Black
					};
					state.CurrentCodeParagraph.Inlines.Add(run);
					continue;
				}
				Match headingMatch = HeadingsRegex().Match(line);
				if (headingMatch.Success) {
					int level = headingMatch.Groups[1].Value.Length;
					Paragraph heading = new(new Run(headingMatch.Groups[2].Value)) {
						FontFamily = _fontFamilyBold,
						FontWeight = FontWeights.Bold,
						FontSize = 24 - (level * 2)
					};
					rtb.Document.Blocks.Add(heading);
					continue;
				}
				line = line.Trim();
				if (line.Contains(_wsc, StringComparison.Ordinal)) {
					// line is at least two words, treat it as a paragraph.
					Paragraph paragraph = new();
					foreach (Inline inline in ParseInline(line)) {
						paragraph.Inlines.Add(inline);
					}
					rtb.Document.Blocks.Add(paragraph);
				} else {
					if (rtb.Document.Blocks.Count == 0 || line.Contains(_nlc)) {
						rtb.Document.Blocks.Add(new Paragraph());
					}
					rtb.Document.Blocks.LastBlock!.ContentEnd.InsertTextInRun($"{line}{_wsc}");
				}
			}
		}
		internal static IEnumerable<Run> ParseMarkdownTokens(IEnumerable<string> tokens) {
			bool inBold = false;
			bool inItalic = false;
			bool inCode = false;
			// Helper to create a Run with the current style flags
			Run CreateRun(string text) {
				Run run = new(text);
				if (inBold) {
					run.FontWeight = FontWeights.Bold;
				}
				if (inItalic) {
					run.FontStyle = FontStyles.Italic;
				}
				if (inCode) {
					run.FontFamily = _fontFamily;
					run.Background = Brushes.LightGray;
				}
				return run;
			}
			foreach (string token in tokens) {
				switch (token) {
					case _ts:
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
					// Look ahead for the next md indicator inside the same token
					int nextBold = remaining.IndexOf(_ts, StringComparison.Ordinal);
					int nextItalic = remaining.IndexOf(_os, StringComparison.Ordinal);
					int nextCode = remaining.IndexOf(_t, StringComparison.Ordinal);
					// Find the earliest markdown md indicator inside the current fragment
					int nextPos = int.MaxValue;
					string? mdIndicator = null;
					if (nextBold >= 0 && nextBold < nextPos) { nextPos = nextBold; mdIndicator = _ts; }
					if (nextItalic >= 0 && nextItalic < nextPos) { nextPos = nextItalic; mdIndicator = _os; }
					if (nextCode >= 0 && nextCode < nextPos) { nextPos = nextCode; mdIndicator = _t; }
					// If no md indicator was found, nextPos will stay at int.MaxValue
					if (nextPos == int.MaxValue) {
						// Emit the rest of the text with the current style and exit the loop
						yield return CreateRun(remaining);
						break;
					}
					// Emit text before the md indicator
					if (nextPos > 0) {
						yield return CreateRun(remaining[..nextPos]);
					}
					// Toggle the appropriate flag for the md indicator we just saw
					switch (mdIndicator) {
						case "**":
							inBold = !inBold;
							break;
						case "*":
							inItalic = !inItalic;
							break;
						case "`":
							inCode = !inCode;
							break;
					}
					// Move past the marker and continue parsing the rest
					remaining = remaining[(nextPos + mdIndicator?.Length ?? 1)..];
				}
			}
			// If the md was malformed and a span is left open,
			// the remaining style simply stays applied to whatever follows.
		}
		private static IEnumerable<Inline> ParseInline(string line) {
			// Split the line into raw markdown fragments (the same regex you used before)
			string[] tokens = TokensRegex().Split(line);
			// Use the stateful parser that can handle isolated markers
			foreach (Run run in ParseMarkdownTokens(tokens))
				yield return run;
		}
		private static void SomethingWentWrong(RichTextBox theirResponse, bool? couldNotParseUserInput = false, string? exceptionMessage = null) {
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
		[GeneratedRegex(@"(\\*\\*[^*]+\\*\\*|\\*[^*]+\\*|`[^`]+\`)", RegexOptions.Singleline)]
		private static partial Regex TokensRegex();
		[GeneratedRegex(@"^(#{1,6})\s+(.*)")]
		private static partial Regex HeadingsRegex();
	}
}
