using System;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace CentralLicenceApp.Services
{
    /// <summary>
    /// Singleton that owns a single long-lived Chromium browser instance.
    /// Reusing one browser across requests eliminates the ~2-4 s cold-start
    /// cost of launching a new process for every PDF generation.
    /// </summary>
    public interface IBrowserProvider : IAsyncDisposable
    {
        Task WarmUpAsync();
        Task<IBrowser> GetBrowserAsync();
    }

    public sealed class BrowserProvider : IBrowserProvider
    {
        private IBrowser?         _browser;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly LaunchOptions LaunchOpts = new()
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
        };

        public async Task WarmUpAsync()
        {
            // Downloads Chromium if needed and launches the browser at startup
            // so the first real request is fast.
            _ = await GetBrowserAsync();
        }

        public async Task<IBrowser> GetBrowserAsync()
        {
            // Fast path — browser already running
            if (_browser is { IsClosed: false })
                return _browser;

            await _lock.WaitAsync();
            try
            {
                if (_browser is { IsClosed: false })
                    return _browser;

                var fetcher = new BrowserFetcher();
                await fetcher.DownloadAsync();

                _browser = await Puppeteer.LaunchAsync(LaunchOpts);
                return _browser;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                try { await _browser.CloseAsync(); } catch { /* best effort */ }
            }
            _lock.Dispose();
        }
    }
}
