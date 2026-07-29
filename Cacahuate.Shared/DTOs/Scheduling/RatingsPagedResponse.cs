using Cacahuate.Shared.DTOs.Common;

namespace Cacahuate.Shared.DTOs.Scheduling;

public class RatingsPagedResponse : PagedResult<RatingResponse>
{
    public double? AverageStars { get; set; }
}
