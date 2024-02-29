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
                return CreateBaseResponse<RecipeDto>($"The recipe with id {id} wasn't found", StatusCode.NotFound);
            }

            return CreateBaseResponse<RecipeDto>("Sucess!", StatusCode.Ok, _mapper.Map<RecipeDto>(recipe));
        }
        catch (Exception ex)
        {
            return CreateBaseResponse<RecipeDto>(ex.Message, StatusCode.InternalServerError);

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
    

}