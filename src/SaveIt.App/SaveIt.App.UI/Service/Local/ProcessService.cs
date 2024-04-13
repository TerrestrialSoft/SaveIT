using FluentResults;
using SaveIt.App.Domain.Auth;
using System.Diagnostics;

namespace SaveIt.App.UI.Service.Local;
public class ProcessService : IProcessService
{
    public Task<Result> StartProcess(string path)
    {
		try
		{
			Process.Start(path);
		}
		catch (Exception)
		{
			return Task.FromResult(Result.Ok());

        }

        return Task.FromResult(Result.Ok());
    }
}
