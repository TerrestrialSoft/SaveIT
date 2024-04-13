using FluentResults;
using SaveIt.App.Domain.Auth;
using System.Diagnostics;

namespace SaveIt.App.UI.Service.Local;
public class ProcessService : IProcessService
{
    public Result StartProcess(string path)
    {
		try
		{
			Process.Start(path);
		}
		catch (Exception)
		{
			return Result.Fail("Unable to start process");
        }

        return Result.Ok();
    }
}
