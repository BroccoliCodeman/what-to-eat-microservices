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
        public string LastName { get; set; }=null!;
        public string Avatar {  get; set; } = null!;
        public ICollection<Recipe>? SavedRecipes { get; } = new List<Recipe>();
        public ICollection<Recipe>? CreatedRecipes { get; } = new List<Recipe>();

    }
}
