using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Helpers;
using Recipes.Data.Models;
using Recipes.Data.Responses;
using Recipes.Data.Responses.Enums;
using Recipes.Data.Responses.Interfaces;
using System.Net.WebSockets;

namespace Recipes.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public RecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();

    }

    public async Task<IBaseResponse<RecipeIntroDto>> GetRandom()
    {
        try
        {
            var recipes = await _unitOfWork.RecipeRepository.GetAsync();
            
            var recipe = recipes.MinBy(x => Guid.NewGuid());
            
            if (recipe == null)
                return _responseCreator.CreateBaseNotFound<RecipeIntroDto>("No random recipe found.");

            return _responseCreator.CreateBaseOk(_mapper.Map<RecipeIntroDto>(recipe), 1);
        }
        catch (Exception ex)
        {
            return _responseCreator.CreateBaseServerError<RecipeIntroDto>(ex.Message);
        }
    }

    public async Task<IBaseResponse<RecipeDto>> GetById(Guid id, string? username)
    {
        try
        {
            var recipe = await _unitOfWork.RecipeRepository.GetByIdAsync(id);
             var user = await _unitOfWork.DatabaseContext.Users.Include(p=>p.SavedRecipes).AsNoTracking().FirstOrDefaultAsync(p=>p.UserName == username);

            if (recipe == null)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>($"The recipe with id {id} wasn't found", StatusCode.NotFound);
            }


            var recipeDto = _mapper.Map<RecipeDto>(recipe);
            if (user != null)
            {
                recipeDto.isSavedByCurrentUser = user.SavedRecipes.Any(p => p.Id == recipeDto.Id);
            }

            recipeDto.CookingSteps = recipeDto.CookingSteps?.DistinctBy(step => step.Description).ToList();

            return BaseResponse<RecipeDto>.CreateBaseResponse("Success!", StatusCode.Ok, recipeDto);
        }
        catch (Exception ex)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>(ex.Message, StatusCode.InternalServerError);
        }
    }
    public async Task<IBaseResponse<List<RecipeIntroDto>>> GetByUserIdRecipes(Guid UserId)
    {
        try
        {
            var recipes = await _unitOfWork.RecipeRepository.GetByUserIdAsync(UserId);

            if (recipes.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No recipes found.");

            var RecipeDtos = recipes
                .Select(recipe => _mapper.Map<RecipeDto>(recipe))
                .OrderByDescending(recipe => recipe.SavesCount)
                .ToList();

            if (RecipeDtos.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No popular recipes found.");

            var recipeIntroDtos = RecipeDtos.Select(recipe => _mapper.Map<RecipeIntroDto>(recipe)).ToList();

            return _responseCreator.CreateBaseOk(recipeIntroDtos, recipeIntroDtos.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeIntroDto>>(e.Message);
        }
    }
    public async Task<IBaseResponse<List<RecipeIntroDto>>> GetMostPopularRecipesTitles()
    {
        try
        {
            var recipes = await _unitOfWork.RecipeRepository.GetAsync();
            
            if (recipes.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No recipes found.");
            
            var popularRecipeDtos = recipes
                .Select(recipe => _mapper.Map<RecipeDto>(recipe))
                .OrderByDescending(recipe => recipe.SavesCount)
                .Take(5)
                .ToList();
            
            if (popularRecipeDtos.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No popular recipes found.");
            
            var recipeIntroDtos = popularRecipeDtos.Select(recipe => _mapper.Map<RecipeIntroDto>(recipe)).ToList();
            
            return _responseCreator.CreateBaseOk(recipeIntroDtos, recipeIntroDtos.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeIntroDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<List<RecipeIntroDto>>> GetByTitle(string title)
    {
        try
        {
            if (string.IsNullOrEmpty(title))
                return _responseCreator.CreateBaseBadRequest<List<RecipeIntroDto>>("Name can't be empty.");
            
            var recipes = await _unitOfWork.RecipeRepository.GetByTitle(title);
            
            if (recipes.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No recipes found.");

            var recipeIntroDtos = recipes.Select(recipe => _mapper.Map<RecipeIntroDto>(recipe)).ToList();
            
            return _responseCreator.CreateBaseOk(recipeIntroDtos, recipeIntroDtos.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeIntroDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<PagedList<RecipeDto>>> Get(PaginationParams paginationParams, SearchParams? searchParams, int sortType = 0)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync(paginationParams, searchParams);

            if (models.Count == 0)
                return _responseCreator.CreateBaseNotFound<PagedList<RecipeDto>>("No recipes found.");

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            //sorting
            switch (sortType)
            {
                case 0: break; // 0 - no sorting
                case 1: dtoList = dtoList.OrderBy(dto => dto.Title).ToList(); break; // 1 - asc by alphabet
                case 2: dtoList = dtoList.OrderByDescending(dto => dto.Title).ToList(); break; // 2 - desc by alphabet
                case 3: dtoList = dtoList.OrderBy(dto => dto.SavesCount).ToList(); break; // 3 - asc by savings
                case 4: dtoList = dtoList.OrderByDescending(dto => dto.SavesCount).ToList(); break; // 4 - desc by savings
                case 5: dtoList = dtoList.OrderBy(dto => dto.CreationDate).ToList(); break; // 5 - asc by creation date
                case 6: dtoList = dtoList.OrderByDescending(dto => dto.CreationDate).ToList(); break; // 6 - desc by creation date
                case 7: dtoList = dtoList.OrderBy(dto => dto.Calories).ToList(); break; // 7 - asc by calories
                case 8: dtoList = dtoList.OrderByDescending(dto => dto.Calories).ToList(); break; // 8 - desc by calories
                default: return _responseCreator.CreateBaseBadRequest<PagedList<RecipeDto>>("Invalid sort type."); break;
            }

            var pagedList = new PagedList<RecipeDto>(dtoList, models.TotalCount, models.CurrentPage, models.PageSize);

            return _responseCreator.CreateBaseOk(pagedList, pagedList.TotalCount);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<PagedList<RecipeDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(RecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();

            await _unitOfWork.RecipeRepository.InsertAsync(_mapper.Map<Recipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }
    public async Task<IBaseResponse<string>> Update(Guid recipeId,RecipeDto? modelDto)
    {
        try
        {
            if (modelDto is null)
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            if (recipeId == Guid.Empty)
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Id can`t be empty...", StatusCode.BadRequest);

            modelDto.Id = recipeId;
            await _unitOfWork.RecipeRepository.UpdateAsync(_mapper.Map<Recipe>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object Updated!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }
    public async Task<IBaseResponse<string>> SaveRecipe(Guid UserId, Guid RecipeId)
    {
        try
        {
            await _unitOfWork.RecipeRepository.SaveRecipe(UserId, RecipeId);

 

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Recipe saved!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> RemoveRecipeFromSaved(Guid UserId, Guid RecipeId)
    {
        try
        {
            await _unitOfWork.RecipeRepository.RemoveRecipeFromSaved(UserId, RecipeId);



            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Recipe Removed From Saved!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredientsAndSteps? recipereqest)
    {
        try
        {
            if (recipereqest is null)
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object can't be empty...", StatusCode.BadRequest);


            var recipe = _mapper.Map<Recipe>(recipereqest);

            recipe.Id = Guid.NewGuid();

            // �������� �� �����䳺��� � ���� �����
            var Ingredients = await _unitOfWork.IngredientRepository.GetAsync();

            // �������� ������ �����䳺��� � ����� DTO, �� ������ � ��� �����
            var newIngredients = recipe.Ingredients.Where(dtoIng => !Ingredients.AsReadOnly().Any(dbIng => dbIng.Name == dtoIng.Name && dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type && dbIng.Quantity == dtoIng.Quantity));

            var existingIngredients = Ingredients.Where(dbIng =>
                recipe.Ingredients.Any(dtoIng =>
                dbIng.Name == dtoIng.Name &&
                dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type &&
                dbIng.Quantity == dtoIng.Quantity));

            var steps = _mapper.Map<ICollection<CookingStepDtoNoId>, ICollection<CookingStep>>(recipereqest.CookingSteps);

            
            foreach(var step in steps)
            {

                step.Id = Guid.NewGuid();

                step.RecipeId = recipe.Id;
                await _unitOfWork.CookingStepRepository.InsertAsync(step);


            }

            var allUnits = await _unitOfWork.WeightUnitRepository.GetAsync();
            foreach (var newIngredient in newIngredients)
            {
                newIngredient.Id = Guid.NewGuid();
                var unit = allUnits.FirstOrDefault(p => p.Type == newIngredient.WeightUnit.Type);

                if (unit == null)
                {
               
                    await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(newIngredient.WeightUnit));
                }
                else
                    newIngredient.WeightUnit = unit;
              await  _unitOfWork.IngredientRepository.InsertAsync(newIngredient);
            }
            foreach (var existingIngredient in existingIngredients)
            {
                var recipesIngredient = recipe.Ingredients.FirstOrDefault(p =>
                    p.Name == existingIngredient.Name &&
                    p.WeightUnit.Type == existingIngredient.WeightUnit.Type &&
                    p.Quantity == existingIngredient.Quantity);

                if (recipesIngredient != null)
                {

                    var unitreq = await _unitOfWork.WeightUnitRepository.GetAsync();
                     var unit = unitreq.FirstOrDefault(p => p.Type == recipesIngredient.WeightUnit.Type);
                    
                    if (unit == null)
                    {
                        await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(recipesIngredient.WeightUnit));
                    }
                    else
                    {
                        recipesIngredient.WeightUnit = unit;
                    }

                    
                    recipe.Ingredients.Remove(recipesIngredient);
                    recipe.Ingredients.Add(existingIngredient);
                }
            }


            // �������� ������ � ���� �����
            await _unitOfWork.RecipeRepository.InsertAsync(recipe);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError, recipereqest.ToString());
        }
    }
    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.RecipeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<List<RecipeIntroDto>>> GetByUserId(Guid UserId)
    {
        try
        {
            if (UserId == Guid.Empty)
                return _responseCreator.CreateBaseBadRequest<List<RecipeIntroDto>>("UserId can't be empty.");

            var recipes = await _unitOfWork.RecipeRepository.GetAsync();

            recipes = recipes.Where(x => x.SavedByUsers.Any(p=>p.Id == UserId)).ToList();
            if (recipes.Count == 0)
                return _responseCreator.CreateBaseNotFound<List<RecipeIntroDto>>("No recipes found.");

            var recipeIntroDtos = recipes.Select(recipe => _mapper.Map<RecipeIntroDto>(recipe)).ToList();

            return _responseCreator.CreateBaseOk(recipeIntroDtos, recipeIntroDtos.Count);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<List<RecipeIntroDto>>(e.Message);
        }
    }
}