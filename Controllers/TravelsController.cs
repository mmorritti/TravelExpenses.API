using Microsoft.AspNetCore.Mvc;
using TravelExpenses.Api.Dtos;
using TravelExpenses.Api.Services.ExchangeRates;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelsController : ControllerBase
{
    private readonly ITravelService _travelService;
    private readonly ITravelCurrencyRateService _travelCurrencyRateService;
    private readonly IExpanseService _expanseService;
    private readonly ICategoryService _categoryService;
    private readonly IExchangeRateService _exchangeRateService;

    public TravelsController(
        ITravelService travelService,
        ITravelCurrencyRateService travelCurrencyRateService,
        IExpanseService expanseService,
        ICategoryService categoryService,
        IExchangeRateService exchangeRateService)
    {
        _travelService = travelService;
        _travelCurrencyRateService = travelCurrencyRateService;
        _expanseService = expanseService;
        _categoryService = categoryService;
        _exchangeRateService = exchangeRateService;
    }

    // GET: api/travels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TravelDto>>> GetAll()
    {
        var travels = await _travelService.GetAllAsync();

        var result = travels
            .Select(t => new TravelDto
            {
                TravelId = t.TravelId,
                Name = t.Name,
                CountryCode = t.CountryCode,
                HomeCurrencyCode = t.HomeCurrencyCode,
                TravelCurrencyCode = t.TravelCurrencyCode,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            })
            .OrderBy(travel => travel.StartDate);

        return Ok(result);
    }

    // GET: api/travels/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TravelDto>> GetById(Guid id)
    {
        var travel = await _travelService.GetByIdAsync(id);
        if (travel is null)
            return NotFound();

        var dto = new TravelDto
        {
            TravelId = travel.TravelId,
            Name = travel.Name,
            CountryCode = travel.CountryCode,
            HomeCurrencyCode = travel.HomeCurrencyCode,
            TravelCurrencyCode = travel.TravelCurrencyCode,
            StartDate = travel.StartDate,
            EndDate = travel.EndDate
        };

        return Ok(dto);
    }

    // GET: api/travels/{id}/summary
    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<TravelSummaryDto>> GetSummary(Guid id)
    {
        var travel = await _travelService.GetByIdAsync(id);
        if (travel is null)
            return NotFound();

        var summary = await BuildTravelSummaryAsync(travel);
        return Ok(summary);
    }

    // GET: api/travels/summaries
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<TravelSummaryDto>>> GetAllSummaries()
    {
        var travels = await _travelService.GetAllAsync();

        var list = new List<TravelSummaryDto>();
        foreach (var travel in travels)
        {
            var summary = await BuildTravelSummaryAsync(travel);
            list.Add(summary);
        }

        var ordered = list.OrderBy(t => t.StartDate);
        return Ok(ordered);
    }

    // POST: api/travels
    [HttpPost]
    public async Task<ActionResult<TravelDto>> Create([FromBody] CreateTravelRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var travel = new Travel
        {
            TravelId = Guid.NewGuid(),
            Name = request.Name,
            CountryCode = request.CountryCode,
            HomeCurrencyCode = request.HomeCurrencyCode,
            TravelCurrencyCode = request.TravelCurrencyCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _travelService.CreateAsync(travel);

        var dto = new TravelDto
        {
            TravelId = travel.TravelId,
            Name = travel.Name,
            CountryCode = travel.CountryCode,
            HomeCurrencyCode = travel.HomeCurrencyCode,
            TravelCurrencyCode = travel.TravelCurrencyCode,
            StartDate = travel.StartDate,
            EndDate = travel.EndDate
        };

        // crea automaticamente il rate di cambio usando il servizio esterno
        var rateToBase = await _exchangeRateService.GetRateToEurAsync(travel.TravelCurrencyCode);

        if (rateToBase.HasValue)
        {
            var rate = new TravelCurrencyRate
            {
                TravelCurrencyRateId = Guid.NewGuid(),
                TravelId = travel.TravelId,
                CurrencyCode = travel.TravelCurrencyCode,
                RateToBase = rateToBase.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _travelCurrencyRateService.CreateAsync(rate);
        }

        return CreatedAtAction(nameof(GetById), new { id = dto.TravelId }, dto);
    }

    // PUT: api/travels/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTravelRequest request)
    {
        var existing = await _travelService.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.Name = request.Name;
        existing.CountryCode = request.CountryCode;
        existing.HomeCurrencyCode = request.HomeCurrencyCode;
        existing.TravelCurrencyCode = request.TravelCurrencyCode;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _travelService.UpdateAsync(existing);

        return NoContent();
    }

    // DELETE: api/travels/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _travelService.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        await _travelService.DeleteAsync(id);

        return NoContent();
    }

    #region Private Methods

    private async Task<TravelSummaryDto> BuildTravelSummaryAsync(Travel travel)
    {
        var expanses = await _expanseService.GetByTravelIdAsync(travel.TravelId);

        var rates = await _travelCurrencyRateService.GetByTravelIdAsync(travel.TravelId);
        var ratesDict = rates.ToDictionary(r => r.CurrencyCode, r => r.RateToBase);

        decimal travelToHomeRate = 1m;
        if (ratesDict.TryGetValue(travel.TravelCurrencyCode, out var mainRate))
        {
            travelToHomeRate = mainRate;
        }

        decimal totalHome = 0m;
        decimal totalTravel = 0m;

        var allCategories = await _categoryService.GetAllAsync();
        var catDict = allCategories.ToDictionary(c => c.CategoryId, c => c.Name);

        var categoryTotals = new Dictionary<Guid, (decimal travelTotal, decimal homeTotal)>();

        foreach (var e in expanses)
        {
            // importo in valuta di casa
            decimal amountHome;
            if (e.CurrencyCode == travel.HomeCurrencyCode)
            {
                amountHome = e.Amount;
            }
            else if (ratesDict.TryGetValue(e.CurrencyCode, out var rateToBase))
            {
                amountHome = e.Amount * rateToBase;
            }
            else
            {
                amountHome = e.Amount;
            }

            // importo in valuta del viaggio
            decimal amountTravel;
            if (travelToHomeRate > 0)
            {
                amountTravel = amountHome / travelToHomeRate;
            }
            else
            {
                amountTravel = e.Amount;
            }

            totalHome += amountHome;
            totalTravel += amountTravel;

            if (!categoryTotals.TryGetValue(e.CategoryId, out var totals))
            {
                totals = (0m, 0m);
            }

            totals.travelTotal += amountTravel;
            totals.homeTotal += amountHome;
            categoryTotals[e.CategoryId] = totals;
        }

        var summary = new TravelSummaryDto
        {
            TravelId = travel.TravelId,
            Name = travel.Name,
            CountryCode = travel.CountryCode,
            HomeCurrencyCode = travel.HomeCurrencyCode,
            TravelCurrencyCode = travel.TravelCurrencyCode,
            StartDate = travel.StartDate,
            EndDate = travel.EndDate,
            TotalInHomeCurrency = totalHome,
            TotalInTravelCurrency = totalTravel,
            ExchangeRateTravelToHome = travelToHomeRate,
            Categories = categoryTotals
                .Select(ct =>
                {
                    var categoryId = ct.Key;
                    var totals = ct.Value;
                    catDict.TryGetValue(categoryId, out var catName);

                    var percentage = totalHome > 0
                        ? Math.Round((totals.homeTotal / totalHome) * 100, 2)
                        : 0m;

                    return new CategorySummaryDto
                    {
                        CategoryId = categoryId,
                        CategoryName = catName ?? "Sconosciuta",
                        TotalInTravelCurrency = totals.travelTotal,
                        TotalInHomeCurrency = totals.homeTotal,
                        PercentageOfTotalHomeCurrency = percentage
                    };
                })
                .OrderByDescending(c => c.TotalInHomeCurrency)
                .ToList()
        };

        return summary;
    }

    #endregion
}
