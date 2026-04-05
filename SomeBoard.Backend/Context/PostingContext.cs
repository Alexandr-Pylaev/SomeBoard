using Microsoft.EntityFrameworkCore;
using SomeBoard.Backend.Models;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }

    public PostingContext() { }
}