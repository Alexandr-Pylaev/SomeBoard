using Microsoft.AspNetCore.Mvc;
using Serilog;
using SomeBoard.Backend.Context;
using SomeBoard.Backend.Models;
using SomeBoard.Shared;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PostingController : ControllerBase
{
    [HttpGet]
    [ActionName(Paths.Post)]
    public ServerPostDTO[] Fetch(int position, PostingContext context)
    {
        Log.Information($"{HttpContext.TraceIdentifier}: Fetched 25 posts from position {position}.");
        return context.Fetch(position, 25).Select(x => IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(x)!
            .ToDTO()).ToArray();
    }
    
    [HttpPost]
    [ActionName(Paths.Post)]
    public ServerPostDTO Publish(CreatePostDTO input, PostingContext context)
    {
        Log.Information($"{HttpContext.TraceIdentifier}: Requested new post with author {input.Author}.");
        var post = context.Publish(input.Author, input.Message, DateTime.Now);
        Log.Information($"{HttpContext.TraceIdentifier}: Created new post {post.PostId}.");
        return IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(post)! 
            .ToDTO();
    }
    
    [HttpDelete]
    [ActionName(Paths.Post)]
    public JsonResult Delete(DeletePostDTO input, PostingContext context, IConfiguration configuration)
    {
        Log.Information($"{HttpContext.TraceIdentifier}: Requested delete post {input.PostId}.");
        return new JsonResult(_RealDelete(input, context, configuration));
    }

    private object? _RealDelete(DeletePostDTO input, PostingContext context, IConfiguration configuration)
    {
        if (Program.VerifyAdminSecret(input.Secret, configuration))
            return IDTODeserializable<ServerPostDTO>
                .Convert<PostModel?>
                    (context.Delete(input.PostId))?
                .ToDTO();
        if (string.IsNullOrEmpty(input.Secret)) Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Log.Information($"{HttpContext.TraceIdentifier}: Failed to verify admin secret ({Response.StatusCode}).");
        return new ErrorDTO()
            {
                ErrorText = "Failed to verify admin secret.",
                ErrorCode = ErrorCodes.BAD_ADMIN_SECRET
            };
    }
}