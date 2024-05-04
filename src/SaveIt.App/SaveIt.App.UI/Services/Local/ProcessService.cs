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
			ProcessStartInfo startInfo = new()
			{
                FileName = path,
                UseShellExecute = true
            };
			Process.Start(startInfo);
		}
		catch (Exception)
		{
			return Result.Fail("Unable to start process");
        }

        return Result.Ok();
    }
}
