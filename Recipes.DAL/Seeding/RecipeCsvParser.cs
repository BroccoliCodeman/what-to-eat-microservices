using Azure;
using CsvHelper;
using CsvHelper.Configuration;
using Recipes.Data.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Recipes.Data.Parsers
{
    public class RecipeCsvParser
    {
        public List<Recipe> ParseRecipes(string csvRecipesFilePath, string csvReviewsFilePath)
        {
            var recipes = new List<Recipe>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var recipesReader = new StreamReader(csvRecipesFilePath);
            using var recipesCsvReader = new CsvReader(recipesReader, config);

            recipesCsvReader.Read();
            recipesCsvReader.ReadHeader();

            var reviewsByRecipeId = ParseReviews(csvReviewsFilePath);
            int count = 0;
            while (recipesCsvReader.Read())
            {
                try
                {
                    var RecipeId = recipesCsvReader.GetField<int>("RecipeId");
                    var Id = Guid.NewGuid();
                    var RespondsList = new List<Respond>();

                    try
                    {
                        RespondsList = reviewsByRecipeId[RecipeId].ToList().Select(r =>
                        {
                            r.RecipeId = Id;
                            return r;
                        }).ToList();
                        }
                    catch (KeyNotFoundException ex )
                    {
                        Console.WriteLine($"Error parsing respond: {ex.Message}");
                    }
                    var recipe = new Recipe
                    {

                        Id = Id,
                        Title = recipesCsvReader.GetField<string>("Name") ?? string.Empty,
                        Description = recipesCsvReader.GetField<string>("Description") ?? string.Empty,
                        Servings = ParseServings(recipesCsvReader.GetField<string>("RecipeServings")),
                        CookingTime = ParseCookingTime(recipesCsvReader.GetField<string>("TotalTime")),
                        Calories = ParseCalories(recipesCsvReader.GetField<string>("Calories")),
                        Photo = ParseFirstImage(recipesCsvReader.GetField<string>("Images")),
                        CreationDate = ParseDate(recipesCsvReader.GetField<string>("DatePublished")),
                        Ingredients = ParseIngredients(
                            recipesCsvReader.GetField<string>("RecipeIngredientParts"),
                            recipesCsvReader.GetField<string>("RecipeIngredientQuantities")
                        ),
                        CookingSteps = ParseCookingSteps(recipesCsvReader.GetField<string>("RecipeInstructions"))?.Select(r => { r.RecipeId = Id; return r; }).ToList(),
                        Responds = RespondsList,
                        AuthorId = Guid.Parse("8e445865-a24d-4543-a6c6-9443d048cdb9")
                    };

                    recipes.Add(recipe);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing recipe: {ex.Message}");
                }
                count++;
                if (count == 1000)
                {
                    break;
                }
            }

            return recipes;
        }

        private int ParseServings(string servings)
        {
            if (string.IsNullOrWhiteSpace(servings)) return 1;

            if (double.TryParse(servings, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return (int)Math.Round(result);
            }
            return 1;
        }

        private int ParseCookingTime(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString)) return 0;

            // Парсимо ISO 8601 duration формат (PT4H25M)
            var hours = 0;
            var minutes = 0;

            var hourMatch = Regex.Match(timeString, @"(\d+)H");
            if (hourMatch.Success)
            {
                hours = int.Parse(hourMatch.Groups[1].Value);
            }

            var minuteMatch = Regex.Match(timeString, @"(\d+)M");
            if (minuteMatch.Success)
            {
                minutes = int.Parse(minuteMatch.Groups[1].Value);
            }

            return hours * 60 + minutes; // Повертаємо в хвилинах
        }

        private int ParseCalories(string calories)
        {
            if (string.IsNullOrWhiteSpace(calories)) return 0;

            if (double.TryParse(calories, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return (int)Math.Round(result);
            }
            return 0;
        }

        private string ParseFirstImage(string imagesString)
        {
            if (string.IsNullOrWhiteSpace(imagesString)) return string.Empty;

            // Витягуємо URL з формату c("url1", "url2", ...)
            var urlMatch = Regex.Match(imagesString, @"https?://[^""]+");
            return urlMatch.Success ? urlMatch.Value : string.Empty;
        }

        private DateTime ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return DateTime.Now;

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateTime.Now;
        }

        private ICollection<Ingredient> ParseIngredients(string names, string quantities)
        {
            var ingredients = new List<Ingredient>();

            if (string.IsNullOrWhiteSpace(names)) return ingredients;

            var nameList = ParseCArray(names);
            var quantityList = ParseCArray(quantities);

            for (int i = 0; i < nameList.Count; i++)
            {
                var ingredient = new Ingredient
                {
                    Id = Guid.NewGuid(),
                    Name = nameList[i],
                    Quantity = i < quantityList.Count ? ParseQuantity(quantityList[i]) : 0
                };

                ingredients.Add(ingredient);
            }

            return ingredients;
        }

        private ICollection<CookingStep> ParseCookingSteps(string instructions)
        {
            var steps = new List<CookingStep>();

            if (string.IsNullOrWhiteSpace(instructions)) return steps;

            var stepList = ParseCArray(instructions);

            for (int i = 0; i < stepList.Count; i++)
            {
                var step = new CookingStep
                {
                    Id = Guid.NewGuid(),
                    Description = stepList[i],
                    Order = i + 1
                };

                steps.Add(step);
            }

            return steps;
        }

        private List<string> ParseCArray(string cArrayString)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(cArrayString)) return result;

            // Витягуємо всі значення в лапках з формату c("value1", "value2", ...)
            var matches = Regex.Matches(cArrayString, @"""([^""]*)""");

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var value = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        private float ParseQuantity(string quantity)
        {
            if (string.IsNullOrWhiteSpace(quantity)) return 0;

            // Обробляємо дроби типу "1/2", "1/4"
            if (quantity.Contains("/"))
            {
                var parts = quantity.Split('/');
                if (parts.Length == 2 &&
                    float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var numerator) &&
                    float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var denominator))
                {
                    return numerator / denominator;
                }
            }

            if (float.TryParse(quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return 0;
        }
        private Dictionary<int, List<Respond>> ParseReviews(string csvReviewsFilePath)
        {
            var reviewsByRecipeId = new Dictionary<int, List<Respond>>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(csvReviewsFilePath);
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                try
                {
                    var recipeId = csv.GetField<int>("RecipeId");
                    var rating = csv.GetField<int>("Rating");
                    var review = csv.GetField<string>("Review") ?? string.Empty;

                    if (!reviewsByRecipeId.ContainsKey(recipeId))
                    {
                        reviewsByRecipeId[recipeId] = new List<Respond>();
                    }

                    reviewsByRecipeId[recipeId].Add(new Respond
                    {
                        Id = Guid.NewGuid(),
                        Text = review,
                        Rate = rating,
                        UserId = Guid.Parse("9c445865-a24d-4233-a6c6-9443d048cdb9")

                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing review: {ex.Message}");
                }
            }

            return reviewsByRecipeId;
        }
    }
}