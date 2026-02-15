namespace OLLM.Utility;

public abstract record Block;
public record ParagraphBlock(List<InlineSpan> Inlines, int HeadingLevel = 0) : Block;
public record CodeBlock(string Code) : Block;
public record BulletListBlock(List<List<InlineSpan>> Items) : Block;
public abstract record InlineSpan;
public record TextSpan(string Text) : InlineSpan;
public record CodeSpan(string Text) : InlineSpan;
public record BoldSpan(List<InlineSpan> Children) : InlineSpan;
public record ItalicSpan(List<InlineSpan> Children) : InlineSpan;
