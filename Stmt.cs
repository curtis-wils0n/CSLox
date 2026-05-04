namespace CSLox;

public abstract class Stmt
{
	public interface IVisitor
	{
		void VisitBlockStmt(Block stmt);
		void VisitExpressionStmt(Expression stmt);
		void VisitIfStmt(If stmt);
		void VisitPrintStmt(Print stmt);
		void VisitVarStmt(Var stmt);
		void VisitWhileStmt(While stmt);
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

	public class If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
	{
		public Expr Condition { get; } = condition;
		public Stmt ThenBranch { get; } = thenBranch;
		public Stmt? ElseBranch { get; } = elseBranch;

		public override void Accept(IVisitor visitor) => visitor.VisitIfStmt(this);
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

	public class While(Expr? condition, Stmt body) : Stmt
	{
		public Expr? Condition { get; } = condition;
		public Stmt Body { get; } = body;

		public override void Accept(IVisitor visitor) => visitor.VisitWhileStmt(this);
	}
}
