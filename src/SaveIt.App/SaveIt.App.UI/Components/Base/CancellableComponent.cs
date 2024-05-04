using Microsoft.AspNetCore.Components;

namespace SaveIt.App.UI.Components.Base;
public class CancellableComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource? _tokenSource;

    protected CancellationToken CancellationToken
    {
        get
        {
            _tokenSource ??= new();
            return _tokenSource.Token;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_tokenSource is not null)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }
    }

    ~CancellableComponent() 
    {
        Dispose(false);
    }

    protected void ResetToken()
    {
        if (_tokenSource is not null)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new();
        }
    }
}
