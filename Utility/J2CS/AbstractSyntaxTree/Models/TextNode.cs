namespace JinjaToCSharp.AbstractSyntaxTree.Models;

internal sealed class TextNode(string text) : Node {
	public string Text { get; } = text;
}