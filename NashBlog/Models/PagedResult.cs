using NashBlog.Data.Entities;

namespace NashBlog.Models;

public record PagedResult<TResult>(TResult[] Records, int TotalCount);

public record DetailPageModel(BlogPost BlogPost, BlogPost[] RelatedPosts)
{
    public static DetailPageModel Empty() => new(default, []);
    public bool IsEmpty => BlogPost is null;
}
