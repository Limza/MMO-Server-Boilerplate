namespace TestRunner;

internal static class Program
{
    private static async Task Main()
    {
        WriteColorLine(">>> [TestRunner] Initializing Infra Environment", ConsoleColor.Green);
        await using var infra = new GameInfraContext();
        await infra.StartAsync();

        using var cts = new CancellationTokenSource();

        WriteColorLine(">>> Launching Program Server & DbWorker", ConsoleColor.Green);
        string natsUrl = GameInfraContext.NatsUrl;
        var ctsToken = cts.Token;
        var serverTask = Task.Run(() => Server.Program.Main(natsUrl, ctsToken), ctsToken);
        var dbWorkerTask = Task.Run(() => DbWorker.Program.Main(natsUrl, ctsToken), ctsToken);

        WriteColorLine(">>> [System Ready] Press Enter to shutdown...", ConsoleColor.Green);
        Console.ReadLine();

        WriteColorLine(">>> Shutting down...", ConsoleColor.Green);
        await cts.CancelAsync();
        await Task.WhenAll(serverTask, dbWorkerTask);

        WriteColorLine(">>> Shutdown", ConsoleColor.Green);
    }

    private static void WriteColorLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}