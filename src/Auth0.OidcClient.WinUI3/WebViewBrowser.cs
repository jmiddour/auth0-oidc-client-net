using IdentityModel.OidcClient.Browser;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace Auth0.OidcClient
{
    /// <summary>
    /// Implements the <see cref="IBrowser"/> interface using the <see cref="WebView2"/> control.
    /// </summary>
    public class WebViewBrowser : IBrowser
    {
        private readonly Func<Window> _windowFactory;
        private readonly bool _shouldCloseWindow;
        private readonly Window _appWindow;
        /// <summary>
        /// Create a new instance of <see cref="WebViewBrowser"/> with a custom windowFactory and optional window close flag.
        /// </summary>
        /// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
        /// <param name="shouldCloseWindow"> Whether the Window should be closed or not after completion.</param>
        public WebViewBrowser( Window appWindow, bool shouldCloseWindow = true)
        {
          _appWindow = appWindow;
            _shouldCloseWindow = shouldCloseWindow;
        }

        /// <summary>
        /// Create a new instance of <see cref="WebViewBrowser"/> allowing parts of the <see cref="Window"/> container to be set.
        /// </summary>
        /// <param name="title">Optional title for the form - defaults to 'Authenticating...'.</param>
        /// <param name="width">Optional width for the form in pixels. Defaults to 1024.</param>
        /// <param name="height">Optional height for the form in pixels. Defaults to 768.</param>


        /// <inheritdoc />
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<BrowserResult>();
            var previousContent = _appWindow.Content;
            var frame = new Frame();
            var grid = new Grid();
            var closeButtonRow = new RowDefinition() { Height = GridLength.Auto };
            var webViewRow = new RowDefinition();

            grid.RowDefinitions.Add(closeButtonRow);
            grid.RowDefinitions.Add(webViewRow);


            var closeButton = new Button();
            closeButton.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));
            closeButton.Content = new SymbolIcon(Symbol.Cancel);
            closeButton.HorizontalAlignment = HorizontalAlignment.Right;

            closeButton.Click += (sender,e)=>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
                _appWindow.Content = previousContent;
            };
            var webView = new WebView2 { };

            webView.NavigationStarting += (sender, e) =>
            {
                if (e.Uri.StartsWith(options.EndUrl))
                {

                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = e.Uri.ToString() });
                    _appWindow.Content = previousContent;

                }

            };


            Grid.SetRow(webView, 1);
            Grid.SetRow(closeButton, 0);
            grid.Children.Add(closeButton);
            grid.Children.Add(webView);
            _appWindow.Content = grid;
            _appWindow.Activate();
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate(options.StartUrl);



            return await tcs.Task;
        }

       
    }
}