namespace CSLox;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor
{
	public object? VisitLiteralExpr(Expr.Literal expr)
	{
		return expr.Value;
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

	public void Interpret(List<Stmt> statements)
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

	private object? Evaluate(Expr expr)
	{
		return expr.Accept(this);
	}

	private void Execute(Stmt stmt)
	{
		stmt.Accept(this);
	}

	public void VisitExpressionStmt(Stmt.Expression stmt)
	{
		Evaluate(stmt.InnerExpression);
	}

	public void VisitPrintStmt(Stmt.Print stmt)
	{
		var value = Evaluate(stmt.InnerExpression);
		if (value != null) Console.WriteLine(Stringify(value));
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
