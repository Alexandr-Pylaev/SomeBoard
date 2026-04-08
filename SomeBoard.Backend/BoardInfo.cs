using SomeBoard.Shared;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend;

public class BoardInfo : IDTOSerializable<BoardInfo, BoardInfoDTO>, IDTODeserializable<BoardInfoDTO>
{
    public string Name = "Some board";
    public string Description = "Default board description";
    public BoardInfo FromDTO(BoardInfoDTO dto)
    {
        Name = dto.Name;
        Description = dto.Description;

        return this;
    }

    public BoardInfoDTO ToDTO()
    {
        return new()
        {
            Name = Name,
            Description = Description,
        };
    }
}