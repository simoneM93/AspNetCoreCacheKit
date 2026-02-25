using System.ComponentModel.DataAnnotations;

namespace AspNetCoreCacheKit.Models
{
    public class CacheOptions
    {
        public bool IsEnabled { get; set; } = true;

        [Range(typeof(TimeSpan), "00:00:01", "24:00:00", ErrorMessage = "Duration must be between 1 second and 24 hours")]
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(60);
    }
}
