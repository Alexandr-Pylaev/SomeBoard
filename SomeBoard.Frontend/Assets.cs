namespace SomeBoard.Frontend;

public class Assets
{
    public const string CONFIG_PATH = "SomeBoard";
    public const string BOARDS_PATH = $"{CONFIG_PATH}:Boards";
    public const string DEFAULT_BOARD_ADDR = $"{CONFIG_PATH}:DefaultBoard";
    public const string BOARDS_QUERY_NAME = "addr";
    public static Assets Singleton => _singleton.Value;
    private static readonly Lazy<Assets> _singleton = new();

    public Board[] Boards { get; private set; }= null!;
    public Board DefaultBoard = null!;
    public static Board FailedBoard => new Board() { Name = "Unknown", Description = "Very unknown board" };
    public void Initialize(IConfiguration configuration)
    {
        Boards = configuration.GetSection(BOARDS_PATH).Get<Board[]>() ?? [];
        var defaultBoardAddr = configuration.GetValue<string?>(DEFAULT_BOARD_ADDR);
        DefaultBoard = Boards.FirstOrDefault(x => x.Query == defaultBoardAddr) ?? Boards.First();
    }
}