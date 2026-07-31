using System.ComponentModel.DataAnnotations;

namespace AspNetCoreCacheKit.Models
{
    /// <summary>Configuration for <see cref="AspNetCoreCacheKit.Interfaces.ICacheService"/>, bound from the "CacheOptions" configuration section.</summary>
    /// <remarks>
    /// Bound once at startup via <c>IOptions&lt;CacheOptions&gt;</c> and captured by the
    /// singleton <c>CacheService</c>. Changes to <c>appsettings.json</c> at runtime are not
    /// picked up without restarting the application.
    /// </remarks>
    public class CacheOptions
    {
        /// <summary>Enables or disables caching entirely. When <c>false</c>, factories are always invoked and nothing is stored.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Global default cache duration in minutes, used when no group duration or explicit duration applies.</summary>
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0.")]
        public int DurationMinutes { get; set; } = 60;

        internal TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);

        /// <summary>Per-group cache durations in minutes. Key = group name, value = duration in minutes.</summary>
        public Dictionary<string, int> GroupDurations { get; set; } = [];

        internal TimeSpan? GetGroupDuration(string groupKey)
        {
            if (string.IsNullOrEmpty(groupKey))
                return null;

            return GroupDurations.TryGetValue(groupKey, out var minutes)
                ? TimeSpan.FromMinutes(minutes)
                : null;
        }
    }
}
