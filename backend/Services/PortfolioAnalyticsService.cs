using backend.Contracts.Analytics;
using backend.Contracts.Market;
using backend.Contracts.Snapshots;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PortfolioAnalyticsService : IPortfolioAnalyticsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMarketDataService _marketDataService;
    private readonly IFxRateService _fxRateService;
    private readonly ILogger<PortfolioAnalyticsService> _logger;

    public PortfolioAnalyticsService(
        ApplicationDbContext dbContext,
        IMarketDataService marketDataService,
        IFxRateService fxRateService,
        ILogger<PortfolioAnalyticsService> logger)
    {
        _dbContext = dbContext;
        _marketDataService = marketDataService;
        _fxRateService = fxRateService;
        _logger = logger;
    }

    public async Task<PortfolioAnalyticsDto?> GetPortfolioAnalyticsAsync(string userId, int portfolioId, CancellationToken cancellationToken = default)
    {
        var portfolio = await _dbContext.Portfolios
            .AsNoTracking()
            .Include(portfolio => portfolio.Holdings)
            .ThenInclude(holding => holding.Group)
            .FirstOrDefaultAsync(portfolio => portfolio.Id == portfolioId && portfolio.UserId == userId, cancellationToken);

        if (portfolio is null)
        {
            return null;
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == userId, cancellationToken);
        var baseCurrency = user?.BaseCurrency ?? "EUR";
        var rates = await _fxRateService.GetRatesAsync(cancellationToken);

        var positions = new List<PositionAnalyticsDto>();
        var missingSymbolsCount = 0;

        foreach (var holding in portfolio.Holdings)
        {
            var quote = await _marketDataService.GetQuoteAsync(holding.Symbol, cancellationToken);
            if (quote is null)
            {
                missingSymbolsCount++;
                continue;
            }

            var marketValueHoldingCurrency = holding.Quantity * quote.Price;
            var costBasisHoldingCurrency = holding.Quantity * holding.AveragePurchasePrice;
            var marketValueBase = _fxRateService.Convert(marketValueHoldingCurrency, holding.Currency, baseCurrency, rates);
            var costBasisBase = _fxRateService.Convert(costBasisHoldingCurrency, holding.Currency, baseCurrency, rates);
            var pnlBase = marketValueBase - costBasisBase;

            positions.Add(new PositionAnalyticsDto
            {
                HoldingId = holding.Id,
                Symbol = holding.Symbol,
                HoldingCurrency = holding.Currency,
                GroupName = holding.Group?.Name ?? "Uncategorized",
                Quantity = holding.Quantity,
                AveragePurchasePrice = holding.AveragePurchasePrice,
                CurrentPrice = quote.Price,
                MarketValueBase = marketValueBase,
                CostBasisBase = costBasisBase,
                UnrealizedPnLBase = pnlBase,
                UnrealizedPnLPercent = costBasisBase == 0 ? 0 : decimal.Round((pnlBase / costBasisBase) * 100m, 2)
            });
        }

        var totalMarket = positions.Sum(position => position.MarketValueBase);
        var totalCost = positions.Sum(position => position.CostBasisBase);
        var totalPnL = totalMarket - totalCost;
        var totalPnLPercent = totalCost == 0 ? 0 : decimal.Round((totalPnL / totalCost) * 100m, 2);

        var groupAllocations = positions
            .GroupBy(position => position.GroupName)
            .Select(group => new GroupAllocationDto
            {
                GroupId = portfolio.Holdings.FirstOrDefault(holding => holding.Group?.Name == group.Key)?.GroupId ?? 0,
                GroupName = group.Key,
                MarketValueBase = group.Sum(position => position.MarketValueBase),
                AllocationPercent = totalMarket == 0 ? 0 : decimal.Round(group.Sum(position => position.MarketValueBase) / totalMarket * 100m, 2)
            })
            .OrderByDescending(group => group.MarketValueBase)
            .ToList();

        var currencyExposures = positions
            .GroupBy(position => position.HoldingCurrency)
            .Select(group => new CurrencyExposureDto
            {
                Currency = group.Key,
                MarketValueBase = group.Sum(position => position.MarketValueBase),
                AllocationPercent = totalMarket == 0 ? 0 : decimal.Round(group.Sum(position => position.MarketValueBase) / totalMarket * 100m, 2)
            })
            .OrderByDescending(exposure => exposure.MarketValueBase)
            .ToList();

        return new PortfolioAnalyticsDto
        {
            PortfolioId = portfolio.Id,
            BaseCurrency = baseCurrency,
            TotalMarketValueBase = decimal.Round(totalMarket, 2),
            TotalCostBasisBase = decimal.Round(totalCost, 2),
            TotalUnrealizedPnLBase = decimal.Round(totalPnL, 2),
            TotalUnrealizedPnLPercent = totalPnLPercent,
            IsPartial = missingSymbolsCount > 0,
            MissingSymbolsCount = missingSymbolsCount,
            Positions = positions,
            GroupAllocations = groupAllocations,
            CurrencyExposures = currencyExposures
        };
    }

    public async Task<PortfolioSnapshotDto?> CreateSnapshotAsync(string userId, int portfolioId, CancellationToken cancellationToken = default)
    {
        var analytics = await GetPortfolioAnalyticsAsync(userId, portfolioId, cancellationToken);
        if (analytics is null)
        {
            return null;
        }

        var rates = await _fxRateService.GetRatesAsync(cancellationToken);

        var snapshot = new PortfolioSnapshot
        {
            PortfolioId = portfolioId,
            UserId = userId,
            CapturedAtUtc = DateTime.UtcNow,
            BaseCurrency = analytics.BaseCurrency,
            TotalMarketValueBase = analytics.TotalMarketValueBase,
            TotalCostBasisBase = analytics.TotalCostBasisBase,
            TotalUnrealizedPnLBase = analytics.TotalUnrealizedPnLBase,
            IsPartial = analytics.IsPartial,
            MissingSymbolsCount = analytics.MissingSymbolsCount,
            FxTimestampUtc = rates.RetrievedAtUtc,
            EurUsdRate = rates.EurUsd,
            EurRonRate = rates.EurRon
        };

        _dbContext.PortfolioSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSnapshot(snapshot);
    }

    public async Task<IReadOnlyList<PortfolioSnapshotDto>> GetSnapshotsAsync(string userId, int portfolioId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Portfolios
            .AsNoTracking()
            .AnyAsync(portfolio => portfolio.Id == portfolioId && portfolio.UserId == userId, cancellationToken);
        if (!exists)
        {
            return [];
        }

        var query = _dbContext.PortfolioSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.PortfolioId == portfolioId && snapshot.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(snapshot => snapshot.CapturedAtUtc >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(snapshot => snapshot.CapturedAtUtc <= to.Value);
        }

        var snapshots = await query
            .OrderByDescending(snapshot => snapshot.CapturedAtUtc)
            .ToListAsync(cancellationToken);

        return snapshots.Select(MapSnapshot).ToList();
    }

    public async Task<IReadOnlyList<PortfolioTimeSeriesPointDto>> GetTimeSeriesAsync(string userId, int portfolioId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PortfolioSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.PortfolioId == portfolioId && snapshot.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(snapshot => snapshot.CapturedAtUtc >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(snapshot => snapshot.CapturedAtUtc <= to.Value);
        }

        var snapshots = await query
            .OrderBy(snapshot => snapshot.CapturedAtUtc)
            .ToListAsync(cancellationToken);

        return snapshots.Select(snapshot => new PortfolioTimeSeriesPointDto
        {
            CapturedAtUtc = snapshot.CapturedAtUtc,
            TotalMarketValueBase = snapshot.TotalMarketValueBase,
            TotalCostBasisBase = snapshot.TotalCostBasisBase,
            TotalUnrealizedPnLBase = snapshot.TotalUnrealizedPnLBase,
            IsPartial = snapshot.IsPartial
        }).ToList();
    }

    private static PortfolioSnapshotDto MapSnapshot(PortfolioSnapshot snapshot)
    {
        return new PortfolioSnapshotDto
        {
            Id = snapshot.Id,
            PortfolioId = snapshot.PortfolioId,
            CapturedAtUtc = snapshot.CapturedAtUtc,
            BaseCurrency = snapshot.BaseCurrency,
            TotalMarketValueBase = snapshot.TotalMarketValueBase,
            TotalCostBasisBase = snapshot.TotalCostBasisBase,
            TotalUnrealizedPnLBase = snapshot.TotalUnrealizedPnLBase,
            IsPartial = snapshot.IsPartial,
            MissingSymbolsCount = snapshot.MissingSymbolsCount,
            FxTimestampUtc = snapshot.FxTimestampUtc,
            EurUsdRate = snapshot.EurUsdRate,
            EurRonRate = snapshot.EurRonRate
        };
    }
}
