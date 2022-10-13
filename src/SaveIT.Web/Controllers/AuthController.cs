using Microsoft.AspNetCore.Mvc;
using SaveIT.Api.Services;
using SaveIT.Web.Models;
using System.Diagnostics;

namespace SaveIT.Web.Controllers;

public class AuthController : Controller
{
	private readonly IOAuthService _oAuthService;
	private readonly IConfiguration _configuration;

	public AuthController(IOAuthService oAuthService, IConfiguration configuration)
	{
		_oAuthService = oAuthService;
		_configuration = configuration;
	}

	public IActionResult AuthorizeAccount(string code)
	{
		if (code is null)
		{
			// Handle Error
			return View();
		}

		return RedirectPermanent("https://accounts.google.com/o/oauth2/v2/auth?" +
			"scope=https://www.googleapis.com/auth/drive&" +
			"access_type=offline&" +
			"include_granted_scopes=true&" +
			"response_type=code&" +
			$"state={code}&" +
			"redirect_uri=https://localhost:44307/Auth/authorized&" +
			$"client_id={_configuration["GoogleDriveOauth:ClientId"]}");
	}

	public async Task<IActionResult> Authorized(string error, string state, string code)
	{
		await _oAuthService.StoreTokenAsync(state, code);

		return View();
	}

	[HttpGet("[Controller]/token")]
	public ActionResult<OAuthToken?> GetToken(string code)
	{
		var token = _oAuthService.GetToken(code);

		if (token is null)
			return NotFound();

		return Json(token);
	}

	[HttpGet("[Controller]/token/refresh")]
	public async Task<ActionResult<OAuthToken?>> RefreshToken(string refreshToken)
	{
		var token = await _oAuthService.RefreshTokenAsync(refreshToken);
		return Json(token);
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
