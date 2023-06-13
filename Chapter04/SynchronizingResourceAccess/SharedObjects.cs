static class SharedObjects
{
    public static string? Message; // a shared resouce
    public static object Conch = new();
    public static int Counter = 0; // another shared resouce
}