using System;
using System.Configuration;
using System.IO;
using Hanssens.Net;

public class Storage : IDisposable
{
    public LocalStorage Instance;

    public Storage()
    {
        var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Wanshitong");
        var dirInfo = new DirectoryInfo(rootDir);
        dirInfo.Create();

        var config = new LocalStorageConfiguration()
        {
            Filename = dirInfo + "/.localstorage"
        };

        Instance = new LocalStorage(config);

        if (Instance.Exists("rootFolderPath") == false)
        {
            Instance.Store<string>("rootFolderPath", rootDir);
        }
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