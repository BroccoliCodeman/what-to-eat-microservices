using AutoMapper;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.DataTransferObjects.UserDTOs;
using Recipes.Data.Models;

namespace Recipes.BLL.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CookingStep, CookingStepDto>().ReverseMap();
        CreateMap<CookingStep, CookingStepDtoNoId>().ReverseMap();
        CreateMap<User, GetUserDto>().ReverseMap();


        CreateMap<Ingredient, IngredientDto>().ReverseMap();

        CreateMap<Recipe, RecipeDto>()
             .ForMember(dest => dest.Ingredients, opt => opt.MapFrom(src => src.Ingredients))
             .ForMember(dest => dest.SavedRecipes, opt => opt.MapFrom(src => src.Users!.Count))
             .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
             .ReverseMap();

        CreateMap<RecipeDtoWithIngredientsAndSteps, Recipe>()
        .ForMember(dest => dest.Ingredients, opt => { opt.MapFrom(src => src.Ingredients);opt.AllowNull(); })
        .ForMember(dest => dest.CookingSteps,opt => { opt.MapFrom(src => src.Steps); opt.AllowNull(); } )
        .ReverseMap();

        CreateMap<Respond, RespondDto>().ReverseMap();
        
        CreateMap<WeightUnit, WeightUnitDto>().ReverseMap();
    }
}