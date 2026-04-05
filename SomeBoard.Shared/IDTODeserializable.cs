namespace SomeBoard.Shared;

public interface IDTODeserializable<TDTO>
{
    public TDTO ToDTO();
    public static IDTODeserializable<TDTO>? Convert<T>(T? val) where T : IDTODeserializable<TDTO> => val;
}