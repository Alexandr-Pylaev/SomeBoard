namespace SomeBoard.Frontend;

public record Board
{
    public string? Name { get; set; } = null;
    public string? Description  { get; set; } = null;
    public string BackendUrl { get; set; }
    public string Query { get; set; }

    public long PostCount;
}