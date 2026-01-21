using System.Diagnostics;
using DotNet.Testcontainers.Containers;
using Testcontainers.Nats;

namespace TestRunner;

public class GameInfraContext : IAsyncDisposable
{
    private IContainer? _natsContainer;
    private const string ContainerName = "mmo-nats-dev"; // 고정 컨테이너 이름
    private const int FixedPort = 4222; // 고정 포트

    public static string NatsUrl => $"nats://127.0.0.1:{FixedPort}";
    
    public async Task StartAsync()
    {
        Console.WriteLine("[Infra] Starting NATS Container...");
        
        KillContainerIfExists(ContainerName);
        
        var builder = new NatsBuilder("nats:latest")
            .WithName(ContainerName)
            .WithPortBinding(FixedPort, FixedPort);

        _natsContainer = builder.Build();
        
        await _natsContainer.StartAsync();
        Console.WriteLine($"[Infra] Starting NATS Started at: {NatsUrl}");
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_natsContainer != null)
            await _natsContainer.DisposeAsync();
        
        Console.WriteLine("[Infra] NATS Container Disposed.");
        
        GC.SuppressFinalize(this);
    }
    
    private void KillContainerIfExists(string containerName)
    {
        try
        {
            // "docker rm -f {이름}" 명령을 실행해서 강제로 날려버림
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"rm -f {containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
            
            // 삭제 성공 여부는 중요하지 않음 (없으면 에러 날 테니 무시)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Infra] Warning during cleanup: {ex.Message}");
        }
    }
}