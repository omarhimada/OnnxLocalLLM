using System.Windows;
using System.Windows.Documents;

namespace OLLM.Utility.Syntax;

using static MdFd;

internal static class MdSyntaxApply {
	internal static Block RenderCodeBlock(
		string code,
		string? fenceLang,
		double fontSize = 14) {
		LanguageDefinition lang = LanguageRegistry.Resolve(fenceLang);

		Section section = new() {
			Margin = new Thickness(0, 8, 0, 8)
		};

		Paragraph codeBlockPara = new() {
			Margin = new Thickness(0),
			FontFamily = _fontFamily0x,
			FontSize = fontSize,
			LineHeight = fontSize * 1.1,
			Background = _black,
			Padding = new Thickness(10),
			TextIndent = 0
		};

		foreach ((TokenKind kind, string? text) in Highlighter.Highlight(code, lang)) {
			Run run = new(text) {
				Foreground = Theme.For(kind)
			};

			if (kind == TokenKind.Keyword) {
				run.FontWeight = FontWeights.Bold;
			}

			codeBlockPara.Inlines.Add(run);
		}

		section.Blocks.Add(codeBlockPara);
		return section;
	}
}