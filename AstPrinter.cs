using System.Text;
namespace CSLox;

public class AstPrinter : Expr.IVisitor<string>
{
	public string Print(Expr expr)
	{
		return expr.Accept(this);
	}

	public string VisitAssignExpr(Expr.Assign expr)
	{
		return Parenthesize("= " + expr.Name.Lexeme, expr.Value);
	}

	public string VisitBinaryExpr(Expr.Binary expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
	}

	public string VisitCallExpr(Expr.Call expr)
	{
		var args = new Expr[expr.Arguments.Count + 1];
		args[0] = expr.Callee;
		expr.Arguments.CopyTo(args, 1);
		return Parenthesize("call", args);
	}

	public string VisitGroupingExpr(Expr.Grouping expr)
	{
		return Parenthesize("group", expr.Expression);
	}

	public string VisitLiteralExpr(Expr.Literal expr)
	{
		return expr.Value?.ToString() ?? "nil";
	}
	
	public string VisitLogicalExpr(Expr.Logical expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
	}

	public string VisitUnaryExpr(Expr.Unary expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Right);
	}
	public string VisitVariableExpr(Expr.Variable expr)
	{
		return expr.Name.Lexeme;
	}

	private string Parenthesize(string name, params Expr[] exprs)
	{
		var builder = new StringBuilder();

		builder.Append('(').Append(name);
		foreach (var expr in exprs)
		{
			builder.Append(' ');
			builder.Append(expr.Accept(this));
		}
		builder.Append(')');

		return builder.ToString();
	}
}
