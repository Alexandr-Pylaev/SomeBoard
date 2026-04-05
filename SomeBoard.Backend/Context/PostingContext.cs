using Microsoft.EntityFrameworkCore;
using SomeBoard.Backend.Models;
using SomeBoard.Shared.Posting;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }

    public PostingContext() { }

    public async Task<PostModel> PostAsync(string author, string message, CancellationToken token)
    {
        PostModel model = new(new Post(author, message, DateTime.Now));
        await Posts.AddAsync(model, token);
        await SaveChangesAsync(token);
        return model;
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        (await Posts.FindAsync([id], token))?.DeletePost();
        await SaveChangesAsync(token);
    }
    
    public PostModel Post(string author, string message) => PostAsync(author, message, CancellationToken.None).Result;
    public void Delete(Guid id) => DeleteAsync(id, CancellationToken.None).Wait();
}