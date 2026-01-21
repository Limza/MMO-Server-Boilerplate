using NATS.Client.Core;
using NATS.Client.Serializers.Json;

namespace Server;

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
        Console.WriteLine($"[Server] Launching... NATS url: {natsUrl}");

        // NATS test
        {
            var opts = new NatsOpts 
            { 
                Url = natsUrl, 
                Name = "GameServer",
                SerializerRegistry = NatsJsonSerializerRegistry.Default
            };

            await using var nats = new NatsConnection(opts);
            await nats.ConnectAsync();

            // 워커가 켜질 때까지 아주 잠깐 대기 (Race Condition 방지용 단순 대기)
            await Task.Delay(1000, ct); 
            Console.WriteLine("[Server] Sending 'Ping' to DbWorker...");
            
            try 
            {
                // 요청 보내기 (RequestAsync)
                var reply = await nats.RequestAsync<string, string>(
                    "db.echo", "Hello DB Worker!", cancellationToken: ct);

                // 결과 확인
                Console.WriteLine($"[Server] Reply Received: '{reply.Data}'");
            }
            catch (NatsNoRespondersException)
            {
                Console.WriteLine("[Server] No Responders! (DbWorker가 아직 안 켜졌거나 주제가 틀림)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error: {ex.Message}");
            }
        }

        try
        {
            await Task.Delay(-1, ct);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[Server] Shutdown signal received");
        }
        finally
        {
            Console.WriteLine("[Server] Shutdown");
        }
        
        Console.WriteLine("[Server] Stopped");
    }
}