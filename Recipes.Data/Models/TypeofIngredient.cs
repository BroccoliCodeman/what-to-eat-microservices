using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.Data.Models
{
    public class TypeofIngredient
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }  
    }
}
