using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class IngredientService : IIngredientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IngredientService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<IngredientDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<IngredientDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.IngredientRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<IngredientDto>.CreateBaseResponse<IEnumerable<IngredientDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<IngredientDto>(model)).ToList();

            return BaseResponse<IngredientDto>.CreateBaseResponse<IEnumerable<IngredientDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return BaseResponse<IngredientDto>.CreateBaseResponse<IEnumerable<IngredientDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(IngredientDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<IngredientDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();
                
            await _unitOfWork.IngredientRepository.InsertAsync(_mapper.Map<Ingredient>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<IngredientDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<IngredientDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.IngredientRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<IngredientDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<IngredientDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }
    

}