using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public static class Telemetry
{
    private static TelemetryClient _instance;

    public static TelemetryClient Instance { get {

        if (_instance == null)
        {
            var configuration = new TelemetryConfiguration("8aebb6ee-8f56-4661-8a41-6c025904e324");
            _instance = new TelemetryClient(configuration);
            _instance.Context.Component.Version = "0.0.21";
        }

        return _instance;
    } }
}