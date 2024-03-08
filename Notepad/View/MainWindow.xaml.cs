using Microsoft.UI.Xaml;
using System;
using Google.Apis.Auth.OAuth2;
using System.Diagnostics;
using System.IO;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notepad
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "client_secrets.json");

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(NotepadTitleBar);

            this.Closed += Window_Closed;
        }

        /// <summary>
        /// When closing the window, remove the Google Token
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, WindowEventArgs e)
        {
            if (NotepadPage.authPath == "Google")
            {
                RemoveToken();
            }
        }

        /// <summary>
        /// Remove the token for the Google user
        /// </summary>
        private void RemoveToken()
        {
            try
            {
                UserCredential credential;

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { "https://www.googleapis.com/auth/userinfo.profile" },
                        "user",
                        CancellationToken.None
                    ).Result;
                }

                credential.RevokeTokenAsync(CancellationToken.None).Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error revoking token: {ex.Message}");
            }
        }
    }
}
