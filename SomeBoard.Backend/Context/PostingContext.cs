using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using SomeBoard.Backend.Models;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }
    
    public long PostCount { get; private set; }

    public PostingContext(DbContextOptions options) : base(options)
    {
        Database.Migrate();
        PostCount = Posts.Count(x => !x.Deleted);
    }

    public async Task<PostModel> PublishAsync(string author, string message, DateTime dateTime, CancellationToken token)
    {
        PostModel model = new(author, message, dateTime);
        model = (await Posts.AddAsync(model, token)).Entity;
        PostCount++;
        await SaveChangesAsync(token);
        Log.Verbose($"Published new post {model.PostId} by {author}");
        return model;
    }

    public IQueryable<PostModel> Fetch(int position, int count)
    {
        return Posts.Where(x => x.Deleted == false).OrderBy(x => x.PublishTime).Reverse().Skip(position).Take(count);
    } 

    public async Task<PostModel?> DeleteAsync(Guid id, CancellationToken token)
    {
        var post = await Posts.FindAsync([id], token);
        if (post is null) return post;
        post?.DeletePost();
        await SaveChangesAsync(token);
        PostCount--;
        Log.Verbose($"Deleted post {id}.");
        return post;
    }
    public PostModel Publish(string author, string message, DateTime dateTime) => PublishAsync(author, message, dateTime, CancellationToken.None).Result;
    public PostModel? Delete(Guid id) => DeleteAsync(id, CancellationToken.None).Result;
}