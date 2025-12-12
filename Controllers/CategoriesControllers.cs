using Microsoft.AspNetCore.Mvc;
using TravelExpenses.Api.Dtos;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        var result = categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Icon = c.Icon,
            ColorHex = c.ColorHex,
            SortOrder = c.SortOrder
        }).OrderBy(category => category.SortOrder);

        return Ok(result);
    }

    // GET: api/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        var dto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Icon = category.Icon,
            ColorHex = category.ColorHex,
            SortOrder = category.SortOrder
        };

        return Ok(dto);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = request.Name,
            Icon = request.Icon,
            ColorHex = request.ColorHex,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _categoryService.CreateAsync(category);

        var dto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Icon = category.Icon,
            ColorHex = category.ColorHex,
            SortOrder = category.SortOrder
        };

        // ritorna 201 con Location: api/categories/{id}
        return CreatedAtAction(nameof(GetById), new { id = dto.CategoryId }, dto);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.Name = request.Name;
        existing.Icon = request.Icon;
        existing.ColorHex = request.ColorHex;
        existing.SortOrder = request.SortOrder;
        existing.UpdatedAt = DateTime.UtcNow;

        await _categoryService.UpdateAsync(existing);

        return NoContent();
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        await _categoryService.DeleteAsync(id);

        return NoContent();
    }
}
