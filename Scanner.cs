namespace CSLox;

public class Scanner(string source)
{
	private readonly List<Token> _tokens = [];
	private int _start;
	private int _current;
	private int _line = 1;

	public List<Token> ScanTokens()
	{
		while (!IsAtEnd())
		{
			// We are at the beginning of the next lexeme.
			_start = _current;
			ScanToken();
		}

		_tokens.Add(new Token(TokenType.EOF, "", null, _line));
		return _tokens;
	}

	private void ScanToken()
	{
		char c = Advance();
		switch (c)
		{
			case '(': AddToken(TokenType.LeftParen); break;
			case ')': AddToken(TokenType.RightParen); break;
			case '{': AddToken(TokenType.LeftBrace); break;
			case '}': AddToken(TokenType.RightBrace); break;
			case ',': AddToken(TokenType.Comma); break;
			case '.': AddToken(TokenType.Dot); break;
			case '-': AddToken(TokenType.Minus); break;
			case '+': AddToken(TokenType.Plus); break;
			case ';': AddToken(TokenType.Semicolon); break;
			case '*': AddToken(TokenType.Star); break;
			case '!':
				AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
				break;
			case '=':
				AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
				break;
			case '<':
				AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
				break;
			case '>':
				AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
				break;
			case '/':
				if (Match('/'))
				{
					while (Peek() != '\n' && !IsAtEnd()) Advance();
				}
				else
				{
					AddToken(TokenType.Slash);
				}
				break;
			case ' ':
			case '\r':
			case '\t':
				// Ignore whitespace.
				break;
			case '\n':
				_line++;
				break;
			case '"': String(); break;
			default:
				Lox.Error(_line, "Unexpected character.");
				break;
		}
	}

	private void String()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n') _line++;
			Advance();
		}

		if (IsAtEnd())
		{
			Lox.Error(_line, "Unterminated string.");
			return;
		}
		
		// The closing ".
		Advance();

		string value = source.Substring(_start + 1, _current - 1);
		AddToken(TokenType.String, value);
		
	}

	private bool Match(char expected)
	{
		if (IsAtEnd()) return false;
		if (source[_current] != expected) return false;

		_current++;
		return true;
	}

	private char Peek()
	{
		return IsAtEnd() ? '\0' : source[_current];
	}

	private bool IsAtEnd()
	{
		return _current >= source.Length;
	}

	private char Advance()
	{
		return source[_current++];
	}

	private void AddToken(TokenType type, object? literal = null)
	{
		string text = source.Substring(_start, _current);
		_tokens.Add(new Token(type, text, literal, _line));
	}
}
