using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.Data.DataTransferObjects.UserDTOs
{
    public class GetUserForRecipeDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Avatar { get; set; } = null!;

       // public List<RecipeIntroDto> SavedRecipes { get; set; }
    }
}
