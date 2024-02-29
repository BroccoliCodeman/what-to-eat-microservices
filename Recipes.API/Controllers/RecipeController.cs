using Microsoft.AspNetCore.Mvc;
using Recipes.BLL.Interfaces;
using Recipes.Data.DataTransferObjects;

namespace Recipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _service;

    public RecipeController(IRecipeService service)
    {
        _service = service;
    }
    
    [HttpGet("Get")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> Get()
    {
        return Ok(await _service.Get());
    }
    
    [HttpPost]
    public async Task<ActionResult> Insert([FromBody] RecipeDto modelDto)
    {
        return Ok(await _service.Insert(modelDto));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        return Ok(await _service.DeleteById(id));
    }
}