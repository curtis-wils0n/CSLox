namespace CSLox;

public abstract class Expr
{
	public interface IVisitor<out TR>
	{
		TR VisitBinaryExpr(Binary expr);
		TR VisitGroupingExpr(Grouping expr);
		TR VisitLiteralExpr(Literal expr);
		TR VisitUnaryExpr(Unary expr);
	}

	public abstract TR Accept<TR>(IVisitor<TR> visitor);

	public class Binary(Expr left, Token op, Expr right) : Expr
	{
		public Expr Left { get; } = left;
		public Token Op { get; } = op;
		public Expr Right { get; } = right;

		public override TR Accept<TR>(IVisitor<TR> visitor) => visitor.VisitBinaryExpr(this);
	}

	public class Grouping(Expr expression) : Expr
	{
		public Expr Expression { get; } = expression;

		public override TR Accept<TR>(IVisitor<TR> visitor) => visitor.VisitGroupingExpr(this);
	}

	public class Literal(object? value) : Expr
	{
		public object? Value { get; } = value;

		public override TR Accept<TR>(IVisitor<TR> visitor) => visitor.VisitLiteralExpr(this);
	}

	public class Unary(Token op, Expr right) : Expr
	{
		public Token Op { get; } = op;
		public Expr Right { get; } = right;

		public override TR Accept<TR>(IVisitor<TR> visitor) => visitor.VisitUnaryExpr(this);
	}
}
