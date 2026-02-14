using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static OLLM.Constants;
// ReSharper disable All

namespace OLLM.Utility;

using MdBlock = MdFd.Block;
using WpfBlock = Block;
using WpfList = List;

internal static class MdFd {
	public abstract record Block;
	public record ParagraphBlock(List<InlineSpan> Inlines, int HeadingLevel = 0) : MdBlock;
	public record CodeBlock(string Code) : MdBlock;
	public record BulletListBlock(List<List<InlineSpan>> Items) : MdBlock;
	public abstract record InlineSpan;
	public record TextSpan(string Text) : InlineSpan;
	public record CodeSpan(string Text) : InlineSpan;
	public record BoldSpan(List<InlineSpan> Children) : InlineSpan;
	public record ItalicSpan(List<InlineSpan> Children) : InlineSpan;

	public static class SimpleChatMarkdown {
		public static List<MdBlock> Parse(string? s) {
			List<MdBlock> blocks = [];
			string[] lines = (s ?? string.Empty).Replace("\r\n", "\n").Split('\n');

			bool inCode = false;
			StringBuilder code = new();

			List<string> paraLines = [];
			int pendingHeading = 0;

			List<string> bulletLines = [];

			foreach (string raw in lines) {
				if (raw.StartsWith(_tbt)) {
					if (!inCode) {
						FlushBullets();
						FlushParagraph();
						inCode = true;
					} else {
						inCode = false;
						blocks.Add(new CodeBlock(code.ToString().TrimEnd('\n', '\r')));
					}

					code.Clear();
					continue;
				}

				if (inCode) {
					code.AppendLine(raw);
					continue;
				}

				if (string.IsNullOrWhiteSpace(raw)) {
					FlushBullets();
					FlushParagraph();
					continue;
				}

				int level = CountHeadingLevel(raw);
				if (level > 0) {
					FlushBullets();
					FlushParagraph();

					string headingText = raw[level..].TrimStart();
					pendingHeading = level;
					paraLines.Add(headingText);
					FlushParagraph();
					continue;
				}

				if (raw.StartsWith("- ") || raw.StartsWith("* ")) {
					FlushParagraph();
					bulletLines.Add(raw[2..].Trim());
					continue;
				}

				FlushBullets();
				paraLines.Add(raw.Trim());
			}

			FlushBullets();
			FlushParagraph();

			if (inCode && code.Length > 0) {
				blocks.Add(new CodeBlock(code.ToString().TrimEnd('\n', '\r')));
			}

			return blocks;

			void FlushParagraph() {
				if (paraLines.Count == 0) {
					return;
				}

				// join paragraph lines with spaces (chat-style)
				string text = string.Join(" ", paraLines.Select(x => x.Trim()).Where(x => x.Length > 0)).Trim();
				paraLines.Clear();

				if (text.Length == 0) {
					return;
				}

				List<InlineSpan> inlines = ParseInlines(text);
				blocks.Add(new ParagraphBlock(inlines, pendingHeading));
				pendingHeading = 0;
			}

			void FlushBullets() {
				if (bulletLines.Count == 0) {
					return;
				}

				List<List<InlineSpan>> items = [];
				foreach (string b in bulletLines) {
					items.Add(ParseInlines(b));
				}

				bulletLines.Clear();
				blocks.Add(new BulletListBlock(items));
			}
		}

		private static int CountHeadingLevel(string line) {
			int i = 0;
			while (i < line.Length && i < 6 && line[i] == '#')
				i++;
			return i == 0 ? 0 : i < line.Length && char.IsWhiteSpace(line[i]) ? i : 0;
		}

		private static List<InlineSpan> ParseInlines(string text) {
			List<InlineSpan> result = [];
			int i = 0;

			while (i < text.Length) {
				if (text[i] == '`') {
					int end = text.IndexOf('`', i + 1);
					if (end > i) {
						string code = text.Substring(i + 1, end - i - 1);
						if (code.Length > 0) {
							result.Add(new CodeSpan(code));
						}

						i = end + 1;
						continue;
					}

					AddText("`");
					i++;
					continue;
				}

				if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*') {
					int end = text.IndexOf("**", i + 2, StringComparison.Ordinal);
					if (end > i) {
						string inner = text.Substring(i + 2, end - (i + 2));
						List<InlineSpan> children = ParseInlines(inner);
						result.Add(new BoldSpan(children));
						i = end + 2;
						continue;
					}

					AddText("**");
					i += 2;
					continue;
				}

				if (text[i] == '*') {
					int end = text.IndexOf('*', i + 1);
					if (end > i) {
						string inner = text.Substring(i + 1, end - i - 1);
						List<InlineSpan> children = ParseInlines(inner);
						result.Add(new ItalicSpan(children));
						i = end + 1;
						continue;
					}

					AddText("*");
					i++;
					continue;
				}

				int next = NextSpecial(text, i);
				AddText(text.Substring(i, next - i));
				i = next;
			}

			return result;

			void AddText(string t) {
				if (string.IsNullOrEmpty(t)) {
					return;
				}

				if (result.Count > 0 && result[^1] is TextSpan ts) {
					result[^1] = ts with { Text = ts.Text + t };
				} else {
					result.Add(new TextSpan(t));
				}
			}
		}

		private static int NextSpecial(string s, int start) {
			int best = s.Length;

			int t1 = s.IndexOf('`', start);
			if (t1 >= 0) {
				best = Math.Min(best, t1);
			}

			int t2 = s.IndexOf("**", start, StringComparison.Ordinal);
			if (t2 >= 0) {
				best = Math.Min(best, t2);
			}

			int t3 = s.IndexOf('*', start);
			if (t3 >= 0) {
				best = Math.Min(best, t3);
			}

			return best;
		}
	}

	public static class FlowDocRenderer {
		public static FlowDocument Render(IEnumerable<MdBlock> blocks) {
			FlowDocument doc = new() {
				PageWidth = 900,
			};

			foreach (MdBlock b in blocks) {
				switch (b) {
					case ParagraphBlock p:
						doc.Blocks.Add(RenderParagraph(p));
						break;

					case BulletListBlock bl:
						doc.Blocks.Add(RenderBulletList(bl));
						break;

					case CodeBlock c:
						doc.Blocks.Add(RenderCodeBlock(c));
						break;
				}
			}

			return doc;
		}

		private static WpfBlock RenderParagraph(ParagraphBlock p) {
			Paragraph para = new() {
				Margin = new Thickness(0, p.HeadingLevel > 0 ? 10 : 6, 0, 6)
			};

			if (p.HeadingLevel > 0) {
				para.FontWeight = FontWeights.SemiBold;
				para.FontSize = p.HeadingLevel switch {
					1 => 22,
					2 => 18,
					3 => 16,
					4 => 14,
					_ => 13
				};
			}

			foreach (Inline inline in RenderInlines(p.Inlines))
				para.Inlines.Add(inline);

			return para; // Paragraph : System.Windows.Documents.Block
		}

		private static WpfBlock RenderBulletList(BulletListBlock bl) {
			WpfList list = new() {
				MarkerStyle = TextMarkerStyle.Disc,
				Margin = new Thickness(18, 4, 0, 8)
			};

			foreach (List<InlineSpan> item in bl.Items) {
				Paragraph para = new() { Margin = new Thickness(0, 2, 0, 2) };
				foreach (Inline inline in RenderInlines(item))
					para.Inlines.Add(inline);

				list.ListItems.Add(new ListItem(para));
			}

			return list;
		}

		private static WpfBlock RenderCodeBlock(CodeBlock c) {
			Paragraph para = new(new Run(c.Code)) {
				FontFamily = new FontFamily("Cascadia Mono"),
				Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
				Foreground = Brushes.Gainsboro,
				Padding = new Thickness(10),
				Margin = new Thickness(0, 8, 0, 8),
				LineHeight = 16,
			};
			return para; // Paragraph : System.Windows.Documents.Block
		}

		private static IEnumerable<Inline> RenderInlines(IEnumerable<InlineSpan> spans) {
			foreach (InlineSpan s in spans) {
				switch (s) {
					case TextSpan t:
						yield return new Run(t.Text);
						break;

					case CodeSpan c:
						yield return new Run(c.Text) {
							FontFamily = new FontFamily("Cascadia Mono"),
							Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
							Foreground = Brushes.Gainsboro
						};
						break;

					case BoldSpan b: {
							Bold bold = new();
							foreach (Inline child in RenderInlines(b.Children))
								bold.Inlines.Add(child);
							yield return bold;
							break;
						}

					case ItalicSpan it: {
							Italic italic = new();
							foreach (Inline child in RenderInlines(it.Children))
								italic.Inlines.Add(child);
							yield return italic;
							break;
						}
				}
			}
		}
	}
}
