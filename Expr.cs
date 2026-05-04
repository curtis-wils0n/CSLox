namespace CSLox;

public abstract class Expr
{
	public interface IVisitor<out TResult>
	{
		TResult VisitBinaryExpr(Binary expr);
		TResult VisitGroupingExpr(Grouping expr);
		TResult VisitLiteralExpr(Literal expr);
		TResult VisitUnaryExpr(Unary expr);
	}

	public abstract TResult Accept<TResult>(IVisitor<TResult> visitor);

	public class Binary(Expr left, Token op, Expr right) : Expr
	{
		public Expr Left { get; } = left;
		public Token Operator { get; } = op;
		public Expr Right { get; } = right;

		public override TResult Accept<TResult>(IVisitor<TResult> visitor) => visitor.VisitBinaryExpr(this);
	}

	public class Grouping(Expr expression) : Expr
	{
		public Expr Expression { get; } = expression;

		public override TResult Accept<TResult>(IVisitor<TResult> visitor) => visitor.VisitGroupingExpr(this);
	}

	public class Literal(object? value) : Expr
	{
		public object? Value { get; } = value;

		public override TResult Accept<TResult>(IVisitor<TResult> visitor) => visitor.VisitLiteralExpr(this);
	}

	public class Unary(Token op, Expr right) : Expr
	{
		public Token Operator { get; } = op;
		public Expr Right { get; } = right;

		public override TResult Accept<TResult>(IVisitor<TResult> visitor) => visitor.VisitUnaryExpr(this);
	}
}
