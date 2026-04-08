using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using RestSharp;
using Serilog;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Frontend.Pages;

public class IndexModel : PageModel
{
    public const string ERRORS_DATANAME = "Errors";
    public const string ALERTS_DATANAME = "Alerts";
    public const string PAGEBANNER_TITLE_DATANAME = "PageBannerTitle";
    public const string PAGEBANNER_TEXT_DATANAME = "PageBannerText";
    public const string POSTS_DATANAME = "Posts";
    public const string IS_INPUT_HIDDEN_DATANAME = "IsInputHidden";
    public const string NO_POST_FOUND_TEXT = "This board is empty.";
    public const string ERROR_QUERY = "error";
    public const string ALERT_QUERY = "info";
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var isQueryPresent = HttpContext.Request.Query.TryGetValue(ALERT_QUERY, out var alertQuery);
        if (isQueryPresent)
        {
            foreach (var val in alertQuery)
            {
                if (val is not null) AddInfo(val);
            }
        }
        
        isQueryPresent = HttpContext.Request.Query.TryGetValue(ERROR_QUERY, out var errorQuery);
        if (isQueryPresent)
        {
            foreach (var val in errorQuery)
            {
                if (val is not null) AddError(val);
            }
        }
        if (!SetBoard()) return;
        var client = new RestClient(
            new RestClientOptions(((Board?)ViewData[Assets.BOARD_DATANAME])!.BackendUrl
                                  ?? throw new NullReferenceException("Cannot access backend: Backend URL is null.")));
        RestResponse result;
        if (MakeRequest(client, out result, "/posting/post")) return;
        try
        {
            var posts = JsonSerializer.Deserialize<ServerPostDTO[]>(result.Content ?? "[]", jsonSerializerOptions) ?? [];
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
                var error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}",jsonSerializerOptions);
                if (error == new ErrorDTO()) throw;
                AddError($"Error: {error.ErrorText}\nError code: {error.ErrorCode}");
                SetPageBanner(NO_POST_FOUND_TEXT, "Server returned errors.");
                Log.Error($"Backend throw error, but status code is {result.StatusCode}.");
                Log.Warning($"Server throw error: {error.ErrorText} ({error.ErrorCode})");
            }
            catch (JsonException ex2)
            {
                AddError("Received invalid respond from server.");
                Console.WriteLine(ex2);
            }
        }
    }
    
    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    private bool MakeRequest(RestClient client, out RestResponse result, string? uri, Method method = Method.Get, object? body = null)
    {
        try
        {
            var req = new RestRequest(uri, method);
            if (body is not null) req.AddJsonBody(body);
            result = client.Execute(req);
            if (!result.IsSuccessful)
            {
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    AddError("Server not found.");
                    SetPageBanner(NO_POST_FOUND_TEXT, "Server not found.");
                    HideInput();
                    Log.Error("Backend /posting/post path was not found. Something wrong with config or server.");
                    return true;
                }
                var error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}",jsonSerializerOptions);
                if (error == new ErrorDTO())
                {
                    Log.Error($"Backend throw {result.StatusCode}, but not supplied it with error.");
                    return true;
                }
                AddError($"Error: {error.ErrorText}\nError code: {error.ErrorCode}");
                SetPageBanner(NO_POST_FOUND_TEXT, "Server returned errors.");
                Log.Warning($"Server throw error: {error.ErrorText} ({error.ErrorCode})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            AddError("Server not responding.");
            SetPageBanner(NO_POST_FOUND_TEXT, "Failed to connect to server.");
            HideInput();
            result = null;
            return true;
        }

        return false;
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
                HideInput();
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
            HideInput();
            return false;
        }
        finally
        {
            ViewData[Assets.BOARD_DATANAME] = setBoard;
        }
    }

    private void HideInput()
    {
        ViewData.Add(IS_INPUT_HIDDEN_DATANAME, true);
    }

    private void SetPageBanner(string? title, string? text)
    {
        ViewData[PAGEBANNER_TITLE_DATANAME] = title;
        ViewData[PAGEBANNER_TEXT_DATANAME] = text;
    }

    private void AddError(string error)
    {
        AddAlert(error, ERRORS_DATANAME);
    }
    
    private void AddInfo(string info)
    {
        AddAlert(info, ALERTS_DATANAME);
    }
    
    private void AddAlert(string alert, string dataName)
    {
        if (ViewData.TryGetValue(dataName, out var ls))
        {
            var list = (List<String>?)ls;
            list?.Add(alert);
        }
        else
        {
            if (!ViewData.TryAdd(dataName, new List<string> { alert }))
                throw new InvalidOperationException($"Alert list {dataName} cannot be added or updated.");
        }
    }

    [BindProperty] public CreatePostDTO CreatePost { get; set; } = new();
    public IActionResult OnPost()
    {
        if (!SetBoard()) return Redirect("/");
        var client = new RestClient(
            new RestClientOptions(((Board?)ViewData[Assets.BOARD_DATANAME])!.BackendUrl
                                  ?? throw new NullReferenceException("Cannot access backend: Backend URL is null.")));
        RestResponse result;
        if (MakeRequest(client, out result, "/posting/post", Method.Post, CreatePost)) return Redirect("/");
        try
        {
            var post = JsonSerializer.Deserialize<ServerPostDTO>(result.Content ?? "{}",jsonSerializerOptions);
            if (post.PostId != Guid.Empty)
            {
                return RedirectWithAlert(ALERT_QUERY, "Post created.");
            }
            else return RedirectWithAlert(ERROR_QUERY, "Post cannot be created: Server responded with non-existing post.");
        }
        catch (JsonException _)
        {
            try
            {
                var error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}",jsonSerializerOptions);
                if (error == new ErrorDTO()) throw;
                Log.Error($"Backend throw error, but status code is {result.StatusCode}.");
                Log.Warning($"Server throw error: {error.ErrorText} ({error.ErrorCode})");
                return RedirectWithAlert(ERROR_QUERY, $"Post cannot be created: Error: {error.ErrorText}\nError code: {error.ErrorCode}");
            }
            catch (JsonException ex2)
            {
                Console.WriteLine(ex2);
                return RedirectWithAlert(ERROR_QUERY, "Post cannot be created: Received invalid respond from server.");
            }
        }

        return Redirect("/");
    }

    private IActionResult RedirectWithAlert(string query,string postCreated)
    {
        return Redirect($"/?{query}="+HttpUtility.UrlEncode(postCreated));
    }
}