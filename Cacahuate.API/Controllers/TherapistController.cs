using Cacahuate.DataAccess.Repositories;
using Cacahuate.Shared.DTOs.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cacahuate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TherapistController(ITherapistRepository therapistRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TherapistListResponse>>> GetAll()
    {
        var therapists = await therapistRepository.GetAllActiveAsync();
        var result = therapists.Select(t => new TherapistListResponse
        {
            Id = t.Id,
            FullName = $"{t.User.FirstName} {t.User.LastName}",
            Bio = t.Bio,
            SessionDurationMinutes = t.SessionDurationMinutes
        }).ToList();

        return Ok(result);
    }
}
