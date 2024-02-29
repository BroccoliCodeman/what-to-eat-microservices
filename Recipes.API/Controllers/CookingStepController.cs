using Microsoft.AspNetCore.Mvc;
using Recipes.BLL.Interfaces;
using Recipes.Data.DataTransferObjects;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CookingStepController : ControllerBase
{
    private readonly ICookingStepService _service;

    public CookingStepController(ICookingStepService service)
    {
        _service = service;
    }
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<CookingStepDto>>> Get()
    {
        return Ok(await _service.Get());
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] CookingStepDto modelDto)
    {
        return Ok(await _service.Insert(modelDto));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        return Ok(await _service.DeleteById(id));
    }
}