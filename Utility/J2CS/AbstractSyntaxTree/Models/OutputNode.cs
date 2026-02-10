namespace JinjaToCSharp.AbstractSyntaxTree.Models;

internal sealed class OutputNode(string expr) : Node {
	public string Expr { get; } = expr;
}
