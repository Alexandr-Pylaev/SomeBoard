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
    [ActionName("Post")]
    public ServerPostDTO[] Fetch(int position, PostingContext context)
    {
        return context.Fetch(position, 25).Select(x => IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(x) 
            .ToDTO()).ToArray();
    }
    
    [HttpPost]
    [ActionName("Post")]
    public ServerPostDTO Publish(CreatePostDTO input, PostingContext context)
    {
        return IDTODeserializable<ServerPostDTO>
            .Convert<PostModel>(context.Publish(input.Author, input.Message, DateTime.Now)) 
            .ToDTO();
    }
}