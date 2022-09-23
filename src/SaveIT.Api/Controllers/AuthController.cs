using Microsoft.AspNetCore.Mvc;
using SaveIT.Api.Models;
using SaveIT.Api.Services;

namespace SaveIT.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IOAuthService _oauthService;

	public AuthController(IOAuthService oauthService)
	{
		_oauthService = oauthService;
	}

	[HttpGet]
	public async Task<OAuthToken> GetToken()
	{
		return await _oauthService.GetTokenAsync();
	}
}
