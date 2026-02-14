using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace OLLM.Utility {
	internal static class MdFd {
		public abstract record Block;
		public record ParagraphBlock(string Text) : Block;
		public record CodeBlock(string Code) : Block;
		public record BulletListBlock(List<string> Items) : Block;

		public static class SimpleChatMarkdown {
			public static List<Block> Parse(string s) {
				List<Block> blocks = [];
				string[] lines = (s ?? "").Replace("\r\n", "\n").Split('\n');

				bool inCode = false;
				StringBuilder code = new();
				StringBuilder para = new();
				List<string> bullets = [];

				void FlushPara() {
					string t = para.ToString().Trim();
					if (t.Length > 0)
						blocks.Add(new ParagraphBlock(t));
					para.Clear();
				}

				void FlushBullets() {
					if (bullets.Count > 0)
						blocks.Add(new BulletListBlock([.. bullets]));
					bullets.Clear();
				}

				foreach (string line in lines) {
					if (line.StartsWith("```")) {
						if (!inCode) {
							FlushBullets();
							FlushPara();
							inCode = true;
							code.Clear();
						} else {
							inCode = false;
							blocks.Add(new CodeBlock(code.ToString().TrimEnd()));
							code.Clear();
						}
						continue;
					}

					if (inCode) {
						code.AppendLine(line);
						continue;
					}

					// bullets: "- " or "* "
					if (line.StartsWith("- ") || line.StartsWith("* ")) {
						FlushPara();
						bullets.Add(line.Substring(2).Trim());
						continue;
					}

					// blank line separates blocks
					if (string.IsNullOrWhiteSpace(line)) {
						FlushBullets();
						FlushPara();
						continue;
					}

					// normal paragraph text
					FlushBullets();
					if (para.Length > 0)
						para.Append(' ');
					para.Append(line.Trim());
				}

				FlushBullets();
				FlushPara();

				// if code fence never closed, treat as code
				if (inCode && code.Length > 0)
					blocks.Add(new CodeBlock(code.ToString().TrimEnd()));

				return blocks;
			}
		}

		public static class FlowDocRenderer {
			public static FlowDocument Render(IEnumerable<Block> blocks) {
				FlowDocument doc = new() {
					PageWidth = 800, // tweak to your chat area width
				};

				foreach (Block b in blocks) {
					switch (b) {
						case ParagraphBlock p:
							doc.Blocks.Add(new Paragraph(new Run(p.Text)));
							break;

						case BulletListBlock bl:
							List list = new() { MarkerStyle = TextMarkerStyle.Disc };
							foreach (string item in bl.Items)
								list.ListItems.Add(new ListItem(new Paragraph(new Run(item))));
							doc.Blocks.Add(list);
							break;

						case CodeBlock c:
							Paragraph para = new(new Run(c.Code)) {
								FontFamily = new FontFamily("Cascadia Mono"),
								Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
								Foreground = Brushes.Gainsboro,
								Padding = new Thickness(8),
								Margin = new Thickness(0, 6, 0, 6)
							};
							doc.Blocks.Add(para);
							break;
					}
				}

				return doc;
			}
		}
	}
}
