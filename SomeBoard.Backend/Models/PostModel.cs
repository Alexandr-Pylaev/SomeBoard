using System.ComponentModel.DataAnnotations;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend.Models;

public class PostModel
{
    [Key]
    public Guid PostId { get; set; }
    public Post Post { get; set; }
    public bool Deleted { get; private set; }

    public void DeletePost() => Deleted = true;

    public PostModel(Post post)
    {
        Post = post;
    }
}