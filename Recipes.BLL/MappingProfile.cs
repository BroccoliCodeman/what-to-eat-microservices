using AutoMapper;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;

namespace Recipes.BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CookingStep, CookingStepDto>();
        CreateMap<CookingStepDto, CookingStep>();
        
        CreateMap<Ingredient, IngredientDto>();
        CreateMap<IngredientDto, Ingredient>();
        
        CreateMap<Recipe, RecipeDto>();
        CreateMap<RecipeDto, Recipe>();
        
        CreateMap<RecipeIngredient, RecipeIngredientDto>();
        CreateMap<RecipeIngredientDto, RecipeIngredient>();
        
        CreateMap<Respond, RespondDto>();
        CreateMap<RespondDto, Respond>();
        
        CreateMap<SavedRecipe, SavedRecipeDto>();
        CreateMap<SavedRecipeDto, SavedRecipe>();
        
        CreateMap<WeightUnit, WeightUnitDto>();
        CreateMap<WeightUnitDto, WeightUnit>();
    }
}