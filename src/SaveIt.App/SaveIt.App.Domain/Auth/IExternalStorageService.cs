namespace SaveIt.App.Domain.Auth;
public interface IExternalStorageService
{
    Task<string> GetProfileEmailAsync(string accessToken);
}
