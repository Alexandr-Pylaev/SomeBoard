namespace SomeBoard.Shared;

public interface IDTOSerializable <TSelf, TDTO>
{
    public TSelf FromDTO(TDTO dto);
}