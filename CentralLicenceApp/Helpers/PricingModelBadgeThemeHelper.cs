using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CentralLicenceApp.Helpers
{
    public sealed class PricingModelBadgeTheme
    {
        public string BackgroundColor { get; init; } = "#eef2ff";
        public string TextColor { get; init; } = "#3730a3";
        public string BorderColor { get; init; } = "#c7d2fe";
    }

    public static class PricingModelBadgeThemeHelper
    {
        private static readonly IReadOnlyList<PricingModelBadgeTheme> FallbackThemes =
            new List<PricingModelBadgeTheme>
            {
                new() { BackgroundColor = "#e0f2fe", TextColor = "#075985", BorderColor = "#7dd3fc" },
                new() { BackgroundColor = "#ede9fe", TextColor = "#6d28d9", BorderColor = "#c4b5fd" },
                new() { BackgroundColor = "#dcfce7", TextColor = "#166534", BorderColor = "#86efac" },
                new() { BackgroundColor = "#fef3c7", TextColor = "#92400e", BorderColor = "#fcd34d" },
                new() { BackgroundColor = "#ffe4e6", TextColor = "#be123c", BorderColor = "#fda4af" },
                new() { BackgroundColor = "#cffafe", TextColor = "#155e75", BorderColor = "#67e8f9" },
                new() { BackgroundColor = "#ecfccb", TextColor = "#3f6212", BorderColor = "#bef264" },
                new() { BackgroundColor = "#fce7f3", TextColor = "#9d174d", BorderColor = "#f9a8d4" }
            };

        public static PricingModelBadgeTheme Resolve(string? pricingModelName)
        {
            var normalized = (pricingModelName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return FallbackThemes[0];
            }

            var lower = normalized.ToLowerInvariant();

            if (lower.Contains("silver"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#f3f6fb",
                    TextColor = "#667085",
                    BorderColor = "#cbd5e1"
                };
            }

            if (lower.Contains("gold"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#fff7d6",
                    TextColor = "#a16207",
                    BorderColor = "#f5d06f"
                };
            }

            if (lower.Contains("platinum"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#eef2ff",
                    TextColor = "#4f46e5",
                    BorderColor = "#c7d2fe"
                };
            }

            if (lower.Contains("premium"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#fce7f3",
                    TextColor = "#9d174d",
                    BorderColor = "#f9a8d4"
                };
            }

            if (lower.Contains("basic") || lower.Contains("starter"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#ecfeff",
                    TextColor = "#155e75",
                    BorderColor = "#a5f3fc"
                };
            }

            if (lower.Contains("enterprise") || lower.Contains("elite"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#ede9fe",
                    TextColor = "#5b21b6",
                    BorderColor = "#c4b5fd"
                };
            }

            if (lower.Contains("standard"))
            {
                return new PricingModelBadgeTheme
                {
                    BackgroundColor = "#e0f2fe",
                    TextColor = "#0369a1",
                    BorderColor = "#7dd3fc"
                };
            }

            return FallbackThemes[GetStableIndex(normalized, FallbackThemes.Count)];
        }

        private static int GetStableIndex(string value, int count)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            var hash = BitConverter.ToUInt32(bytes, 0);
            return (int)(hash % count);
        }
    }
}