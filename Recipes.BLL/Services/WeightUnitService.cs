using AutoMapper;
using Recipes.BLL.Helpers;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.DataTransferObjects;
using Recipes.Data.Models;
using Recipes.Data.Responses.Interfaces;

namespace Recipes.BLL.Services;

public class WeightUnitService : IWeightUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseCreator _responseCreator;

    public WeightUnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseCreator = new ResponseCreator();
    }

    public Task<IBaseResponse<WeightUnitDto>> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IBaseResponse<List<WeightUnitDto>>> Get()
    {
        try
        {
            var models = await _unitOfWork.WeightUnitRepository.GetAsync();

            models = models.OrderBy(model => model.Type).DistinctBy(p=>p.Type).ToList();
            if (models.Count is 0)
                return _responseCreator.CreateBaseNotFound<List<WeightUnitDto>>("No weight units found.");

            var dtoList = models.Select(model => _mapper.Map<WeightUnitDto>(model)).ToList();

            return _responseCreator.CreateBaseOk(dtoList, dtoList.Count);
        }
        catch(Exception e) 
        {
            return _responseCreator.CreateBaseServerError<List<WeightUnitDto>>(e.Message);
        }
    }

    public async Task<IBaseResponse<string>> Insert(WeightUnitDto? modelDto)
    {
        try
        {
            if (modelDto is null) 
                return _responseCreator.CreateBaseBadRequest<string>("Weight unit is empty.");
            
            await _unitOfWork.WeightUnitRepository.InsertAsync(_mapper.Map<WeightUnit>(modelDto));
            await _unitOfWork.SaveChangesAsync();

            return _responseCreator.CreateBaseOk($"Weight unit added.", 1);
        }
        catch (Exception e)
        {
            return _responseCreator.CreateBaseServerError<string>(e.Message);
        }
    }
}