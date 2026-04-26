using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("product")]
        public async Task<IActionResult> Create([FromBody] ProductDTO dto)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            Product product = await _productService.CreateAsync(dto);
            if (product != null)
            {
                return Ok(product);
            }
            return BadRequest(product);
        }
        [HttpGet("products")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);

        }
        [HttpGet("product/{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product != null) { return Ok(product); }
            return BadRequest(product);
        }

        [HttpPut("product/{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id,[FromBody]ProductDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = await _productService.UpdateAsync(id, dto);
            if (product != null) 
            {
                return Ok(product);
            }
            return BadRequest(product);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("product/{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            bool delete = await _productService.DeleteAsync(id);
            if (delete)
            {
                return Ok("Delete Thanh cong");
            }
            return BadRequest("Delete That Bai");
        }
    }
}
