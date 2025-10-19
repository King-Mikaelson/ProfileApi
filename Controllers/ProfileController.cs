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

        [HttpGet("/me")]
        [Produces("application/json")]
        public async Task<IActionResult> GetMe()
        {
            _logger.LogInformation("GET /me called at {TimeUtc}", DateTime.UtcNow);

            var (success, fact) = await _catFactService.GetRandomFactAsync();

            var response = new
            {
                status = success ? "success" : "error",
                user = new
                {
                    email = _profileOptions.Email,
                    name = _profileOptions.FullName,
                    stack = _profileOptions.Stack
                },
                timestamp = DateTime.UtcNow.ToString("o"),
                fact
            };

            return success
                ? Ok(response)
                : StatusCode(StatusCodes.Status502BadGateway, response);
        }
    }
}
