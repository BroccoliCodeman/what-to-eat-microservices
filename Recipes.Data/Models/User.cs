using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.Data.Models
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Avatar { get; set; } = null!;

        // Рецепти, які користувач зберіг (закладки)
        public ICollection<Recipe> SavedRecipes { get; set; } = new List<Recipe>();

        // Рецепти, які користувач створив (автор)
        public ICollection<Recipe> CreatedRecipes { get; set; } = new List<Recipe>();

        public ICollection<Respond> Responds { get; set; } = new List<Respond>();
    }
}
