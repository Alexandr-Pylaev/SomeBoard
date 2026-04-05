namespace SomeBoard.Shared;

public interface IDTOSerializable <TSelf, TDTO>
{
    public TSelf FromDTO(TDTO dto);
    public static IDTOSerializable<TSelf, TDTO> Convert<T>(TSelf val) where T : IDTOSerializable<TSelf, TDTO> => (IDTOSerializable<TSelf, TDTO>) val!;
}