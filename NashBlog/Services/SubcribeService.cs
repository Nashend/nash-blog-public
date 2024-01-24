using NashBlog.Data;
using NashBlog.Data.Entities;
using NashBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace NashBlog.Services;

public interface ISubcribeService
{
	Task<string?> SuscribeAsync(Subscriber subscriber);
	Task<PagedResult<Subscriber>> GetSubscribersAsync(int startIndex, int pageSize);
}

public class SubcribeService : ISubcribeService
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

	public SubcribeService(IDbContextFactory<ApplicationDbContext> contextFactory)
	{
		_contextFactory = contextFactory;
	}

	public async Task<string?> SuscribeAsync(Subscriber subscriber)
	{
		using var context = _contextFactory.CreateDbContext();
		var alreadySubscribed = await context.Subscribers
												.AsNoTracking()
												.AnyAsync(s => s.Email == subscriber.Email);

		if (alreadySubscribed)
		{
			return "You are already subscribed.";
		}
		subscriber.SubscribedOn = DateTime.UtcNow;
		await context.Subscribers.AddAsync(subscriber);
		await context.SaveChangesAsync();
		return null;
	}

	public async Task<PagedResult<Subscriber>> GetSubscribersAsync(int startIndex, int pageSize)
	{
        using var context = _contextFactory.CreateDbContext();
		var query = context.Subscribers
							.AsNoTracking()
							.OrderByDescending(s => s.SubscribedOn);

		var totalCount = await query.CountAsync();

		var records = await query.Skip(startIndex)
								.Take(pageSize)
								.ToArrayAsync();

		return new PagedResult<Subscriber>(records, totalCount);
    }
}
