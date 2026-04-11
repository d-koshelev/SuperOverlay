using System.IO;
using System.Text.Json;
using IRSDKSharper;

namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingInventoryDumpService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<IRacingInventoryDumpResult> DumpAsync(string outputRootDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRootDirectory);

        Directory.CreateDirectory(outputRootDirectory);

        var sdk = new IRacingSdk();
        var telemetrySeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sessionSeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var connectedSeen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var failure = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);

        sdk.OnConnected += () => connectedSeen.TrySetResult();
        sdk.OnTelemetryData += () => telemetrySeen.TrySetResult();
        sdk.OnSessionInfo += () => sessionSeen.TrySetResult();
        sdk.OnException += ex => failure.TrySetResult(ex);

        sdk.UpdateInterval = 1;
        sdk.Start();

        try
        {
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
            var waitTask = Task.WhenAll(connectedSeen.Task, telemetrySeen.Task, sessionSeen.Task);
            var completed = await Task.WhenAny(waitTask, failure.Task, timeoutTask).ConfigureAwait(false);

            if (completed == failure.Task)
            {
                throw new InvalidOperationException("IRSDKSharper reported an exception while reading iRacing data.", await failure.Task.ConfigureAwait(false));
            }

            if (completed == timeoutTask)
            {
                throw new TimeoutException("Timed out waiting for iRacing telemetry/session data. Ensure iRacing is running and memory telemetry is enabled.");
            }

            await waitTask.ConfigureAwait(false);

            if (!sdk.IsConnected)
            {
                throw new InvalidOperationException("iRacing telemetry is not connected.");
            }

            var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            var dumpDirectory = Path.Combine(outputRootDirectory, $"iracing-inventory-{stamp}");
            Directory.CreateDirectory(dumpDirectory);

            var yamlPath = Path.Combine(dumpDirectory, "session-info.yaml");
            var jsonPath = Path.Combine(dumpDirectory, "inventory.json");

            var yaml = sdk.Data.SessionInfoYaml ?? string.Empty;
            File.WriteAllText(yamlPath, yaml);

            var variables = sdk.Data.TelemetryDataProperties
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x => new IRacingTelemetryVariableInventoryEntry(
                    x.Value.Name ?? x.Key,
                    x.Value.Desc ?? string.Empty,
                    x.Value.Unit ?? string.Empty,
                    x.Value.VarType.ToString(),
                    x.Value.Offset,
                    x.Value.Count,
                    x.Value.CountAsTime,
                    TryReadSampleValue(sdk, x.Key, x.Value.Count)))
                .ToArray();

            var document = new IRacingInventoryDumpDocument(
                DateTimeOffset.UtcNow,
                sdk.IsConnected,
                sdk.Data.Version,
                sdk.Data.Status,
                sdk.Data.TickRate,
                sdk.Data.SessionInfoUpdate,
                sdk.Data.SessionInfoLength,
                sdk.Data.SessionInfoOffset,
                sdk.Data.VarCount,
                sdk.Data.VarHeaderOffset,
                sdk.Data.BufferCount,
                sdk.Data.BufferLength,
                sdk.Data.TickCount,
                sdk.Data.Offset,
                sdk.Data.FramesDropped,
                variables,
                Path.GetFileName(yamlPath));

            File.WriteAllText(jsonPath, JsonSerializer.Serialize(document, JsonOptions));

            return new IRacingInventoryDumpResult(dumpDirectory, jsonPath, yamlPath, variables.Length);
        }
        finally
        {
            sdk.Stop();
        }
    }

    private static string? TryReadSampleValue(IRacingSdk sdk, string name, int count)
    {
        try
        {
            if (count <= 1)
            {
                var value = sdk.Data.GetValue(name);
                return value?.ToString();
            }

            var value0 = sdk.Data.GetValue(name, 0);
            return value0 is null ? null : $"{value0} (+{count - 1} more)";
        }
        catch
        {
            return null;
        }
    }
}
