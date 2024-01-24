using NashBlog.Data;
using NashBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace NashBlog.Services
{
    public interface ICategoryService
    {
        Task<Category[]> GetCategoriesAsync();
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<Category> SaveCategoryAsync(Category category);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        private async Task<TResult> ExecuteOnContext<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
        {
            using var context = _contextFactory.CreateDbContext();
            return await query.Invoke(context);
        }

        public async Task<Category[]> GetCategoriesAsync()
        {
            return await ExecuteOnContext(async context =>
            {
                var categories = await context.Categories
                                .AsNoTracking()
                                .ToArrayAsync();
                return categories;
            });
        }
        public async Task<Category> SaveCategoryAsync(Category category)
        {
            return await ExecuteOnContext(async context =>
            {
                if (category.Id == 0)
                {
                    // New category
                    if (await context.Categories.AsNoTracking().AnyAsync(c => c.Name == category.Name))
                        throw new InvalidOperationException($"Category with the name {category.Name} already exists.");

                    category.Slug = category.Name.ToSlug();
                    await context.Categories.AddAsync(category);
                }
                else
                {
                    // Existing category
                    if (await context.Categories.AsNoTracking().AnyAsync(c => c.Name == category.Name && c.Id != category.Id))
                        throw new InvalidOperationException($"Category with the name {category.Name} already exists.");

                    var dbCategory = await context.Categories.FindAsync(category.Id);

                    // never modifying the slug or links from the bot indexers will be broken
                    category.Slug = dbCategory.Slug;
                    dbCategory.Name = category.Name;
                    dbCategory.ShowOnNavbar = category.ShowOnNavbar;
                }

                await context.SaveChangesAsync();
                return category;
            });
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await ExecuteOnContext(async context =>
            {
                return await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);
            });
        }
    }
}
