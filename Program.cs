namespace CSLox;

public class Lox
{
	private static bool _hadError;

	public static void Main(string[] args)
	{
		switch (args.Length)
		{
			case > 1:
				Console.WriteLine("Usage: cslox [script]");
				Environment.Exit(64);
				break;
			case 1:
				RunFile(args[0]);
				break;
			default:
				RunPrompt();
				break;
		}
	}

	private static void RunFile(string path)
	{
		var text = File.ReadAllText(path);
		Run(text);

		if (_hadError) Environment.Exit(65);
	}

	private static void RunPrompt()
	{
		while (true)
		{
			Console.Write("> ");
			var line = Console.ReadLine();
			if (line == null) break;
			Run(line);
			_hadError = false;
		}
	}

	private static void Run(string source)
	{
		var scanner = new Scanner(source);
		var tokens = scanner.ScanTokens();
		foreach (var token in tokens)
		{
			Console.WriteLine(token.ToString());
		}
	}

	public static void Error(int line, string message)
	{
		Report(line, "", message);
	}

	private static void Report(int line, string where, string message)
	{
		Console.WriteLine($"[line {line}] Error{where}: {message}");
		_hadError = true;
	}
}
