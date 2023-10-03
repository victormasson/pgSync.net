using System.ComponentModel.DataAnnotations;

namespace PgSync.Model;
public record ConfigurationFile
{
    [Required]
    public DatabasesConnection DatabasesConnection { get; set; } = new DatabasesConnection();

    [Required]
    public List<Group> Groups { get; set; } = new List<Group>();
}

public record Group
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public List<Table> Tables { get; set; } = new List<Table>();
}

public record Table
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string WhereCondition { get; set; } = string.Empty;
    public string DeleteConditionTarget { get; set; } = string.Empty;
}

public record DatabasesConnection
{
    [Required]
    public string From { get; set; } = string.Empty;

    [Required]
    public string To { get; set; } = string.Empty;
}
