using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelExpenses.Api.Dtos;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpansesController : ControllerBase
{
    private readonly IExpanseService _expanseService;
    private readonly ITravelService _travelService;
    private readonly ICategoryService _categoryService;

    public ExpansesController(
        IExpanseService expanseService,
        ITravelService travelService,
        ICategoryService categoryService)
    {
        _expanseService = expanseService;
        _travelService = travelService;
        _categoryService = categoryService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    // GET: api/expanses?travelId=...
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpanseDto>>> GetAll([FromQuery] Guid? travelId)
    {
        IEnumerable<Expense> expanses;
        var userId = GetUserId();

        if (travelId.HasValue)
        {
            var travel = await _travelService.GetByIdAsync(travelId.Value);
            if (travel is null) return NotFound($"Travel {travelId} non trovato");
            if (travel.UserId != userId) return Forbid(); // Sicurezza

            expanses = await _expanseService.GetByTravelIdAsync(travelId.Value);
        }
        else
        {
            // Recupera TUTTE le spese dell'utente (di tutti i viaggi)
            expanses = await _expanseService.GetAllAsync(userId);
        }

        var result = expanses.Select(e => new ExpanseDto
        {
            ExpanseId = e.ExpenseId,
            TravelId = e.TravelId,
            CategoryId = e.CategoryId,
            ExpanseDate = e.ExpenseDate,
            Name = e.Name,
            Amount = e.Amount,
            CurrencyCode = e.CurrencyCode,
            Description = e.Description
        });

        return Ok(result);
    }

    // GET: api/expanses/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpanseDto>> GetById(Guid id)
    {
        var expanse = await _expanseService.GetByIdAsync(id);
        if (expanse is null) return NotFound();
        if (expanse.UserId != GetUserId()) return NotFound(); // Protezione

        var dto = new ExpanseDto
        {
            ExpanseId = expanse.ExpenseId,
            TravelId = expanse.TravelId,
            CategoryId = expanse.CategoryId,
            ExpanseDate = expanse.ExpenseDate,
            Name = expanse.Name,
            Amount = expanse.Amount,
            CurrencyCode = expanse.CurrencyCode,
            Description = expanse.Description
        };

        return Ok(dto);
    }

    // POST: api/expanses
    [HttpPost]
    public async Task<ActionResult<ExpanseDto>> Create([FromBody] CreateExpanseRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        // Verifica Travel
        var travel = await _travelService.GetByIdAsync(request.TravelId);
        if (travel is null) return NotFound("Viaggio non trovato");
        if (travel.UserId != userId) return Forbid();

        // Verifica Category
        var category = await _categoryService.GetByIdAsync(request.CategoryId);
        if (category is null) return NotFound("Categoria non trovata");

        var expanse = new Expense
        {
            ExpenseId = Guid.NewGuid(),
            TravelId = request.TravelId,
            CategoryId = request.CategoryId,
            ExpenseDate = request.ExpanseDate,
            Name = request.Name,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            Description = request.Description,
            UserId = userId, // <--- Importante
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _expanseService.CreateAsync(expanse);

        var dto = new ExpanseDto
        {
            ExpanseId = expanse.ExpenseId,
            TravelId = expanse.TravelId,
            CategoryId = expanse.CategoryId,
            ExpanseDate = expanse.ExpenseDate,
            Name = expanse.Name,
            Amount = expanse.Amount,
            CurrencyCode = expanse.CurrencyCode,
            Description = expanse.Description
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.ExpanseId }, dto);
    }

    // PUT: api/expanses/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpanseRequest request)
    {
        var existing = await _expanseService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        if (existing.UserId != GetUserId()) return Forbid();

        // Opzionale: verificare se stai spostando la spesa su un viaggio non tuo
        if (existing.TravelId != request.TravelId)
        {
            var newTravel = await _travelService.GetByIdAsync(request.TravelId);
            if (newTravel == null || newTravel.UserId != GetUserId()) return BadRequest("Viaggio non valido");
        }

        existing.TravelId = request.TravelId;
        existing.CategoryId = request.CategoryId;
        existing.ExpenseDate = request.ExpanseDate;
        existing.Name = request.Name;
        existing.Amount = request.Amount;
        existing.CurrencyCode = request.CurrencyCode;
        existing.Description = request.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _expanseService.UpdateAsync(existing);

        return NoContent();
    }

    // DELETE: api/expanses/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _expanseService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        if (existing.UserId != GetUserId()) return Forbid();

        await _expanseService.DeleteAsync(id);

        return NoContent();
    }
}