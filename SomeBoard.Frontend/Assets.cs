namespace SomeBoard.Frontend;

public class Assets
{
    public const string CONFIG_PATH = "SomeBoard";
    public const string BOARD_DESC_PATH = $"{CONFIG_PATH}:Board:Description";
    public const string BOARD_NAME_PATH = $"{CONFIG_PATH}:Board:Name";
    public const string BOARD_BACKEND_URL_PATH = $"{CONFIG_PATH}:Board:Backend:URL";
    public static Assets Singleton => _singleton.Value;
    private static readonly Lazy<Assets> _singleton = new();

    public string BoardDescription = "Public text board";
    public string BoardName = "SomeBoard";
    public string? BoardBackendUrl;

    public void Initialize(IConfiguration configuration)
    {
        BoardDescription = configuration.GetValue<string>(BOARD_DESC_PATH, BoardDescription);
        BoardName = configuration.GetValue<string>(BOARD_NAME_PATH, BoardName);
        BoardBackendUrl = configuration.GetValue<string?>(BOARD_BACKEND_URL_PATH, null);
    }
}