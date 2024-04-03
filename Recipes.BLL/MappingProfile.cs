using AutoMapper;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;

namespace Recipes.BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CookingStep, CookingStepDto>().ReverseMap();
        
        CreateMap<Ingredient, IngredientDto>().ReverseMap();
        
        CreateMap<Recipe, RecipeDto>().ReverseMap();
       

        CreateMap<RecipeDtoWithIngredients, Recipe>()
    .ForMember(dest => dest.Ingredients, opt => opt.MapFrom(src => src.Ingredients)).ReverseMap();
        // CreateMap<RecipeIngredient, RecipeIngredientDto>().ReverseMap();

        CreateMap<Respond, RespondDto>().ReverseMap();
        
        CreateMap<SavedRecipe, SavedRecipeDto>().ReverseMap();
        
        CreateMap<WeightUnit, WeightUnitDto>().ReverseMap();
    }
}