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
    
    RestClientOptions BackendOptions =>
        new(((Board?)ViewData[Assets.BOARD_DATANAME])!.BackendUrl
            ?? throw new NullReferenceException("Cannot access backend: Backend URL is null."));
    JsonSerializerOptions SerializationOptions = new ()
    {
        PropertyNameCaseInsensitive = true
    };
    [BindProperty] public CreatePostDTO CreatePost { get; set; } = new();

    public void OnGet()
    {
        _ApplyQueryAlerts(ALERT_QUERY);
        _ApplyQueryAlerts(ERROR_QUERY);
        if (!_SetBoard()) return;
        
        var result = _MakeRequest(new RestClient(BackendOptions), "/posting/post", Method.Post, CreatePost);
        if (result is null) return;
        ServerPostDTO[]? posts = _GetResult<ServerPostDTO[]>(result);
        if (posts is null) return;
        
        ViewData.Add(POSTS_DATANAME, posts);
        if (posts.Length == 0)
        {
            _SetPageBanner(NO_POST_FOUND_TEXT, "We think this board is empty. Maybe it's time to post something.");
        }
        else _SetPageBanner(null, null);
    }

    private T? _GetResult<T>(RestResponse result)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(result.Content ?? JsonSerializer.Serialize(default(T)), SerializationOptions) ?? default(T);
        }
        catch (JsonException _)
        {
            try
            {
                if (_ProcessErrorAsErrorDTO(result))
                {
                    Log.Error($"Backend throw error, but status code is {result.StatusCode}.");
                }
            }
            catch (JsonException ex2)
            {
                _AddError("Received invalid respond from server.");
                Console.WriteLine(ex2);
            }
        }

        return default(T);
    }
    
    private bool _ProcessErrorAsErrorDTO(RestResponse result)
    {
        ErrorDTO? error = null;
        try
        {
            error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}", SerializationOptions);
        }
        catch (ArgumentNullException _) { }
        if (error is null || error.ErrorCode == null) return false;
        _PrintError(error);
        return true;
    }

    private void _PrintError(ErrorDTO error)
    {
        _AddError($"Error: {error.ErrorText}\nError code: {error.ErrorCode}");
        Log.Warning($"Server throw error: {error.ErrorText} ({error.ErrorCode})");
    }

    // When form is used
    public IActionResult OnPost()
    {
        if (!_SetBoard()) return Redirect("/");
        var result = _MakeRequest(new RestClient(BackendOptions), "/posting/post", Method.Post, CreatePost);
        if (result is null) return Redirect("/");
        ServerPostDTO? post = _GetResult<ServerPostDTO>(result);
        if (post is null) return _RedirectWithAlert(ERROR_QUERY, "Failed to create post.");
        if (post.PostId != Guid.Empty)
        {
            return _RedirectWithAlert(ALERT_QUERY, "Post created.");
        }
        else return _RedirectWithAlert(ERROR_QUERY, "Post cannot be created: Server responded with non-existing post.");
    }

    private void _ApplyQueryAlerts(string query)
    {
        var isQueryPresent = HttpContext.Request.Query.TryGetValue(query, out var alertQuery);
        if (isQueryPresent)
        {
            foreach (var val in alertQuery)
            {
                if (val is not null) _AddAlert(val, query);
            }
        }
    }
    
    private RestResponse? _MakeRequest(RestClient client, string? uri, Method method = Method.Get, object? body = null)
    {
        RestResponse result;
        try
        {
            result = _SendRequest(client, uri, method, body);
            if (!result.IsSuccessful)
            {
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    _AddError("Server not found.");
                    _SetPageBanner(NO_POST_FOUND_TEXT, "Server not found.");
                    _HideInput();
                    Log.Error("Backend /posting/post path was not found. Something wrong with config or server.");
                    return null;
                }

                if (!_ProcessErrorAsErrorDTO(result))
                {
                    Log.Error($"Backend throw {result.StatusCode}, but not supplied it with ErrorDTO error.");
                    _PrintError(new ErrorDTO()
                    {
                        ErrorText = "Request to server failed.",
                        ErrorCode = "SERVER_HTTP_ERROR"
                    });
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            _AddError("Server not responding.");
            _SetPageBanner(NO_POST_FOUND_TEXT, "Failed to connect to server.");
            _HideInput();
            return null;
        }

        return result;
    }

    private static RestResponse _SendRequest(RestClient client, string? uri, Method method, object? body)
    {
        var req = new RestRequest(uri, method);
        if (body is not null) req.AddJsonBody(body);
        var result = client.Execute(req);
        return result;
    }

    private bool _SetBoard()
    {
        var setBoard = Assets.FailedBoard;
        try
        {
            _SetPageBanner(NO_POST_FOUND_TEXT, "Because this board doesn't exists.");
            var isQueryPresent = HttpContext.Request.Query.TryGetValue(Assets.BOARDS_QUERY_NAME, out var query);
            if (query.Count <= 0)
            {
                setBoard = Assets.Singleton.DefaultBoard;
                return true;
            }
            else if (query.Count > 1)
            {
                _AddError("Multiple addresses was set.");
                _HideInput();
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
                _AddError("Unknown board.");
            }
            _HideInput();
            return false;
        }
        finally
        {
            ViewData[Assets.BOARD_DATANAME] = setBoard;
        }
    }

    private void _HideInput()
    {
        ViewData.Add(IS_INPUT_HIDDEN_DATANAME, true);
    }

    private void _SetPageBanner(string? title, string? text)
    {
        ViewData[PAGEBANNER_TITLE_DATANAME] = title;
        ViewData[PAGEBANNER_TEXT_DATANAME] = text;
    }

    private void _AddError(string error)
    {
        _AddAlert(error, ERRORS_DATANAME);
    }
    
    private void _AddInfo(string info)
    {
        _AddAlert(info, ALERTS_DATANAME);
    }
    
    private void _AddAlert(string alert, string dataName)
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
    
    private IActionResult _RedirectWithAlert(string query,string postCreated)
    {
        return Redirect($"/?{query}="+HttpUtility.UrlEncode(postCreated));
    }
}