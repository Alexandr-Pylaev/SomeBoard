using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet(Name = "Test")]
    public ActionResult Test()
    {
        return Ok("Hello, World!");
    }
}