namespace CSLox;

public abstract class Stmt
{
	public interface IVisitor
	{
		void VisitBlockStmt(Block stmt);
		void VisitExpressionStmt(Expression stmt);
		void VisitPrintStmt(Print stmt);
		void VisitVarStmt(Var stmt);
	}

	public abstract void Accept(IVisitor visitor);

	public class Block(List<Stmt?> statements) : Stmt
	{
		public List<Stmt?> Statements { get; } = statements;

		public override void Accept(IVisitor visitor) => visitor.VisitBlockStmt(this);
	}

	public class Expression(Expr expression) : Stmt
	{
		public Expr InnerExpression { get; } = expression;

		public override void Accept(IVisitor visitor) => visitor.VisitExpressionStmt(this);
	}

	public class Print(Expr expression) : Stmt
	{
		public Expr InnerExpression { get; } = expression;

		public override void Accept(IVisitor visitor) => visitor.VisitPrintStmt(this);
	}

	public class Var(Token name, Expr? initializer) : Stmt
	{
		public Token Name { get; } = name;
		public Expr? Initializer { get; } = initializer;

		public override void Accept(IVisitor visitor) => visitor.VisitVarStmt(this);
	}
}
