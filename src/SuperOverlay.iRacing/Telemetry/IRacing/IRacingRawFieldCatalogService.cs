namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed class IRacingRawFieldCatalogService
{
    public IReadOnlyList<IRacingTelemetryVariableInventoryEntry> BuildTelemetryCatalog(IRacingTelemetryRawState? telemetryState)
        => telemetryState?.Catalog ?? Array.Empty<IRacingTelemetryVariableInventoryEntry>();

    public IReadOnlyList<string> BuildSessionInfoCatalog(IRacingSessionInfoRawState? sessionState)
        => sessionState?.Fields.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();
}
