using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OLLM.Utility;

using static Constants;
using static MdFd;

public static class Md {
	public static List<Block> Parse(string? s) {
		if (s == null) {
			return [];
		}

		List<Block> blocks = [];
		string[] lines = s.Replace(_nlrs, _nl).Split(_nlc);

		bool inCode = false;
		StringBuilder code = new();

		List<string> paraLines = [];
		int pendingHeading = 0;

		List<string> bulletLines = [];

		string[] thinkingLineRange = [];
		string postThinkingTextStart = string.Empty;
		int indexOfThinkEnd = -1;

		if (lines[0].StartsWith("<think>")) {
			lines[0] = string.Empty;
			// With the system prompt construction in Constants, the reasoning model will begin with <think>
			int indexOfThinkStart = 0;
			string? lineWithEndThink = lines.FirstOrDefault(line => line.Contains("</think>"));
			if (lineWithEndThink != null) {
				indexOfThinkEnd = lines.IndexOf(lineWithEndThink);

				thinkingLineRange = lines[indexOfThinkStart..indexOfThinkEnd];
				string[] splitLineWithEndThink = lineWithEndThink.Split("</think>");

				string lastThinkingPortion = splitLineWithEndThink[0];
				postThinkingTextStart = splitLineWithEndThink[1];

				ParagraphBlock thinkingBlock = new(ParseInlines(string.Join(_wsc, thinkingLineRange.Append(lastThinkingPortion))), 0);
				blocks.Add(thinkingBlock);
			} else {
				// Malformed - thinking starts but does not end. 
				lines[0] = lines[0].Replace("<think>", string.Empty);
			}
		}

		if (!string.IsNullOrEmpty(postThinkingTextStart) && indexOfThinkEnd != -1) {
			// The model was thinking
			// Separate the thinking in its own paragraph block that we can later style differently, or we can keep it hidden.
			lines = lines.Except(thinkingLineRange).ToArray();
		}

		foreach (string raw in lines) {
			if (raw.StartsWith(_tbt) || raw.StartsWith(_lineBreak)) {
				if (!inCode) {
					FlushBullets();
					FlushParagraph();
					inCode = true;
				} else {
					inCode = false;
					blocks.Add(new CodeBlock(code.ToString().TrimEnd(_nlc, _rc)));
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

			if (raw.StartsWith(_ds) || raw.StartsWith(_os)) {
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
			blocks.Add(new CodeBlock(code.ToString().TrimEnd(_nlc, _rc)));
		}

		return blocks;

		void FlushParagraph() {
			if (paraLines.Count == 0) {
				return;
			}

			string text = string.Join(_wsc, paraLines.Select(x => x.Trim()).Where(x => x.Length > 0)).Trim();
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
}