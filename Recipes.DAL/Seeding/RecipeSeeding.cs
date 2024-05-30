using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.DAL.Seeding
{
    public static class RecipeSeeding
    {

        public static async Task SeedRecipes(RecipesContext context)
        {
            if (context.Recipes.Count() == 0)
            {
                string json = File.ReadAllText(@"../recipes/dishes.txt");
                var Recipes = JsonConvert.DeserializeObject<List<RecipeDtoWithIngredientsAndSteps>>(json);

                await context.AddRangeAsync(Recipes);
                await context.SaveChangesAsync();
            }
        }
    }
}
