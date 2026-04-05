using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SomeBoard.Backend.Models;

namespace SomeBoard.Backend.Context;

public class PostingContext : DbContext
{
    public DbSet<PostModel> Posts { get; set; }

    public PostingContext(DbContextOptions options) : base(options)
    {
        Database.Migrate();
    }

    public async Task<PostModel> PublishAsync(string author, string message, DateTime dateTime, CancellationToken token)
    {
        PostModel model = new(author, message, dateTime);
        await Posts.AddAsync(model, token);
        await SaveChangesAsync(token);
        return model;
    }

    public IQueryable<PostModel> Fetch(int position, int count)
    {
        return Posts.Where(x => x.Deleted == false).OrderBy(x => x.PostId).Skip(position).Take(count);
    } 

    public async Task<PostModel?> DeleteAsync(Guid id, CancellationToken token)
    {
        var post = await Posts.FindAsync([id], token);
        if (post is null) return post;
        post?.DeletePost();
        await SaveChangesAsync(token);
        return post;
    }
    public PostModel Publish(string author, string message, DateTime dateTime) => PublishAsync(author, message, dateTime, CancellationToken.None).Result;
    public PostModel? Delete(Guid id) => DeleteAsync(id, CancellationToken.None).Result;
}