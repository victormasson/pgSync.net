using Spectre.Console;

namespace PgSync.Model;

public static class StyleApp
{
    public static Style BasicStype { get; } = new Style(foreground: Color.Aqua, decoration: Decoration.None);
}