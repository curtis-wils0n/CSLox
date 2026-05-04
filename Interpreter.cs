namespace CSLox;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor
{
	private Environment _environment = new();

	public object? VisitLiteralExpr(Expr.Literal expr)
	{
		return expr.Value;
	}

	public object? VisitLogicalExpr(Expr.Logical expr)
	{
		var left = Evaluate(expr.Left);

		if (expr.Operator.Type == TokenType.Or)
		{
			if (IsTruthy(left)) return left;
		}
		else
		{
			if (!IsTruthy(left)) return left;
		}

		return Evaluate(expr.Right);
	}

	public object? VisitGroupingExpr(Expr.Grouping expr)
	{
		return Evaluate(expr.Expression);
	}

	public object? VisitUnaryExpr(Expr.Unary expr)
	{
		var right = Evaluate(expr.Right);

		return expr.Operator.Type switch
		{
			TokenType.Bang => !IsTruthy(right),
			TokenType.Minus => -CheckNumberOperand(expr.Operator, right),
			_ => throw new InvalidOperationException($"Unreachable unary operator: {expr.Operator.Type}")
		};
	}
	public object? VisitVariableExpr(Expr.Variable expr)
	{
		return _environment.Get(expr.Name);
	}

	public void Interpret(List<Stmt?> statements)
	{
		try
		{
			foreach (var statement in statements)
			{
				Execute(statement);
			}
		}
		catch (RuntimeError err)
		{
			Lox.RuntimeError(err);
		}
	}

	private static bool IsTruthy(object? obj)
	{
		return obj switch
		{
			null => false,
			bool b => b,
			_ => true
		};
	}

	private static string? Stringify(object obj)
	{
		switch (obj)
		{
			case null:
				return "nil";
			case double:
			{
				var text = obj.ToString();
				if (text != null && text.EndsWith(".0"))
				{
					text = text[..^2];
				}
				return text;
			}
			default:
				return obj.ToString();
		}
	}

	private object? Evaluate(Expr? expr)
	{
		return expr?.Accept(this);
	}

	private void Execute(Stmt? stmt)
	{
		stmt?.Accept(this);
	}

	private void ExecuteBlock(List<Stmt?> statements, Environment environment)
	{
		var previous = _environment;
		try
		{
			_environment = environment;
			foreach (var statement in statements)
			{
				Execute(statement);
			}
		}
		finally
		{
			_environment = previous;
		}
	}

	public void VisitBlockStmt(Stmt.Block stmt)
	{
		ExecuteBlock(stmt.Statements, new Environment(_environment));
	}

	public void VisitExpressionStmt(Stmt.Expression stmt)
	{
		Evaluate(stmt.InnerExpression);
	}

	public void VisitIfStmt(Stmt.If stmt)
	{
		if (IsTruthy(Evaluate(stmt.Condition)))
		{
			Execute(stmt.ThenBranch);
		}
		else if (stmt.ElseBranch != null)
		{
			Execute(stmt.ElseBranch);
		}
	}

	public void VisitPrintStmt(Stmt.Print stmt)
	{
		var value = Evaluate(stmt.InnerExpression);
		if (value != null) Console.WriteLine(Stringify(value));
	}

	public void VisitVarStmt(Stmt.Var stmt)
	{
		var value = Evaluate(stmt.Initializer);

		_environment.Define(stmt.Name.Lexeme, value);
	}

	public void VisitWhileStmt(Stmt.While stmt)
	{
		while (IsTruthy(Evaluate(stmt.Condition)))
		{
			Execute(stmt.Body);
		}
	}

	public object? VisitAssignExpr(Expr.Assign expr)
	{
		var value = Evaluate(expr.Value);
		_environment.Assign(expr.Name, value);
		return value;
	}

	public object? VisitBinaryExpr(Expr.Binary expr)
	{
		var left = Evaluate(expr.Left);
		var right = Evaluate(expr.Right);

		return expr.Operator.Type switch
		{
			TokenType.Greater => CheckNumberOperands(expr.Operator, left, right, out var rightValue) > rightValue,
			TokenType.GreaterEqual => CheckNumberOperands(expr.Operator, left, right, out var rightValue) >= rightValue,
			TokenType.Less => CheckNumberOperands(expr.Operator, left, right, out var rightValue) < rightValue,
			TokenType.LessEqual => CheckNumberOperands(expr.Operator, left, right, out var rightValue) <= rightValue,
			TokenType.BangEqual => !Equals(left, right),
			TokenType.EqualEqual => Equals(left, right),
			TokenType.Minus => CheckNumberOperands(expr.Operator, left, right, out var rightValue) - rightValue,
			TokenType.Slash => CheckNumberOperands(expr.Operator, left, right, out var rightValue) / rightValue,
			TokenType.Star => CheckNumberOperands(expr.Operator, left, right, out var rightValue) * rightValue,
			TokenType.Plus => (left, right) switch
			{
				(double l, double r) => l + r,
				(string ls, string rs) => ls + rs,
				_ => throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.")
			},
			_ => throw new InvalidOperationException($"Unreachable binary operator: {expr.Operator.Type}")
		};
	}

	private static double CheckNumberOperand(Token op, object? operand)
	{
		return operand is not double d ? throw new RuntimeError(op, "Operand must be a number.") : d;
	}

	private static double CheckNumberOperands(Token op, object? left, object? right, out double rightValue)
	{
		if (left is not double l || right is not double r) throw new RuntimeError(op, "Operands must be two numbers.");
		rightValue = r;
		return l;
	}
}
