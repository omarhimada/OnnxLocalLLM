using System.Text;

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

		foreach (string raw in lines) {
			if (raw.StartsWith(_tbt)) {
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