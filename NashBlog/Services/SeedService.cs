using NashBlog.Data;
using NashBlog.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace NashBlog.Services
{
	public interface ISeedService
	{
		Task SeedDataAsync();
	}

	public class SeedService : ISeedService
	{
		private readonly ApplicationDbContext _context;
		private readonly IUserStore<ApplicationUser> _userStore;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
		//private readonly IBlogPostsAdminService _adminBlogPostService;

        public SeedService(ApplicationDbContext context, IUserStore<ApplicationUser> userStore,
			UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
		{
			_context = context;
			_userStore = userStore;
			_userManager = userManager;
			_roleManager = roleManager;
            _configuration = configuration;
			//_adminBlogPostService = adminBlogPostService;
        }

		private async Task MigrateDatabaseAsync()
		{
#if DEBUG
			if((await _context.Database.GetPendingMigrationsAsync()).Any())
			{
				await _context.Database.MigrateAsync();
			}
#endif
		}

		public async Task SeedDataAsync()
		{
			// automate the creating migrations process in development mode
			await MigrateDatabaseAsync();

			// Seed AdminRole
			if (await _roleManager.FindByNameAsync(_configuration["AdminAccount:Role"]!) is null)
			{
				//Admin role doesnt exist
				var adminRole = new IdentityRole(_configuration["AdminAccount:Role"]!);
				var result = await _roleManager.CreateAsync(adminRole);
				if (!result.Succeeded)
				{
					string errorsString = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
					throw new Exception($"Error in creating admin role: {Environment.NewLine}{errorsString}");
				}
			}

			// Seed AdminUser
			if (await _userManager.FindByEmailAsync(_configuration["AdminAccount:Email"]!) is null)
			{
				//Admin user doesnt exist
				var adminUser = new ApplicationUser();
				adminUser.Name = _configuration["AdminAccount:Name"]!;

                await _userStore.SetUserNameAsync(adminUser, _configuration["AdminAccount:Email"]!, CancellationToken.None);

				var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
				await emailStore.SetEmailAsync(adminUser, _configuration["AdminAccount:Email"]!, CancellationToken.None);

				var result = await _userManager.CreateAsync(adminUser, _configuration["AdminAccount:Password"]!);
				if (!result.Succeeded)
				{
					string errorsString = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
					throw new Exception($"Error in creating admin user: {Environment.NewLine}{errorsString}");
				}
			}

			// Seed Categories
			if (!await _context.Categories.AsNoTracking().AnyAsync())
			{
				// No categories in database
				// Add default ones
				await _context.Categories.AddRangeAsync(Category.GetSeedCategories());
				await _context.SaveChangesAsync();
			}



			// Add first default blog post
			/*BlogPost post = new();
			post.Title = "Test";
			post.Introduction = "Test";
			post.Content = "Test";
			post.CategoryId = 1;
			post.IsPublished = true;
			post.IsFeatured = true;
			post.Image = "";
			await _adminBlogPostService.SaveBlogPostAsync(post, "253069b4-60c4-46b9-bb2b-fcd86813333b");*/
		}
	}
}
