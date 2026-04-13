using System.Globalization;
using App.Application.Features.Satellites.DTOs;

namespace App.Application.Features.Satellites;

/// <summary>
/// Parses raw TLE Line 1 and Line 2 strings into a <see cref="CelesTrakTleDto"/>.
/// Used by both the CelesTrak fetch service and manual TLE entry.
/// </summary>
public static class TleParser
{
    /// <summary>
    /// Parses two TLE lines. Returns null if the lines are not valid TLE format.
    /// </summary>
    public static CelesTrakTleDto? Parse(string line1, string line2, string name = "")
    {
        if (string.IsNullOrWhiteSpace(line1) || string.IsNullOrWhiteSpace(line2))
            return null;

        line1 = line1.Trim();
        line2 = line2.Trim();

        if (!line1.StartsWith("1 ") || !line2.StartsWith("2 "))
            return null;

        if (line1.Length < 69 || line2.Length < 69)
            return null;

        try
        {
            return new CelesTrakTleDto
            {
                Name              = name,
                NoradId           = int.Parse(line1.Substring(2, 5).Trim()),
                Line1             = line1,
                Line2             = line2,
                Epoch             = ParseEpoch(line1),
                FirstDerivative   = ParseDouble(line1.Substring(33, 10).Trim()),
                SecondDerivative  = ParseScientificNotation(line1.Substring(44, 8).Trim()),
                BSTar             = ParseScientificNotation(line1.Substring(53, 8).Trim()),
                Inclination       = ParseDouble(line2.Substring(8, 8).Trim()),
                RAAN              = ParseDouble(line2.Substring(17, 8).Trim()),
                Eccentricity      = ParseDouble("0." + line2.Substring(26, 7).Trim()),
                ArgumentOfPerigee = ParseDouble(line2.Substring(34, 8).Trim()),
                MeanAnomaly       = ParseDouble(line2.Substring(43, 8).Trim()),
                MeanMotion        = ParseDouble(line2.Substring(52, 11).Trim()),
                RevolutionNumber  = int.Parse(line2.Substring(63, 5).Trim()),
            };
        }
        catch
        {
            return null;
        }
    }

    // Epoch: 2-digit year + day-of-year.fraction (e.g. "26097.50000000")
    private static DateTime ParseEpoch(string line1)
    {
        var raw    = line1.Substring(18, 14).Trim();
        var year2d = int.Parse(raw.Substring(0, 2));
        var year   = year2d >= 57 ? 1900 + year2d : 2000 + year2d;
        var dayFrac = double.Parse(raw.Substring(2), CultureInfo.InvariantCulture);
        return new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(dayFrac - 1);
    }

    private static double ParseDouble(string s)
        => double.Parse(s, CultureInfo.InvariantCulture);

    // TLE coefficient notation: "NNNNN±N" → 0.NNNNN × 10^±N
    private static double ParseScientificNotation(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0.0;

        double sign = 1.0;
        if (s[0] == '-') { sign = -1.0; s = s[1..]; }
        else if (s[0] == '+') { s = s[1..]; }

        var expSignIdx = -1;
        for (var i = s.Length - 2; i >= 1; i--)
        {
            if (s[i] != '+' && s[i] != '-') continue;
            expSignIdx = i;
            break;
        }

        if (expSignIdx < 0) return 0.0;

        var mantissa = double.Parse("0." + s[..expSignIdx], CultureInfo.InvariantCulture) * sign;
        var exponent = int.Parse(s[expSignIdx..], CultureInfo.InvariantCulture);
        return mantissa * Math.Pow(10, exponent);
    }
}
