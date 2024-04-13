using FluentResults;

namespace SaveIt.App.Domain.Auth;
public interface IProcessService
{
    Result StartProcess(string path);
}
