using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SaveIt.Server.UI.Models;

public record RefreshTokenRequestModel([BindRequired]string RefreshToken);
