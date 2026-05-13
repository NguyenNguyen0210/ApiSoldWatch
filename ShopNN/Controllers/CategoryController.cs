using ShopNN.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<List<CategoryResponseDTO>>.SuccessResult(categories, "Categories retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponse<CategoryResponseDTO>.SuccessResult(category, "Category retrieved successfully"));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, ApiResponse<CategoryResponseDTO>.SuccessResult(category, "Category created successfully"));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            var category = await _categoryService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CategoryResponseDTO>.SuccessResult(category, "Category updated successfully"));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result) throw new NotFoundException("Category not found");
            return Ok(ApiResponse<object>.SuccessResult("Category deleted successfully"));
        }
    }
}
