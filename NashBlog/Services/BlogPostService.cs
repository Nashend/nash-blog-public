using NashBlog.Data;
using NashBlog.Data.Entities;
using NashBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace NashBlog.Services;

public interface IBlogPostService
{
    Task<BlogPost[]> GetFeaturedBlogPostsAsync(int count, int categoryId = 0);
    Task<BlogPost[]> GetPopularBlogPostsAsync(int count, int categoryId = 0);
    Task<BlogPost[]> GetRecentBlogPostsAsync(int count, int categoryId = 0);
    Task<DetailPageModel> GetBlogPostBySlugAsync(string slug);
    Task<BlogPost[]> GetBlogPostsAsync(int pageIndex, int pageSize, int categoryId = 0);
}

public class BlogPostService : IBlogPostService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BlogPostService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task<TResult> ExecuteOnContextAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
    {
        using var context = _contextFactory.CreateDbContext();
        return await query.Invoke(context);
    }

    public async Task<BlogPost[]> GetFeaturedBlogPostsAsync(int count, int categoryId = 0)
    {
        return await ExecuteOnContextAsync(async context =>
        {
            var query = context.BlogsPost
                                .AsNoTracking()
                                .Include(p => p.Category)
                                .Include(b => b.User)
                                .Where(b => b.IsPublished && b.IsFeatured);

            if(categoryId > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId);
            }

            var records = await query.Where(b => b.IsFeatured)
                                    .OrderBy(_ => Guid.NewGuid())
                                    .Take(count)
                                    .ToArrayAsync();

            if(count > records.Length)
            {
                // not enough posts in database, lets get more additional records randomly
                var additionalRecords = await query.Where(b => !b.IsFeatured)
									                .OrderBy(_ => Guid.NewGuid())
									                .Take(count - records.Length)
									                .ToArrayAsync();

                records = [.. records, .. additionalRecords];
			}
            return records;
        });
    }

    public async Task<BlogPost[]> GetPopularBlogPostsAsync(int count, int categoryId = 0)
    {
        return await ExecuteOnContextAsync(async context =>
        {
            var query = context.BlogsPost
                                .AsNoTracking()
                                .Include(p => p.Category)
                                .Include(b => b.User)
                                .Where(b => b.IsPublished);

            if (categoryId > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId);
            }

            return await query.OrderByDescending(b => b.ViewCount)
                                .Take(count)
                                .ToArrayAsync();
        });
    }

    public async Task<BlogPost[]> GetRecentBlogPostsAsync(int count, int categoryId = 0)
    {
        return await GetPostsAsync(0, count, categoryId);
    }

    public async Task<DetailPageModel> GetBlogPostBySlugAsync(string slug)
    {
        return await ExecuteOnContextAsync(async context =>
        {
            var blogPost = await context.BlogsPost
                                        .AsNoTracking()
                                        .Include(b => b.Category)
                                        .Include(b => b.User)
                                        .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

            if (blogPost is null)
                return DetailPageModel.Empty();

            var relatedPosts = await context.BlogsPost
                                    .AsNoTracking()
                                    .Include(b => b.Category)
                                    .Include(b => b.User)
                                    .Where(b => b.CategoryId == blogPost.CategoryId && b.IsPublished)
                                    .OrderBy(_ => Guid.NewGuid())
                                    .Take(4)
                                    .ToArrayAsync();

            return new DetailPageModel(blogPost, relatedPosts);
        });
    }

	public async Task<BlogPost[]> GetBlogPostsAsync(int pageIndex, int pageSize, int categoryId = 0)
	{
        return await GetPostsAsync(pageIndex * pageSize, pageSize, categoryId);
	}

    private async Task<BlogPost[]> GetPostsAsync(int skip, int take, int categoryId)
    {
		return await ExecuteOnContextAsync(async context =>
		{
			var query = context.BlogsPost
								.AsNoTracking()
								.Include(p => p.Category)
								.Include(b => b.User)
								.Where(b => b.IsPublished);

			if (categoryId > 0)
			{
				query = query.Where(b => b.CategoryId == categoryId);
			}

			return await query.OrderByDescending(b => b.PublishedAt)
								.Skip(skip)
								.Take(take)
								.ToArrayAsync();
		});
	}
}
