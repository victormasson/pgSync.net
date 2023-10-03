namespace PgSync;

public record Sequence(string Schema, string Name, string Column)
{
    public string FullName => $"{Schema}.{Name}";

    public override string ToString()
    {
        return FullName;
    }
}
