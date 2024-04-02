using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecipeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IBaseResponse<RecipeDto>> GetById(Guid id)
    {
        try
        {
            var recipe = await _unitOfWork.RecipeRepository.GetByIdAsync(id);
            if (recipe == null)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>($"The recipe with id {id} wasn't found", StatusCode.NotFound);
            }

            return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>("Sucess!", StatusCode.Ok, _mapper.Map<RecipeDto>(recipe));
        }
        catch (Exception ex)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<RecipeDto>(ex.Message, StatusCode.InternalServerError);

        }
    }

    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
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


    public async Task<IBaseResponse<string>> InsertWithIngredients(RecipeDtoWithIngredients? recipe)
    {
        try
        {
            if (recipe is null)
                return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object can't be empty...", StatusCode.BadRequest);

            recipe.Id = Guid.NewGuid();

            // �������� �� �����䳺��� � ���� �����
            var existingIngredients = await _unitOfWork.IngredientRepository.GetAsync();

            // �������� ������ �����䳺��� � ����� DTO, �� ������ � ��� �����
            var newIngredients = recipe.ingredients.Where(dtoIng => !existingIngredients.Any(dbIng => dbIng.Name == dtoIng.Name && dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type && dbIng.Quantity == dtoIng.Quantity));

            // ������ ��� �����䳺��� � ���� �����
            foreach (var newIngredient in newIngredients)
            {
                newIngredient.Id = Guid.NewGuid();
                //���������� �� � ������� ����� �� ��������� � ����� � � ���
                var unit = _unitOfWork.WeightUnitRepository.GetAsync().Result.FirstOrDefault(p => p.Type == newIngredient.WeightUnit.Type);

                if (unit == null)
                {
               
                    await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(newIngredient.WeightUnit));
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                newIngredient.WeightUnit.Id = unit.Id;
            }

            // ������������ Id �����䳺���, �� ��� ������� � ��� �����
            /*
                        var existingIngredient = recipe.ingredients.Where(dtoIng => existingIngredients.Any(dbIng => dbIng.Name == dtoIng.Name && dbIng.WeightUnit.Type == dtoIng.WeightUnit.Type && dbIng.Quantity == dtoIng.Quantity))
                            for(int i=0;i<e)*/



            foreach (var existingIngredient in recipe.ingredients)
            {
                var existingDbIngredient = existingIngredients.FirstOrDefault(dbIng =>
                    dbIng.Name == existingIngredient.Name &&
                    dbIng.WeightUnit.Type == existingIngredient.WeightUnit.Type &&
                    dbIng.Quantity == existingIngredient.Quantity);

                if (existingDbIngredient != null)
                {
                    existingIngredient.Id = existingDbIngredient.Id;
                }
            }
            

            // �������� ������ � ���� �����
            await _unitOfWork.RecipeRepository.InsertAsync(_mapper.Map<Recipe>(recipe));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError, recipe.ToString());
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



    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByName(string name)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RecipeDto>(model)).ToList().Where(x => x.Title.Contains(name)); ;

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, dtoList.ToList().Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }


    /*    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
        {
            try
            {
                var models = await _unitOfWork.RecipeRepository.GetAsync();

                if (models.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
                }

                // ��������� �����䳺��� � ���� �����
                var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

                // Գ�������� �����䳺��� � ������ ingredients
                var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

                // ��������� �������, �� ������ ���� � ���� �����䳺�� � ������ ingredients
                var matchingRecipes = new List<Recipe>();

                foreach (var model in models)
                {
                    foreach (var ingredient in model.Ingredients)
                    {
                        if (filteredIngredients.Any(ing => ing.Name == ingredient.Name))
                        {
                            matchingRecipes.Add(model);
                            break;
                        }
                    }
                }

                if (matchingRecipes.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with any of the specified ingredients", StatusCode.NotFound);
                }

                // ��������� ��������� ������� �� DTO
                var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
            }
            catch (Exception e)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
            }
        }
    */

    /*    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
        {
            try
            {
                var models = await _unitOfWork.RecipeRepository.GetAsync();

                if (models.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
                }

                // ��������� �����䳺��� � ���� �����
                var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

                // Գ�������� �����䳺��� � ������ ingredients
                var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

                // ��������� �������, �� ������ �� �����䳺��� � ������ ingredients
                var matchingRecipes = new List<Recipe>();

                foreach (var model in models)
                {
                    bool hasAllIngredients = true;
                    foreach (var ingredientName in ingredients)
                    {
                        if (!model.Ingredients.Any(ing => ing.Name == ingredientName))
                        {
                            hasAllIngredients = false;
                            break;
                        }
                    }
                    if (hasAllIngredients)
                    {
                        matchingRecipes.Add(model);
                    }
                }

                if (matchingRecipes.Count == 0)
                {
                    return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with all of the specified ingredients", StatusCode.NotFound);
                }

                // ��������� ��������� ������� �� DTO
                var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
            }
            catch (Exception e)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
            }
        }*/
    public async Task<IBaseResponse<IEnumerable<RecipeDto>>> GetByIngredients(IEnumerable<string> ingredients)
    {
        try
        {
            var models = await _unitOfWork.RecipeRepository.GetAsync();

            if (models.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("0 objects found", StatusCode.NotFound);
            }

            // ��������� �����䳺��� � ���� �����
            var allIngredients = await _unitOfWork.IngredientRepository.GetAsync();

            // Գ�������� �����䳺��� � ������ ingredients
            var filteredIngredients = allIngredients.Where(ing => ingredients.Contains(ing.Name)).ToList();

            // ��������� �������, �� ������ ���� � ���� �����䳺�� � ������ ingredients, ��� �� ������ ������� ��� �����䳺��� � ������
            var matchingRecipes = new List<Recipe>();

            foreach (var model in models)
            {
                if (model.Ingredients.Any(ing => ingredients.Contains(ing.Name)) && !model.Ingredients.All(ing => !ingredients.Contains(ing.Name)))
                {
                    matchingRecipes.Add(model);
                }
            }

            if (matchingRecipes.Count == 0)
            {
                return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("No recipes found with any of the specified ingredients", StatusCode.NotFound);
            }

            // ��������� ��������� ������� �� DTO
            var dtoList = matchingRecipes.Select(model => _mapper.Map<RecipeDto>(model)).ToList();

            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>("Success!", StatusCode.Ok, dtoList, matchingRecipes.Count);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeDto>.CreateBaseResponse<IEnumerable<RecipeDto>>(e.Message, StatusCode.InternalServerError);
        }
    }


}