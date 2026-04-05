using System.ComponentModel.DataAnnotations;

namespace SomeBoard.Backend.Models;

public class PostModel
{
    [Key]
    public Guid PostId { get; set; }
    public string Author { get; set; }
    public string Message { get; set; }
    public DateTime PublishTime { get; set; }
    public bool Deleted { get; private set; }

    public void DeletePost() => Deleted = true;

    public PostModel(string author = "", string message = "", DateTime? publishTime = null)
    {
        Author = author;
        Message = message;
        PublishTime = publishTime ?? DateTime.Now;
    }
}