using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SaveIt.Server.UI.Api;

public record RequestModel([BindRequired]Guid RequestId);
