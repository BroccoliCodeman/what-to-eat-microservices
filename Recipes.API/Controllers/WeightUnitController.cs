using Microsoft.AspNetCore.Mvc;
using Recipes.BLL.Interfaces;
using Recipes.Data.DataTransferObjects;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeightUnitController : ControllerBase
{
    private readonly IWeightUnitService _service;

    public WeightUnitController(IWeightUnitService service)
    {
        _service = service;
    }
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<WeightUnitDto>>> Get()
    {
        return Ok(await _service.Get());
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] WeightUnitDto modelDto)
    {
        return Ok(await _service.Insert(modelDto));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(int id)
    {
        return Ok(await _service.DeleteById(id));
    }
}