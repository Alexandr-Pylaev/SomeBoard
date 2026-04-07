using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestSharp;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Frontend.Pages;

public class IndexModel : PageModel
{
    public const string ERRORS_DATANAME = "Errors";
    public const string PAGEBANNER_TITLE_DATANAME = "PageBannerTitle";
    public const string PAGEBANNER_TEXT_DATANAME = "PageBannerText";
    public const string POSTS_DATANAME = "Posts";
    public const string NO_POST_FOUND_TEXT = "This board is empty.";
    
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        if (!SetBoard()) return;

        RestClient client = new RestClient(
            new RestClientOptions(((Board?)ViewData[Assets.BOARD_DATANAME])!.BackendUrl
                                  ?? throw new NullReferenceException("Cannot access backend: Backend URL is null.")));
        RestResponse result;
        try
        {
            result = client.Get(new RestRequest("/" + Paths.Post.ToLower()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            AddError("Server not responding.");
            SetPageBanner(NO_POST_FOUND_TEXT, "Failed to connect to server.");
            return;
        }

        try
        {
            var posts = JsonSerializer.Deserialize<ServerPostDTO[]>(result.Content ?? "[]") ?? [];
            ViewData.Add(POSTS_DATANAME, posts);
            if (posts.Length == 0)
            {
                SetPageBanner(NO_POST_FOUND_TEXT, "We think this board is empty. Maybe it's time to post something.");
            }
            else SetPageBanner(null, null);
        }
        catch (JsonException _)
        {
            try
            {
                var error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}");
                if (error is null) throw;
                AddError($"Error: {error.ErrorText}\nError code: {error.ErrorCode}");
                SetPageBanner(NO_POST_FOUND_TEXT, "Server returned errors.");
            }
            catch (JsonException ex2)
            {
                AddError("Received invalid respond from server.");
                Console.WriteLine(ex2);
            }
        }
    }

    private bool SetBoard()
    {
        var setBoard = Assets.FailedBoard;
        try
        {
            SetPageBanner(NO_POST_FOUND_TEXT, "Because this board doesn't exists.");
            var isQueryPresent = HttpContext.Request.Query.TryGetValue(Assets.BOARDS_QUERY_NAME, out var query);
            if (query.Count <= 0)
            {
                setBoard = Assets.Singleton.DefaultBoard;
                return true;
            }
            else if (query.Count > 1)
            {
                AddError("Multiple addresses was set.");
                return false;
            }

            foreach (var board in Assets.Singleton.Boards)
            {
                if (isQueryPresent && query.First() == board.Query)
                {
                    setBoard = board;
                    return true;
                }
            }

            if (setBoard?.Equals(Assets.FailedBoard) ?? false)
            {
                AddError("Unknown board.");
            }

            return false;
        }
        finally
        {
            ViewData[Assets.BOARD_DATANAME] = setBoard;
        }
    }

    private void SetPageBanner(string? title, string? text)
    {
        ViewData[PAGEBANNER_TITLE_DATANAME] = title;
        ViewData[PAGEBANNER_TEXT_DATANAME] = text;
    }

    private void AddError(string error)
    {
        if (ViewData.TryGetValue(ERRORS_DATANAME, out var ls))
        {
            var errors = (List<String>?)ls;
            errors?.Add(error);
        }
        else
        {
            if (!ViewData.TryAdd(ERRORS_DATANAME, new List<string> { error }))
                throw new InvalidOperationException("Errors list cannot be added or updated.");
        }
    }
}