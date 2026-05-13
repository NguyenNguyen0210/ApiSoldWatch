using ShopNN.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exeptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var product = await _productService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponse<ProductResponseDTO>.SuccessResult(product, "Product created"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<List<ProductResponseDTO>>.SuccessResult(products, "Products retrieved"));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ProductQueryDTO query)
        {
            var result = await _productService.GetPagedAsync(query);
            return Ok(ApiResponse<PagedResult<ProductResponseDTO>>.SuccessResult(result, "Products retrieved"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductResponseDTO>.SuccessResult(product, "Product retrieved"));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var product = await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductResponseDTO>.SuccessResult(product, "Product updated"));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);

            if (!deleted)
                throw new NotFoundException("Product not found");

            return Ok(ApiResponse<object>.SuccessResult("Product deleted"));
        }
    }
}
