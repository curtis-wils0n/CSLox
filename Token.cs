namespace CSLox;

public class Token(TokenType type, string lexeme, object? literal, int line)
{
	private readonly int _line = line;

	public override string ToString()
	{
		return type + " " + lexeme + " " + literal;
	}
}
