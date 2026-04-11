namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingInventoryDumpResult(
    string DirectoryPath,
    string InventoryJsonPath,
    string SessionYamlPath,
    int VariableCount);
