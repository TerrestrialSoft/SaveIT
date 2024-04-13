using FluentResults;

namespace SaveIt.App.Domain.Auth;
public interface IProcessService
{
    Task<Result> StartProcess(string path);
}
