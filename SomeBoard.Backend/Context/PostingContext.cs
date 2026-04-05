using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SomeBoard.Backend.Models;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }

    public PostingContext(DbContextOptions options) : base(options) { }

    public async Task<PostModel> PublishAsync(string author, string message, DateTime dateTime, CancellationToken token)
    {
        PostModel model = new(author, message, dateTime);
        await Posts.AddAsync(model, token);
        await SaveChangesAsync(token);
        return model;
    }

    public IQueryable<PostModel> Fetch(int position, int count)
    {
        return Posts.OrderBy(x => x.PostId).Take(new Range(position, position + count));
    } 

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        (await Posts.FindAsync([id], token))?.DeletePost();
        await SaveChangesAsync(token);
    }
    public PostModel Publish(string author, string message, DateTime dateTime) => PublishAsync(author, message, dateTime, CancellationToken.None).Result;
    public void Delete(Guid id) => DeleteAsync(id, CancellationToken.None).Wait();
}