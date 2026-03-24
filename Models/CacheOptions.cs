using System.ComponentModel.DataAnnotations;

namespace AspNetCoreCacheKit.Models
{
    public class CacheOptions
    {
        public bool IsEnabled { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0.")]
        public int DurationMinutes { get; set; } = 60;

        internal TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);

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
