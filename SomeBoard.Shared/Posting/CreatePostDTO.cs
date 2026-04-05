namespace SomeBoard.Shared.Posting;

public class CreatePostDTO
{
    public string Author { get; set; }
    public string Message { get; set; }
    public DateTime PublishTime { get; set; }
}