using Hanssens.Net;

public static class Storage
{
    private static LocalStorage _instance;

    public static LocalStorage Instance { get {

        if (_instance == null)
        {
            _instance = new LocalStorage();
        }

        return _instance;
    } }
}