namespace SomeBoard.Shared.Posting;

public class DeletePostDTO
{
    public Guid PostId { get; set; }
    public string Secret { get; set; } // Very secret admin secret
}