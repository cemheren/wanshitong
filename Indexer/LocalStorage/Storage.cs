using System;
using System.Configuration;
using System.IO;
using Hanssens.Net;

public class Storage : IDisposable
{
    public LocalStorage Instance;

    public Storage()
    {
        var rootDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if(ConfigurationManager.AppSettings["rootFolderPath"] != null)
        {
            rootDir = ConfigurationManager.AppSettings["rootFolderPath"];
        }

        var dirInfo = new DirectoryInfo(rootDir);

        var config = new LocalStorageConfiguration()
        {
            Filename = dirInfo + "/.localstorage"
        };

        Instance = new LocalStorage(config);
    }

    public bool TryGet<T>(string key, out T result)
    {
        if (Instance.Exists(key))
        {
            result = Instance.Get<T>(key);
            return true;
        }
        
        result = default(T);
        return false;
    }

    public T GetOrDefault<T>(string key, T def)
    {
        if (Instance.Exists(key))
        {
            return Instance.Get<T>(key);
        }

        return def;
    }

    public void Dispose()
    {
        Instance.Dispose();
    }

}