using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Serilog;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Frontend;

public class Assets
{
    public const string CONFIG_PATH = "SomeBoard";
    public const string BOARDS_PATH = $"{CONFIG_PATH}:Boards";
    public const string DEFAULT_BOARD_ADDR = $"{CONFIG_PATH}:DefaultBoard";
    public const string BOARDS_QUERY_NAME = "addr";
    public const string BOARD_DATANAME = "Board";
    public static Assets Singleton => _singleton.Value;
    private static readonly Lazy<Assets> _singleton = new();

    public Board[] Boards { get; private set; }= null!;
    public Board DefaultBoard = null!;
    public static Board FailedBoard => new Board() { Name = "Unknown", Description = "Very unknown board" };
    public void Initialize(IConfiguration configuration)
    {
        Boards = configuration.GetSection(BOARDS_PATH).Get<Board[]>() ?? [];
        var defaultBoardAddr = configuration.GetValue<string?>(DEFAULT_BOARD_ADDR);
        DefaultBoard = Boards.FirstOrDefault(x => x.Query == defaultBoardAddr, Boards.First());
        foreach (var board in Boards)
        {
            if (board.Name is not null && board.Description is not null) continue;
            UpdateBoard(board);
        }
    }

    public static void UpdateBoard(Board board)
    {
        try
        {
            var client = new RestClient(board.BackendUrl
                                        ?? throw new NullReferenceException(
                                            "Cannot access backend: Backend URL is null."));
            var req = new RestRequest("/posting/board", Method.Get);
            var response = client.Execute(req);
            if (!response.IsSuccessful)
            {
                Log.Error($"Failed to update board: {response.StatusCode}");
            }
            var boardInfo = JsonSerializer.Deserialize<BoardInfoDTO>(response.Content ?? "{}", new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            if (boardInfo is null || boardInfo?.Name is null) return;
            board.Name = string.IsNullOrEmpty(board.Name) ? boardInfo.Name : board.Name;
            board.Description = string.IsNullOrEmpty(board.Description) ? boardInfo.Description : board.Description;
            board.PostCount = boardInfo.PostCount;
        }
        catch (JsonException ex)
        {
            Log.Error(ex.ToString());
        }
    }
}