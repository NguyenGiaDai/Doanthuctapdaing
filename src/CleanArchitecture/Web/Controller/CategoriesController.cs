using CleanArchitecture.Application.DTO;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CleanArchitecture.Web.Controller;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(CreateCategoryDTO dto)
    {
        return Ok("Valid data");
    }
}
