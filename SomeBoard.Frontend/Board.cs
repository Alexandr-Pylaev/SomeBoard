namespace SomeBoard.Frontend;

public record Board
{
    public string Name { get; set; } = "Board";
    public string Description  { get; set; } = "Default board description";
    public string BackendUrl { get; set; }
    public string Query { get; set; }
}