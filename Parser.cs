namespace CSLox;

public class Parser(List<Token> tokens)
{
	private class ParseError : Exception;

	private int _current;

	private Expr Expression()
	{
		return Assignment();
	}

	private Stmt? Declaration()
	{
		try
		{
			return Match(TokenType.Var) ? VarDeclaration() : Statement();
		}
		catch (ParseError)
		{
			Synchronize();
			return null;
		}
	}

	private Stmt Statement()
	{
		if (Match(TokenType.For)) return ForStatement();
		if (Match(TokenType.If)) return IfStatement();
		if (Match(TokenType.Print)) return PrintStatement();
		if (Match(TokenType.While)) return WhileStatement();
		if (Match(TokenType.LeftBrace)) return new Stmt.Block(Block());

		return ExpressionStatement();
	}

	private Stmt ForStatement()
	{
		Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

		Stmt? initializer;
		if (Match(TokenType.Semicolon))
		{
			initializer = null;
		}
		else if (Match(TokenType.Var))
		{
			initializer = VarDeclaration();
		}
		else
		{
			initializer = ExpressionStatement();
		}

		Expr? condition = null;
		if (!Check(TokenType.Semicolon))
		{
			condition = Expression();
		}
		Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

		Expr? increment = null;
		if (!Check(TokenType.RightParen))
		{
			increment = Expression();
		}
		Consume(TokenType.RightParen, "Expect ')' after for clauses.");

		Stmt body = Statement();

		if (increment != null)
		{
			body = new Stmt.Block([body, new Stmt.Expression(increment)]);
		}

		condition ??= new Expr.Literal(true);
		body = new Stmt.While(condition, body);

		if (initializer != null)
		{
			body = new Stmt.Block([initializer, body]);
		}

		return body;
	}

	private Stmt.If IfStatement()
	{
		Consume(TokenType.LeftParen, "Expect '{' after 'if'.");
		var condition = Expression();
		Consume(TokenType.RightParen, "Expect ')' after if condition.");

		var thenBranch = Statement();
		Stmt? elseBranch = null;
		if (Match(TokenType.Else))
		{
			elseBranch = Statement();
		}

		return new Stmt.If(condition, thenBranch, elseBranch);
	}

	private Stmt.Print PrintStatement()
	{
		var value = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after value.");
		return new Stmt.Print(value);
	}

	private Stmt.Var VarDeclaration()
	{
		var name = Consume(TokenType.Identifier, "Expect variable name.");
		Expr? initializer = null;

		if (Match(TokenType.Equal))
		{
			initializer = Expression();
		}

		Consume(TokenType.Semicolon, "Expect ';' after initializer.");
		return new Stmt.Var(name, initializer);
	}

	private Stmt.While WhileStatement()
	{
		Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
		var condition = Expression();
		Consume(TokenType.RightParen, "Expect ')' after condition.");
		var body = Statement();

		return new Stmt.While(condition, body);
	}

	private Stmt.Expression ExpressionStatement()
	{
		var expr = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after expression.");
		return new Stmt.Expression(expr);
	}

	private List<Stmt?> Block()
	{
		var statements = new List<Stmt?>();
		while (!Check(TokenType.RightBrace) && !IsAtEnd())
		{
			statements.Add(Declaration());
		}

		Consume(TokenType.RightBrace, "Expect '}' after block.");
		return statements;
	}

	private Expr Assignment()
	{
		var expr = Or();

		if (!Match(TokenType.Equal)) return expr;
		var equals = Previous();
		var value = Assignment();

		if (expr is Expr.Variable variable)
		{
			var name = variable.Name;
			return new Expr.Assign(name, value);
		}

		Error(equals, "Invalid assignment target.");

		return expr;
	}

	private Expr Or()
	{
		var expr = And();

		while (Match(TokenType.Or))
		{
			var op = Previous();
			var right = And();
			expr = new Expr.Logical(expr, op, right);
		}

		return expr;
	}

	private Expr And()
	{
		var expr = Equality();

		while (Match(TokenType.And))
		{
			var op = Previous();
			var right = Equality();
			expr = new Expr.Logical(expr, op, right);
		}

		return expr;
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
		if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
		if (Match(TokenType.Identifier)) return new Expr.Variable(Previous());
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

	public List<Stmt?> Parse()
	{
		var statements = new List<Stmt?>();
		while (!IsAtEnd())
		{
			statements.Add(Declaration());
		}

		return statements;
	}
}
