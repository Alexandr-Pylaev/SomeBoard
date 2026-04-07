using Microsoft.AspNetCore.Mvc;
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
        return context.Fetch(position, 25).Select(x => IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(x)!
            .ToDTO()).ToArray();
    }
    
    [HttpPost]
    [ActionName(Paths.Post)]
    public ServerPostDTO Publish(CreatePostDTO input, PostingContext context)
    {
        return IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(context.Publish(input.Author, input.Message, DateTime.Now))! 
            .ToDTO();
    }
    
    [HttpDelete]
    [ActionName(Paths.Post)]
    public JsonResult Delete(DeletePostDTO input, PostingContext context, IConfiguration configuration)
    {
        return new JsonResult(_RealDelete(input, context, configuration));
    }

    private object? _RealDelete(DeletePostDTO input, PostingContext context, IConfiguration configuration)
    {
        if (Program.VerifyAdminSecret(input.Secret, configuration))
            return IDTODeserializable<ServerPostDTO>
                .Convert<PostModel?>
                    (context.Delete(input.PostId))?
                .ToDTO();
        else
            return new ErrorDTO()
            {
                ErrorText = "Failed to verify admin secret.",
                ErrorCode = ErrorCodes.BAD_ADMIN_SECRET
            };
    }
}