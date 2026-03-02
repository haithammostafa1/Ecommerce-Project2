using EcommerceBuisnessLayer; // تأكد إن الاسم مطابق للمشروع عندك (Business vs Buisness)
using EcommerceDataLayer;
using EcommerceDataLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

namespace EcommrceApi.Controllers
{
  
    [Route("api/EcommerceApi")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoryService;

        public CategoriesController(ICategoriesService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{Id}", Name = "GetCategoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDto>> GetCategoryByID(int Id)
        {
            Log.Information("API: Received Request To Get Category By {Id}", Id);

            if (Id < 1)
            {
                Log.Warning("API: BadRequest Wrong {ID}", Id);
                return BadRequest("Wrong Id");
            }

            var categoryDto = await _categoryService.GetCategoryById(Id);

            if (categoryDto == null)
            {
                Log.Warning("API: Category Not Found with {id}", Id);
                return NotFound($"No Category with ID {Id}");
            }

            Log.Information("API: Category Found By {id}", Id);
            return Ok(categoryDto);
        }



        [HttpPut("Update/{id}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory(
        int id,
         [FromBody] CategoryDto updatedCategory)
        {
            Log.Information("API: Received request to update category. ID: {Id}", id);

            // Basic request validation 
            if (updatedCategory == null || id != updatedCategory.Id)
                return BadRequest("Invalid request data.");

            var result = await _categoryService.UpdateCategory(updatedCategory);

            return result switch
            {
                CategoryOperationResult.Success =>
                    Ok(updatedCategory),

                CategoryOperationResult.DuplicateName =>
                    Conflict("Category name already exists."),

                CategoryOperationResult.NotFound =>
                    NotFound($"Category with ID {id} not found."),

                CategoryOperationResult.InvalidData =>
                    BadRequest("Invalid category data."),

                _ =>
                    StatusCode(500, "Unexpected error while updating category.")
            };
        }
        [HttpPost("AddNew", Name = "AddNewCategory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDto>> AddNewCategory([FromBody] CategoryDto newCategory)
        {
            Log.Information("API: Received Request To Add New Category: {Name}", newCategory.Name);
            var result = await _categoryService.AddNewCategory(newCategory);
            return result switch
            {
                CategoryOperationResult.Success =>
                CreatedAtRoute("GetCategoryById", new { id = newCategory.Id }, newCategory),
                CategoryOperationResult.DuplicateName => BadRequest("Category name already exists."),
                CategoryOperationResult.InvalidData => BadRequest("Invalid category data."),
                CategoryOperationResult.Failed => StatusCode(500, "Failed to create category."),
                _ => StatusCode(500, "Unexpected error.")
            };


        }

        [HttpGet("GetCategoriesPaged", Name = "GetCategoriesPaged")]
        public async Task<IActionResult> GetCategoriesPages([FromQuery] PaginationParams pParams)
        {
            try
            {
                Log.Information("API: Received Request. Page: {Page}, Size: {Size}", pParams.PageNumber, pParams.PageSize);

                var pagedList = await _categoryService.GetCategoriesPaged(pParams);

                var metaData = new
                {
                    pagedList.MetaData.TotalCount,
                    pagedList.MetaData.PageSize,
                    pagedList.MetaData.CurrentPage,
                    pagedList.MetaData.TotalPages,
                    pagedList.MetaData.HasNext,
                    pagedList.MetaData.HasPrevious
                };

                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData));
                return Ok(pagedList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "API: Fatal Error in GetCategoriesPaged Endpoint");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Log.Information("API: Request to delete Category ID: {Id}", id);

            if (id < 1) return BadRequest("Invalid ID.");

            var result = await _categoryService.DeleteCategory(id);

            return result switch
            {
                CategoryOperationResult.Success =>
                    Ok(new { Message = $"Category {id} deleted successfully." }),

                CategoryOperationResult.NotFound =>
                    NotFound($"Category with ID {id} not found."),

                CategoryOperationResult.HasProducts =>
                    Conflict("Cannot delete category because it has products."),

                CategoryOperationResult.HasChildren =>
                    Conflict("Cannot delete category because it has sub-categories."),

                CategoryOperationResult.InvalidData =>
                    BadRequest("Invalid category ID."),

                _ => StatusCode(500, "Unexpected error while deleting category.")

            };
        }

    }
}
