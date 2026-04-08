using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend;

public class Assets
{
    public static Assets Singleton => _singleton.Value;
    private static readonly Lazy<Assets> _singleton = new();
    
    public const string CONFIG_PATH = "SomeBoard";
    public const string POSTING_CONNECTIONSTRING_PATH = $"{CONFIG_PATH}:Posting:ConnectionString";
    public const string POSTING_DATABASE_PATH = $"{CONFIG_PATH}:Posting:Database";
    public const string POSTING_USERNAME_PATH = $"{CONFIG_PATH}:Posting:Username";
    public const string POSTING_PASSWORD_PATH = $"{CONFIG_PATH}:Posting:Password";
    public const string POSTING_PASSFILE_PATH = $"{CONFIG_PATH}:Posting:PasswordFile";
    public const string POSTING_HOST_PATH = $"{CONFIG_PATH}:Posting:Host";
    public const string POSTING_PORT_PATH = $"{CONFIG_PATH}:Posting:Port";
    public const string POSTING_ADMIN_SECRETFILE_PATH = $"{CONFIG_PATH}:Posting:AdminSecretFile";
    public const string POSTING_BOARD_INFO = $"{CONFIG_PATH}:Posting:Board";

    public BoardInfo BoardInfo;
    public void Initialize(IConfiguration configuration)
    {
        BoardInfo = configuration.GetValue<BoardInfo>(POSTING_BOARD_INFO) ?? new BoardInfo();
    }
}