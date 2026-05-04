namespace CSLox;

public class Parser(List<Token> tokens)
{
	private class ParseError : Exception;

	private int _current;

	private Expr Expression()
	{
		return Equality();
	}

	private Expr Equality()
	{
		var expr = Comparison();

		while (Match(TokenType.BangEqual, TokenType.EqualEqual))
		{
			var op = Previous();
			var right = Comparison();
			expr = new Expr.Binary(expr, op, right);
		}

		return expr;
	}

	private Expr Comparison()
	{
		var expr = Term();

		while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
		{
			var op = Previous();
			var right = Term();
			expr = new Expr.Binary(expr, op, right);
		}

		return expr;
	}

	private Expr Term()
	{
		var expr = Factor();

		while (Match(TokenType.Minus, TokenType.Plus))
		{
			var op = Previous();
			var right = Factor();
			expr = new Expr.Binary(expr, op, right);
		}

		return expr;
	}

	private Expr Factor()
	{
		var expr = Unary();

		while (Match(TokenType.Slash, TokenType.Star))
		{
			var op = Previous();
			var right = Unary();
			expr = new Expr.Binary(expr, op, right);
		}

		return expr;
	}

	private Expr Unary()
	{
		if (!Match(TokenType.Bang, TokenType.Minus)) return Primary();
		var op = Previous();
		var right = Unary();
		return new Expr.Unary(op, right);
	}

	private Expr Primary()
	{
		if (Match(TokenType.False)) return new Expr.Literal(false);
		if (Match(TokenType.True)) return new Expr.Literal(true);
		if (Match(TokenType.Nil)) return new Expr.Literal(null);

		if (Match(TokenType.Number, TokenType.String))
		{
			return new Expr.Literal(Previous().Literal);
		}

		if (!Match(TokenType.LeftParen)) throw Error(Peek(), "Expected expression.");

		var expr = Expression();
		Consume(TokenType.RightParen, "Expect ')' after expression.");
		return new Expr.Grouping(expr);

	}

	private bool Match(params TokenType[] types)
	{
		if (!types.Any(Check)) return false;
		Advance();
		return true;
	}

	private Token Consume(TokenType type, string message)
	{
		return Check(type) ? Advance() : throw Error(Peek(), message);
	}

	private bool Check(TokenType type)
	{
		if (IsAtEnd()) return false;
		return Peek().Type == type;
	}

	private Token Advance()
	{
		if (!IsAtEnd()) _current++;
		return Previous();
	}

	private bool IsAtEnd()
	{
		return Peek().Type == TokenType.EndOfFile;
	}

	private Token Peek()
	{
		return tokens[_current];
	}

	private Token Previous()
	{
		return tokens[_current - 1];
	}

	private static ParseError Error(Token token, string message)
	{
		Lox.Error(token, message);
		return new ParseError();
	}

	private void Synchronize()
	{
		Advance();

		while (!IsAtEnd())
		{
			if (Previous().Type == TokenType.Semicolon) return;

			switch (Peek().Type)
			{
				case TokenType.Class:
				case TokenType.Fun:
				case TokenType.Var:
				case TokenType.For:
				case TokenType.If:
				case TokenType.While:
				case TokenType.Print:
				case TokenType.Return:
					return;
			}

			Advance();
		}
	}

	public Expr? Parse()
	{
		try
		{
			return Expression();
		}
		catch
		{
			return null;
		}
	}
}
