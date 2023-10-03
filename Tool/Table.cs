namespace PgSync;

public record Table(string Schema, string Name)
{
    public string FullName => $"{Schema}.{Name}";

    public override string ToString()
    {
        return FullName;
    }
}
