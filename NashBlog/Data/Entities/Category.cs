using System.ComponentModel.DataAnnotations;

namespace NashBlog.Data.Entities
{
	public class Category
	{
        public short Id { get; set; }

        [Required, MaxLength(50)]
		public string Name { get; set; }

        [MaxLength(75)]
        public string Slug { get; set; }
        public bool ShowOnNavbar { get; set; }

		public Category Clone() => (MemberwiseClone() as Category)!;

        public static Category[] GetSeedCategories()
        {
            return [
                new Category { Name = "C#", Slug = "c-sharp", ShowOnNavbar = true },
				new Category { Name = "ASP.NET Core", Slug = "asp-net-core", ShowOnNavbar = true },
				new Category { Name = "Blazor", Slug = "blazor", ShowOnNavbar = true },
				new Category { Name = "SQL Server", Slug = "sql-server", ShowOnNavbar = false },
				new Category { Name = "Entity Framework Core", Slug = "ef-core", ShowOnNavbar = true },
				new Category { Name = "Angular", Slug = "angular", ShowOnNavbar = false },
				new Category { Name = "React", Slug = "react", ShowOnNavbar = false },
				new Category { Name = "Vue", Slug = "vue", ShowOnNavbar = true },
				new Category { Name = "JavaScript", Slug = "javascript", ShowOnNavbar = false },
				new Category { Name = "HTML", Slug = "html", ShowOnNavbar = false },
				new Category { Name = "CSS", Slug = "css", ShowOnNavbar = false },
				new Category { Name = "Bootstrap", Slug = "bootstrap", ShowOnNavbar = false },
				new Category { Name = "MVC", Slug = "mvc", ShowOnNavbar = true },
			];
        }
    }
}
