using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using Microsoft.UI.Xaml;
using Notepad.AuthService;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.Http;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;
using System.Text.Json;
using Notepad.View;

namespace Notepad
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : Window
    {
        private UserCredential credential;
        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "client_secrets.json");
        private readonly string appId;
        private readonly string scope = "openid, public_profile";
        private const string URL = "https://notepad.kevindang12.com";
        private UserInfo userInfo;
        private static readonly HttpClient client = new();
        private bool serverStatus = false;

        /// <summary>
        /// Get the App ID and configurations for the Login Window
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            Title = "Notepad";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(LoginTitleBar);
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app_settings.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<JsonDocument>(json);

                    appId = config.RootElement.GetProperty("AppId").GetString();
                    Debug.WriteLine(appId);
                }
                else
                {
                    Debug.WriteLine("app_settings.json file not found in the Resources folder.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading appsettings.json: {ex.Message}");
            }
        }


        /// <summary>
        /// Authenticate the Google user and get the values for the user's ID,
        /// first name and last name to the UserInfo object
        /// </summary>
        /// <returns>The user info containing the ID, first name, and last name</returns>
        public UserInfo? AuthenticateUser()
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { "https://www.googleapis.com/auth/userinfo.profile" },
                        "user",
                        CancellationToken.None
                    ).Result;
                }

                var peopleService = new PeopleServiceService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Notepad"
                });

                var getRequest = peopleService.People.Get("people/me");
                getRequest.PersonFields = "names";

                var person = getRequest.Execute();

                var resourceNameParts = person.ResourceName?.Split('/');
                string? userId = resourceNameParts?.LastOrDefault();

                UserInfo userInfo = new()
                {
                    Id = userId,
                    FirstName = person.Names?.FirstOrDefault()?.GivenName,
                    LastName = person.Names?.FirstOrDefault()?.FamilyName
                };

                return userInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Request Error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Check the status of the Backend server
        /// </summary>
        /// <returns>The server status</returns>
        private async Task GetStatus()
        {
            try
            {
                string url = $"{URL}/api/status";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    serverStatus = true;
                }
                else
                {
                    serverStatus = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                serverStatus = false;
            }
        }

        /// <summary>
        /// Button Listener to login to a Google account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Google_Login(object sender, RoutedEventArgs e)
        {
            await GetStatus();

            if (serverStatus)
            {
                var requrl = TokenService.GetRequestURL();
                if (string.IsNullOrEmpty(requrl))
                {
                    return;
                }

                userInfo = AuthenticateUser();

                if (userInfo != null)
                {
                    NotepadWindow notepadWindow = new(userInfo, "Google");

                    notepadWindow.Activate();

                    this.Close();
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Warning",
                    Content = "Unable to sign in with Google. Please try again later.",
                    CloseButtonText = "OK",
                    XamlRoot = GoogleLogin.XamlRoot
            };
                _ = await dialog.ShowAsync();
            }
        }
        
        /// <summary>
        /// Button Listener to login to a Facebook account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Facebook_Login(object sender, RoutedEventArgs e)
        {
            await GetStatus();

            if (serverStatus)
            {
                string appId = this.appId;
                string requestedScopes = scope;

                FBDialog fbDialog = new FBDialog(appId, requestedScopes);
                fbDialog.Activate();

                fbDialog.AuthenticationCompleted += (userInfo) =>
                {
                    NotepadWindow notepadWindow = new(userInfo, "Facebook");
                    notepadWindow.Activate();

                    Close();
                };

                fbDialog.Activate();
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Warning",
                    Content = "Unable to sign in with Facebook. Please try again later.",
                    CloseButtonText = "OK",
                    XamlRoot = FacebookLogin.XamlRoot
                };
                _ = await dialog.ShowAsync();
            }
        }

        private void FacebookLogin_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            FacebookLogin.Background = new SolidColorBrush(Colors.LightBlue);
        }

        private void FacebookLogin_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Color facebookBlue = Color.FromArgb(255, 59, 89, 153);
            FacebookLogin.Background = new SolidColorBrush(facebookBlue);
        }

        private void GoogleLogin_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            GoogleLogin.Background = new SolidColorBrush(Colors.LightBlue);
        }

        private void GoogleLogin_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            GoogleLogin.Background = new SolidColorBrush(Colors.White);
        }
    }
}
