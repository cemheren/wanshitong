using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public class Telemetry : IDisposable
{
    public static string Version = "default_ver";

    public Telemetry(Storage storage)
    {
        this.storage = storage;

        var id = GetInstanceId();
        var configuration = new TelemetryConfiguration("8aebb6ee-8f56-4661-8a41-6c025904e324");
        client = new TelemetryClient(configuration);
        client.Context.Component.Version = Version;
    
        client.Context.User.Id = id;
        client.Context.Session.Id = id;
    }

    public TelemetryClient client;

    private readonly Storage storage;

    private string GetInstanceId()
    {
        string instanceId;
        if(this.storage.Instance.Exists("instanceId"))
        {
            instanceId = this.storage.Instance.Get<string>("instanceId");
        }
        else
        {
            instanceId = Guid.NewGuid().ToString();
            this.storage.Instance.Store("instanceId", instanceId);
            this.storage.Instance.Persist();
        }

        return instanceId;
    }

    public void Dispose()
    {
        client.TrackEvent("ProgramExit");
        client.Flush();
    }

}