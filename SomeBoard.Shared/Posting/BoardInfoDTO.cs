namespace SomeBoard.Shared.Posting;

public class BoardInfoDTO
{
    public string Name;
    public string Description;
    public long PostCount;

    public BoardInfoDTO SetPostCount(long count)
    {
        PostCount = count;
        return this;
    }
}