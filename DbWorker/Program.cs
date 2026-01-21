using NATS.Client.Core;
using NATS.Client.Serializers.Json;

namespace DbWorker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string natsUrl = args.Length > 0 ? args[0] : "localhost";
        await Main(natsUrl, CancellationToken.None);
    }

    // TestRunner Entry Point
    public static async Task Main(string natsUrl, CancellationToken ct)
    {
        Console.WriteLine($"[DbWorker] Launching... NATS url: {natsUrl}");

        // NATS test
        {
            var opts = new NatsOpts()
            { 
                Url = natsUrl, 
                Name = "DbWorker",
                SerializerRegistry = NatsJsonSerializerRegistry.Default
            };
            
            await using var nats = new NatsConnection(opts);
            await nats.ConnectAsync();
            
            await foreach (var msg in nats.SubscribeAsync<string>(
                               "db.echo", cancellationToken: ct))
            {
                Console.WriteLine($"[DbWorker] Received: {msg.Data}");
                await msg.ReplyAsync($"Pong (Echo: {msg.Data})", cancellationToken: ct);
            }
        }

        try
        {
            await Task.Delay(-1, ct);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[DbWorker] Shutdown signal received");
        }
        finally
        {
            Console.WriteLine("[DbWorker] Shutdown");
        }
        
        Console.WriteLine("[DbWorker] Stopped");
    }
}