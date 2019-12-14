using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public static class Telemetry
{
    private static TelemetryClient _instance;

    public static TelemetryClient Instance { get {

        if (_instance == null)
        {
            var id = GetInstanceId();
            var configuration = new TelemetryConfiguration("8aebb6ee-8f56-4661-8a41-6c025904e324");
            _instance = new TelemetryClient(configuration);
            _instance.Context.Component.Version = "0.0.33";
        
            _instance.Context.User.Id = id;
            _instance.Context.Session.Id = id;
        }

        return _instance;
    } }

    private static string GetInstanceId()
    {
        string instanceId;
        if(Storage.Instance.Exists("instanceId"))
        {
            instanceId = Storage.Instance.Get<string>("instanceId");
        }
        else
        {
            instanceId = Guid.NewGuid().ToString();
            Storage.Instance.Store("instanceId", instanceId);
            Storage.Instance.Persist();
        }

        return instanceId;
    }
}