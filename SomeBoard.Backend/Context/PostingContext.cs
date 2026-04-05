using Microsoft.EntityFrameworkCore;
using SomeBoard.Backend.Models;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }

    public PostingContext() { }

    public PostModel Post(string author, string message)
    {
        PostModel model = new(new Post(author, message, DateTime.Now));
        Posts.Add(model);
        return model;
    }

    public void Delete(Guid id)
    {
        Posts.Find(id)?.DeletePost();
    }
}