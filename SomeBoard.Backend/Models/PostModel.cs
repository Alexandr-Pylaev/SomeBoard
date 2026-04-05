using System.ComponentModel.DataAnnotations;
using SomeBoard.Shared;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend.Models;

public class PostModel : IDTODeserializable<ServerPostDTO>, IDTOSerializable<PostModel, CreatePostDTO>
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

    ServerPostDTO IDTODeserializable<ServerPostDTO>.ToDTO()
    {
        return new ServerPostDTO()
        {
            Author = Author,
            Message = Message,
            PostId = PostId,
            PublishTime = PublishTime
        };
    }

    PostModel IDTOSerializable<PostModel, CreatePostDTO>.FromDTO(CreatePostDTO dto)
    {
        Author = dto.Author;
        Message = dto.Message;
        return this;
    }
}