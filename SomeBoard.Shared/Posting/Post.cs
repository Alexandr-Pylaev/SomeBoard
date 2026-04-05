namespace SomeBoard.Shared.Posting;

/// <summary>
/// Main information about post on board
/// </summary>
public class Post
{
    public Post(string author, string message, DateTime publishTime)
    {
        Author = author;
        Message = message;
        PublishTime = publishTime;
    }

    public string Author { get; set; }
    public string Message { get; set; }
    public DateTime PublishTime { get; set; }

    public static readonly Post Empty = new Post(String.Empty,String.Empty, DateTime.MinValue);
}