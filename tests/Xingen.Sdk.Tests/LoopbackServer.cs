using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xingen.Sdk.Tests;

/// <summary>
/// Minimal loopback HTTP server for integration tests, so the real <see cref="System.Net.Http.HttpClient"/>
/// network stack is under test rather than a mocked handler.
/// </summary>
internal sealed class LoopbackServer : IAsyncDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _handlers = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _acceptLoop;

    public int Port { get; }
    public Uri BaseUrl => new($"http://localhost:{Port}");

    public LoopbackServer()
    {
        Port = GetFreePort();
        _listener.Prefixes.Add($"http://localhost:{Port}/");
    }

    public void MapHandler(string pathPrefix, Func<HttpListenerContext, Task> handler) => _handlers[pathPrefix] = handler;

    public LoopbackServer Start()
    {
        _listener.Start();
        _acceptLoop = AcceptLoopAsync(_cts.Token);
        return this;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            _ = HandleAsync(context);
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        var path = context.Request.Url!.AbsolutePath;
        var match = _handlers
            .OrderByDescending(kv => kv.Key.Length)
            .FirstOrDefault(kv => path == kv.Key || path.StartsWith(kv.Key, StringComparison.Ordinal));

        if (match.Value is null)
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
            return;
        }
        await match.Value(context).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _listener.Stop();
        _listener.Close();
        if (_acceptLoop is not null)
        {
            try { await _acceptLoop; } catch { /* server loop torn down */ }
        }
    }

    private static int GetFreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}

internal static class LoopbackServerExtensions
{
    public static async Task RespondAsync(
        this HttpListenerContext context, int status, string body, string contentType = "application/json")
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Response.StatusCode = status;
        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
        context.Response.OutputStream.Close();
    }

    public static async Task RespondAsync(this HttpListenerContext context, int status, byte[] body, string contentType)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = body.Length;
        await context.Response.OutputStream.WriteAsync(body).ConfigureAwait(false);
        context.Response.OutputStream.Close();
    }

    public static async Task<string> ReadBodyAsync(this HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
