using EcommerceBuisnessLayer;
using EcommerceDataLayer;
using EcommerceDataLayer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace EcommrceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   

    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            Log.Information("API: Request to delete Product ID: {Id}", id);

            if (id < 1) return BadRequest("Invalid Product ID.");

            // الاتصال بالسيرفس
            var result = await _productService.DeleteProduct(id);

            return result switch
            {
                ProductOperationResult.Success => Ok(new { message = "Product Deleted" }),
                ProductOperationResult.NotFound => NotFound(new { message = "NotFound Can Not Delete" }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Delete Failed" })

            };
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddNewProduct", Name = "AddNewProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDTO>> AddNewProduct(ProductDTO productDTO)
        {
            Log.Information("API :Attempting To AddNew Product Name : {Name}", productDTO.Name);

            var result = await _productService.AddNewProduct(productDTO);
            return result switch
            {
                ProductOperationResult.Success =>
            CreatedAtRoute("GetProductById", new { id = productDTO.ID }, productDTO),
                ProductOperationResult.DuplicateName => BadRequest(new { message = "Product name already exists." }),
                ProductOperationResult.InvalidData => BadRequest(new { message = "Invalid Product data." }),
                ProductOperationResult.Failed => StatusCode(500, new { message = "Failed to create Product." }),
                _ => StatusCode(500, new { message = "Unexpected error." })
            };


        }
        [HttpGet("{ProductId}", Name = "GetProductById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDTO>> GetProductById(int ProductId)
        {


            if (ProductId < 0)
            {
                Log.Warning("API: BadRequest Wrong {ID}", ProductId);
                return BadRequest("Wrong Id");
            }
            var Product = await _productService.GetProductById(ProductId);
            if (Product == null)
            {

                Log.Warning($"Product {ProductId} does not exist");
                return NotFound($"Product ProductId :{ProductId} does not exist");
            }

            Log.Information("API: Product Found By {id}", ProductId);
            return Ok(Product);

        }
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO dTO)
        {
            Log.Information("API: Request to update Product ID: {Id}", id);

            if (dTO == null || id != dTO.ID)
            {
                return BadRequest("Invalid Id Or MisMatch Data");
            }

            var Result = await _productService.UpdatedProduct(dTO);

            return Result switch
            {
                ProductOperationResult.Success => Ok(dTO),
                ProductOperationResult.NotFound => NotFound($"ptoduct with this id {dTO.ID}"),
                ProductOperationResult.InvalidData =>
                      BadRequest("Invalid ptoduct data."),

                _ =>
                    StatusCode(500, "Unexpected error while updating ptoduct.")

            };

        }
        [AllowAnonymous]
        [HttpGet("GetAllProducts", Name = "GetAllProductsPaged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsPaged([FromQuery] PaginationParams pParams)
        {
            try
            {
                Log.Information("API: Received Request for Products. Page: {Page}, Size: {Size}", pParams.PageNumber, pParams.PageSize);

                var pagedList = await _productService.GetAllProducts(pParams);

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
                Log.Error(ex, "API: Fatal Error in GetProductsPaged Endpoint");
                return StatusCode(500, "Internal server error.");
            }
        }
    }












}

