using AutoMapper;
using Recipes.BLL.Interfaces;
using Recipes.DAL.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Enums;
using Recipes.Data.Interfaces;
using Recipes.Data.Models;
using Recipes.Data.Responses;

namespace Recipes.BLL.Services;

public class WeightUnitService : IWeightUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WeightUnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<IBaseResponse<WeightUnitDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<IEnumerable<WeightUnitDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.WeightUnitRepository.GetAsync();

            if (models.Count is 0)
            {
                return BaseResponse<WeightUnitDto>.CreateBaseResponse<IEnumerable<WeightUnitDto>>("0 objects found", StatusCode.NotFound);
            }

            var dtoList = models.Select(model => _mapper.Map<WeightUnitDto>(model)).ToList();

            return BaseResponse<WeightUnitDto>.CreateBaseResponse<IEnumerable<WeightUnitDto>>("Success!", StatusCode.Ok, dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return BaseResponse<WeightUnitDto>.CreateBaseResponse<IEnumerable<WeightUnitDto>>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> Insert(WeightUnitDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return BaseResponse<WeightUnitDto>.CreateBaseResponse<string>("Objet can`t be empty...", StatusCode.BadRequest);
            
            modelDto.Id = Guid.NewGuid();
                
            await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<WeightUnitDto>.CreateBaseResponse<string>("Object inserted!", StatusCode.Ok, resultsCount: 1);

        }
        catch (Exception e)
        {
            return BaseResponse<WeightUnitDto>.CreateBaseResponse<string>(e.Message, StatusCode.InternalServerError);
        }
    }

    public async Task<IBaseResponse<string>> DeleteById(Guid id)
    {
        try
        {
            await _unitOfWork.WeightUnitRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return BaseResponse<WeightUnitDto>.CreateBaseResponse<string>("Object deleted!", StatusCode.Ok, resultsCount: 1);
        }
        catch (Exception e)
        {
            return BaseResponse<WeightUnitDto>.CreateBaseResponse<string>($"{e.Message} or object not found", StatusCode.InternalServerError);
        }
    }
    

}