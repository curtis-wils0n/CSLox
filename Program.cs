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
                Environment.Exit(0x667);
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
        byte[] bytes = File.ReadAllBytes(path);
        Run(System.Text.Encoding.Default.GetString(bytes));

        if (_hadError) Environment.Exit(0xA0);
    }

    private static void RunPrompt()
    {
        var reader = new StreamReader(Console.OpenStandardInput());
        for (;;)
        {
            Console.Write("> ");
            var line = reader.ReadLine();
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
        Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
        _hadError = true;
    }
}