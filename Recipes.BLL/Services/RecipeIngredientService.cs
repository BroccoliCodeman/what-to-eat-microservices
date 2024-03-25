/*using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class RecipeIngredientService : IRecipeIngredientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RecipeIngredientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<RecipeIngredientDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<RecipeIngredientDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.RecipeIngredientRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<IEnumerable<RecipeIngredientDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<RecipeIngredientDto>(model)).ToList();

            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<IEnumerable<RecipeIngredientDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<IEnumerable<RecipeIngredientDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(RecipeIngredientDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();
                
            await _unitOfWork.RecipeIngredientRepository.InsertAsync(_mapper.Map<RecipeIngredient>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.RecipeIngredientRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<RecipeIngredientDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }
    
 
}*/