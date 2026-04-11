namespace SuperOverlay.iRacing.Telemetry.IRacing;

public sealed record IRacingNormalizedSnapshot(
    DateTimeOffset CapturedAtUtc,
    IRacingConnectionState Connection,
    IRacingVehicleState Vehicle,
    IRacingInputState Inputs,
    IRacingFlagState Flags,
    IRacingPitState Pit,
    IRacingTrackState Track,
    IRacingSessionStateBlock Session,
    IRacingRawTelemetrySnapshot RawTelemetry,
    IRacingRawSessionSnapshot? RawSession);
