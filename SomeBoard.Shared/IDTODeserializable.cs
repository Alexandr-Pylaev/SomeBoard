namespace SomeBoard.Shared;

public interface IDTODeserializable<TDTO>
{
    public TDTO ToDTO();
}