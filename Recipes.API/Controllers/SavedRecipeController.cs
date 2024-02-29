using Microsoft.AspNetCore.Mvc;
using Recipes.BLL.Interfaces;
using Recipes.Data.DataTransferObjects;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SavedRecipeController : ControllerBase
{
    private readonly ISavedRecipeService _service;

    public SavedRecipeController(ISavedRecipeService service)
    {
        _service = service;
    }
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<SavedRecipeDto>>> Get()
    {
        return Ok(await _service.Get());
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] SavedRecipeDto modelDto)
    {
        return Ok(await _service.Insert(modelDto));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        return Ok(await _service.DeleteById(id));
    }
}