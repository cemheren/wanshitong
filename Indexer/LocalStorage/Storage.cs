using System;
using System.IO;
using Hanssens.Net;

public static class Storage
{
    private static LocalStorage _instance;

    public static LocalStorage Instance { get {

        if (_instance == null)
        {
            var currentDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var dirInfo = new DirectoryInfo(Path.Combine(currentDir, "Index"));

            var config = new LocalStorageConfiguration()
            {
                Filename = dirInfo + "/.localstorage"
            };

            _instance = new LocalStorage(config);
        }

        return _instance;
    } }

    public static bool TryGet<T>(this LocalStorage instance, string key, out T result)
    {
        if (instance.Exists(key))
        {
            result = instance.Get<T>(key);
            return true;
        }
        
        result = default(T);
        return false;
    }
}