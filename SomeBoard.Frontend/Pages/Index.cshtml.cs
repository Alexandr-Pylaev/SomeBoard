using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestSharp;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        if (!SetBoard()) return;

        RestClient client = new RestClient(
            new RestClientOptions(((Board?)ViewData["Board"])!.BackendUrl 
                                  ?? throw new NullReferenceException("Cannot access backend: Backend URL is null.")));
        RestResponse result = null!;
        try
        {
            result = client.Get(new RestRequest("/post"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            AddError("Server not responding.");
            return;
        }

        try
        {
            var posts = JsonSerializer.Deserialize<ServerPostDTO[]>(result.Content ?? "[]") ?? [];
            ViewData.Add("Posts", posts);
        }
        catch (JsonException _)
        {
            try
            {
                var error = JsonSerializer.Deserialize<ErrorDTO>(result.Content ?? "{}");
                if (error is null) throw;
                AddError($"Error: {error.ErrorText}\nError code: {error.ErrorCode}");
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
        ViewData["Board"] = Assets.FailedBoard;
        var isQueryPresent = HttpContext.Request.Query.TryGetValue(Assets.BOARDS_QUERY_NAME, out var query);
        if (query.Count <= 0)
        {
            ViewData["Board"] = Assets.Singleton.DefaultBoard;
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
                ViewData["Board"] = board;
                return true;
            }
        }

        if (ViewData["Board"]?.Equals(Assets.FailedBoard) ?? false)
        {
            AddError("Unknown board.");
        }

        return false;
    }

    private void AddError(string error)
    {
        if (ViewData.TryGetValue("Errors", out var ls))
        {
            var errors = (List<String>?)ls;
            errors?.Add(error);
        }
        else
        {
            if (!ViewData.TryAdd("Errors", new List<string> { error }))
                throw new InvalidOperationException("Errors list cannot be added or updated.");
        }
    }
}