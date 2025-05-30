namespace Cyberjuice;

public static class CyberjuiceDbProperties
{
    public static string DbTablePrefix { get; set; } = "Cyberjuice";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Default";
}
