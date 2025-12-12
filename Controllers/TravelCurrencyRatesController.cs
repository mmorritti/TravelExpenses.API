using Microsoft.AspNetCore.Mvc;
using TravelExpenses.Api.Dtos;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Controllers;

[ApiController]
[Route("api/travels/{travelId:guid}/rates")]
public class TravelCurrencyRatesController : ControllerBase
{
    private readonly ITravelCurrencyRateService _rateService;
    private readonly ITravelService _travelService;

    public TravelCurrencyRatesController(
        ITravelCurrencyRateService rateService,
        ITravelService travelService)
    {
        _rateService = rateService;
        _travelService = travelService;
    }

    // GET: api/travels/{travelId}/rates
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TravelCurrencyRateDto>>> GetRates(Guid travelId)
    {
        var travel = await _travelService.GetByIdAsync(travelId);
        if (travel is null)
            return NotFound($"Travel {travelId} non trovato");

        var rates = await _rateService.GetByTravelIdAsync(travelId);

        var result = rates.Select(r => new TravelCurrencyRateDto
        {
            TravelCurrencyRateId = r.TravelCurrencyRateId,
            TravelId = r.TravelId,
            CurrencyCode = r.CurrencyCode,
            RateToBase = r.RateToBase
        });

        return Ok(result);
    }

    // GET: api/travels/{travelId}/rates/{rateId}
    [HttpGet("{rateId:guid}")]
    public async Task<ActionResult<TravelCurrencyRateDto>> GetRate(Guid travelId, Guid rateId)
    {
        var rate = await _rateService.GetByIdAsync(rateId);
        if (rate is null || rate.TravelId != travelId)
            return NotFound();

        var dto = new TravelCurrencyRateDto
        {
            TravelCurrencyRateId = rate.TravelCurrencyRateId,
            TravelId = rate.TravelId,
            CurrencyCode = rate.CurrencyCode,
            RateToBase = rate.RateToBase
        };

        return Ok(dto);
    }

    // POST: api/travels/{travelId}/rates
    [HttpPost]
    public async Task<ActionResult<TravelCurrencyRateDto>> CreateRate(
        Guid travelId,
        [FromBody] CreateTravelCurrencyRateRequest request)
    {
        var travel = await _travelService.GetByIdAsync(travelId);
        if (travel is null)
            return NotFound($"Travel {travelId} non trovato");

        var rate = new TravelCurrencyRate
        {
            TravelCurrencyRateId = Guid.NewGuid(),
            TravelId = travelId,
            CurrencyCode = request.CurrencyCode,
            RateToBase = request.RateToBase,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _rateService.CreateAsync(rate);

        var dto = new TravelCurrencyRateDto
        {
            TravelCurrencyRateId = rate.TravelCurrencyRateId,
            TravelId = rate.TravelId,
            CurrencyCode = rate.CurrencyCode,
            RateToBase = rate.RateToBase
        };

        return CreatedAtAction(
            nameof(GetRate),
            new { travelId = dto.TravelId, rateId = dto.TravelCurrencyRateId },
            dto);
    }

    // PUT: api/travels/{travelId}/rates/{rateId}
    [HttpPut("{rateId:guid}")]
    public async Task<IActionResult> UpdateRate(
        Guid travelId,
        Guid rateId,
        [FromBody] UpdateTravelCurrencyRateRequest request)
    {
        var existing = await _rateService.GetByIdAsync(rateId);
        if (existing is null || existing.TravelId != travelId)
            return NotFound();

        existing.CurrencyCode = request.CurrencyCode;
        existing.RateToBase = request.RateToBase;
        existing.UpdatedAt = DateTime.UtcNow;

        await _rateService.UpdateAsync(existing);

        return NoContent();
    }

    // DELETE: api/travels/{travelId}/rates/{rateId}
    [HttpDelete("{rateId:guid}")]
    public async Task<IActionResult> DeleteRate(Guid travelId, Guid rateId)
    {
        var existing = await _rateService.GetByIdAsync(rateId);
        if (existing is null || existing.TravelId != travelId)
            return NotFound();

        await _rateService.DeleteAsync(rateId);

        return NoContent();
    }
}
