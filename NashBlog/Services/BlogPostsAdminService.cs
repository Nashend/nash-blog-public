using NashBlog.Data;
using NashBlog.Data.Entities;
using NashBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace NashBlog.Services;

public interface IBlogPostsAdminService
{
    Task<PagedResult<BlogPost>> GetBlogPostsAsync(int startIndex, int pageSize);
    Task<BlogPost?> GetBlogPostByIdAsync(int id);
    Task<BlogPost> SaveBlogPostAsync(BlogPost blogPost, string userId);
}
public class BlogPostsAdminService : IBlogPostsAdminService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BlogPostsAdminService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task<TResult> ExecuteOnContext<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
    {
        using var context = _contextFactory.CreateDbContext();
        return await query.Invoke(context);
    }

    public async Task<BlogPost?> GetBlogPostByIdAsync(int id)
    {
        return await ExecuteOnContext(async context =>
        {
            return await context.BlogsPost
                                .AsNoTracking()
                                .Include(b => b.Category)
                                .FirstOrDefaultAsync(b => b.Id == id);
        });
    }

    public async Task<PagedResult<BlogPost>> GetBlogPostsAsync(int startIndex, int pageSize)
    {
        return await ExecuteOnContext(async context =>
        {
            var query = context.BlogsPost
                                .AsNoTracking();

            var records = await query
                                .Include(b => b.Category)
                                .OrderByDescending(b => b.Id)
                                .Skip(startIndex)
                                .Take(pageSize)
                                .ToArrayAsync();

            var count = await query.CountAsync();
            return new PagedResult<BlogPost>(records, count);
        });
    }

    private async Task<string> GenerateSlugAsync(BlogPost blogPost)
    {
        return await ExecuteOnContext(async context =>
        {
            string originalSlug = blogPost.Title.ToSlug();
            string slug = originalSlug;
            int increment = 1;

            while(await context.BlogsPost.AsNoTracking().AnyAsync(b => b.Slug == slug))
            {
                slug = $"{originalSlug}-{increment++}";
            }
            return slug;
        });
    }

    public async Task<BlogPost> SaveBlogPostAsync(BlogPost blogPost, string userId)
    {
        return await ExecuteOnContext(async context =>
        {
            if(blogPost.Id == 0)
            {
                // new blog post
                if (await context.BlogsPost.AsNoTracking().AnyAsync(b => b.Title == blogPost.Title))
                    throw new InvalidOperationException($"Blog post with the title {blogPost.Title} already exists.");

                blogPost.Slug = blogPost.Title.ToSlug();
                blogPost.CreatedAt = DateTime.UtcNow;
                blogPost.UserId = userId;

                if(blogPost.IsPublished)
                {
                    blogPost.PublishedAt = DateTime.UtcNow;
                }

                await context.BlogsPost.AddAsync(blogPost);
            }
            else
            {
                // existing blogpost
                if (await context.BlogsPost.AsNoTracking().AnyAsync(b => b.Title == blogPost.Title && b.Id != blogPost.Id))
                    throw new InvalidOperationException($"Blog post with the title {blogPost.Title} already exists.");

                var dbBlog = await context.BlogsPost.FindAsync(blogPost.Id);

                dbBlog.Title = blogPost.Title;
                dbBlog.Introduction = blogPost.Introduction;
                dbBlog.Content = blogPost.Content;
                dbBlog.CategoryId = blogPost.CategoryId;
                dbBlog.IsPublished = blogPost.IsPublished;
                dbBlog.IsFeatured = blogPost.IsFeatured;
                dbBlog.Image = blogPost.Image;

                if(blogPost.IsPublished)
                {
                    if (!dbBlog.IsPublished)
                        dbBlog.PublishedAt = DateTime.UtcNow;
                }
                else
                {
                    dbBlog.PublishedAt = null;
                }
            }

            await context.SaveChangesAsync();
            return blogPost;
        });
    }
}

