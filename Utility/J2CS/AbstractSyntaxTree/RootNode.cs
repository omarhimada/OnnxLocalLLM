namespace JinjaToCSharp.AbstractSyntaxTree;

internal sealed class RootNode(List<Node> nodes) : Node {
	public List<Node> Nodes { get; } = nodes;
}