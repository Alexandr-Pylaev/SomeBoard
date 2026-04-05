namespace SomeBoard.Shared.Posting;

public class ServerPostDTO
{
    public Guid PostId { get; set; }
    public string Author { get; set; }
    public string Message { get; set; }
    public DateTime PublishTime { get; set; }
}