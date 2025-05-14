using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Notepad.AuthService;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Windows.UI;
using Microsoft.UI.Xaml.Media.Animation;
using Notepad.View;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notepad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private UserCredential credential;
        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "client_secrets.json");
        private readonly string appId;
        private readonly string scope = "openid, public_profile";
        private const string URL = "http://notepad.kevindang12.com";
        private UserInfo userInfo;
        private static readonly HttpClient client = new();
        private bool serverStatus = false;

        /// <summary>
        /// Get the App ID and configurations for the Login Window
        /// </summary>
        public LoginPage()
        {
            InitializeComponent();
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
                    object[] parameters = new object[] { userInfo, "Google" };
                    Frame.Navigate(typeof(NotepadPage), parameters, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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
