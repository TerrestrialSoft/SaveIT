using Microsoft.AspNetCore.Mvc;

namespace SaveIT.Web.Controllers;
public class ErrorController : Controller
{
	[Route("Error/{statusCode}")]
	public IActionResult HttpStatusCodeHandler(int statusCode)
	{
		ViewBag.ErrorMessage = statusCode switch
		{
			404 => "Sorry, the requested resource could not be found.",
			_ => "We are sorry but something went wrong."
		};

		return View("Error");
	}
}
