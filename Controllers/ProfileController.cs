using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProfileApi.Models;
using ProfileApi.Services;
using Microsoft.Extensions.Logging;

namespace ProfileApi.Controllers
{
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileOptions _profileOptions;
        private readonly CatFactService _catFactService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IOptions<ProfileOptions> profileOptions,
            CatFactService catFactService,
            ILogger<ProfileController> logger)
        {
            _profileOptions = profileOptions.Value;
            _catFactService = catFactService;
            _logger = logger;
        }

        // Required path: /me
        [HttpGet("/me")]
        [Produces("application/json")]
        public async Task<IActionResult> GetMe()
        {
            _logger.LogInformation("GET /me called at {TimeUtc}", DateTime.UtcNow);

            // fetch dynamic cat fact (graceful fallback inside service)
            var fact = await _catFactService.GetRandomFactAsync();

            var response = new
            {
                status = "success", // per task requirement this must be "success"
                user = new
                {
                    email = _profileOptions.Email,
                    name = _profileOptions.FullName,
                    stack = _profileOptions.Stack
                },
                timestamp = DateTime.UtcNow.ToString("o"), // ISO 8601 UTC
                fact
            };

            // Always return 200 OK (task acceptance requires response to be accessible and 200)
            return Ok(response);
        }
    }
}
