using System.ComponentModel.DataAnnotations;

namespace SaveIt.Server.UI.Models;

public record RefreshTokenRequestModel([Required]string RefreshToken);
