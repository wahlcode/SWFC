using SWFC.Application.M900_Intelligence;

namespace SWFC.Application.M900_Intelligence.M902_Prediction_Forecasting;

public sealed class PredictionForecastingService
{
    public IntelligenceForecastResult Forecast(IntelligenceForecastRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.HorizonPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Forecast horizon must be positive.");
        }

        var ordered = request.Points
            .OrderBy(x => x.Timestamp)
            .ThenBy(x => x.Value)
            .ToArray();

        if (ordered.Length == 0)
        {
            throw new ArgumentException("At least one source point is required.", nameof(request));
        }

        var slope = CalculateSlope(ordered);
        var intercept = ordered.Average(x => x.Value) - slope * ((ordered.Length - 1) / 2m);
        var meanAbsoluteError = CalculateMeanAbsoluteError(ordered, slope, intercept);
        var step = ResolveStep(ordered);
        var lastTimestamp = ordered.Last().Timestamp;
        var points = new List<IntelligenceForecastPoint>(request.HorizonPeriods);

        for (var period = 1; period <= request.HorizonPeriods; period++)
        {
            var index = ordered.Length - 1 + period;
            var expected = intercept + slope * index;
            var roundedExpected = Math.Round(expected, 2, MidpointRounding.AwayFromZero);
            var roundedError = Math.Round(meanAbsoluteError, 2, MidpointRounding.AwayFromZero);

            points.Add(new IntelligenceForecastPoint(
                period,
                lastTimestamp.AddTicks(step.Ticks * period),
                roundedExpected,
                Math.Round(roundedExpected - roundedError, 2, MidpointRounding.AwayFromZero),
                Math.Round(roundedExpected + roundedError, 2, MidpointRounding.AwayFromZero)));
        }

        var evidence = new IntelligenceEvidence(
            $"Samples={ordered.Length}; Source={request.SourceModule}; Metric={request.MetricCode}",
            "Deterministic linear trend over sorted points; uncertainty is mean absolute in-sample error.",
            $"TrendPerPeriod={Math.Round(slope, 4, MidpointRounding.AwayFromZero)}; MeanAbsoluteError={Math.Round(meanAbsoluteError, 2, MidpointRounding.AwayFromZero)}",
            "M902 returns forecasts as recommendations only and does not decide or execute operational changes.");

        return new IntelligenceForecastResult(
            request.SourceModule,
            request.MetricCode,
            request.Label,
            request.Unit,
            Math.Round(slope, 4, MidpointRounding.AwayFromZero),
            Math.Round(meanAbsoluteError, 2, MidpointRounding.AwayFromZero),
            points,
            evidence,
            IsRecommendationOnly: true);
    }

    private static decimal CalculateSlope(IReadOnlyList<IntelligenceSeriesPoint> ordered)
    {
        if (ordered.Count < 2)
        {
            return 0;
        }

        var meanX = (ordered.Count - 1) / 2m;
        var meanY = ordered.Average(x => x.Value);
        decimal numerator = 0;
        decimal denominator = 0;

        for (var index = 0; index < ordered.Count; index++)
        {
            var xDeviation = index - meanX;
            numerator += xDeviation * (ordered[index].Value - meanY);
            denominator += xDeviation * xDeviation;
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }

    private static decimal CalculateMeanAbsoluteError(
        IReadOnlyList<IntelligenceSeriesPoint> ordered,
        decimal slope,
        decimal intercept)
    {
        decimal total = 0;

        for (var index = 0; index < ordered.Count; index++)
        {
            var expected = intercept + slope * index;
            total += Math.Abs(ordered[index].Value - expected);
        }

        return ordered.Count == 0 ? 0 : total / ordered.Count;
    }

    private static TimeSpan ResolveStep(IReadOnlyList<IntelligenceSeriesPoint> ordered)
    {
        if (ordered.Count < 2)
        {
            return TimeSpan.FromDays(1);
        }

        var intervals = ordered
            .Zip(ordered.Skip(1), (left, right) => right.Timestamp - left.Timestamp)
            .Where(x => x.Ticks > 0)
            .OrderBy(x => x.Ticks)
            .ToArray();

        if (intervals.Length == 0)
        {
            return TimeSpan.FromDays(1);
        }

        return intervals[intervals.Length / 2];
    }
}
