namespace SomeBoard.Shared.Posting;

public class BoardInfoDTO
{
    public string Name { get; set; }
    public string Description{ get; set; }
    public long PostCount{ get; set; }

    public BoardInfoDTO SetPostCount(long count)
    {
        PostCount = count;
        return this;
    }
}