using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Notepad.View;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notepad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FBDialogPage : Page
    {
        private string p_appID;
        private string p_scopes;
        public UserInfo userInfo = new UserInfo();
        public event Action<UserInfo> AuthenticationCompleted;

        /// <summary>
        /// Facebook Dialog box to use the App ID and scope to 
        /// get the Facebook user authentication and information
        /// </summary>
        public FBDialogPage()
        {
            InitializeComponent();

            // Subscribe to the Navigated event in the constructor
            FBwebBrowser.NavigationCompleted += FBwebBrowser_NavigationCompleted;

            // Load Facebook login page
            //LoadFacebookLoginPage();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check if parameters were passed
            if (e.Parameter != null && e.Parameter is object[] parameters && parameters.Length == 2)
            {
                // Retrieve the parameters
                p_appID = (string) parameters[0];
                p_scopes = (string) parameters[1];

                LoadFacebookLoginPage();
            }
        }

        /// <summary>
        /// Once the Facebook web browser has been navigated and the 
        /// Facebook user was able to sign in, authenticate the user 
        /// and extract the user info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FBwebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (FBwebBrowser.Source.AbsolutePath == "/connect/login_success.html")
            {
                if (FBwebBrowser.Source.Query.Contains("error"))
                {
                    HandleError(WebUtility.UrlDecode(FBwebBrowser.Source.Query));
                }
                else
                {
                    await HandleAuthenticationSuccess(FBwebBrowser.Source.Fragment);
                    object[] parameters = new object[] { userInfo, "Facebook" };
                    Frame.Navigate(typeof(NotepadPage), parameters, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                FBwebBrowser.CoreWebView2.CookieManager.DeleteAllCookies();
            }
        }

        /// <summary>
        /// Open up the Facebook page with the Facebook login URL
        /// </summary>
        private void LoadFacebookLoginPage()
        {
            string redirectUri = "https://www.facebook.com/connect/login_success.html";
            string returnUrl = WebUtility.UrlEncode(redirectUri);
            string scopes = WebUtility.UrlEncode(p_scopes);

            string facebookLoginUrl = $"https://www.facebook.com/dialog/oauth?client_id={p_appID}&redirect_uri={returnUrl}&response_type=token%2Cgranted_scopes&scope={scopes}&display=popup";

            FBwebBrowser.Source = new Uri(facebookLoginUrl);
        }

        /// <summary>
        /// Show an error message if the Facebook authentication fails
        /// </summary>
        /// <param name="errorQueryString">Contains the error message of the query</param>
        private void HandleError(string errorQueryString)
        {
            // Handle the error (for example, display an error message)
            Debug.WriteLine($"Error during Facebook authentication: {errorQueryString}");
        }

        /// <summary>
        /// Extract the user info from the source query
        /// </summary>
        /// <param name="sourceQuery">Contains the query result after authenticating the user</param>
        private async Task HandleAuthenticationSuccess(string sourceQuery)
        {
            string[] parameters = sourceQuery.TrimStart('#').Split('&');

            foreach (var parameter in parameters)
            {
                string[] keyValue = parameter.Split('=');

                switch (keyValue[0])
                {
                    case "access_token":
                        string accessToken = WebUtility.UrlDecode(keyValue[1]);

                        FacebookApiClient facebookApiClient = new FacebookApiClient();
                        FacebookUserInfo facebookUserInfo = await facebookApiClient.GetUserInformationAsync(accessToken);

                        if (facebookUserInfo != null)
                        {
                            userInfo.Id = facebookUserInfo.Id;
                            userInfo.FirstName = facebookUserInfo.First_Name;
                            userInfo.LastName = facebookUserInfo.Last_Name;

                            //AuthenticationCompleted?.Invoke(userInfo);
                        }
                        break;

                    case "expires_in":
                        if (int.TryParse(keyValue[1], out int expiresIn))
                        {
                            DateTime expirationTime = DateTime.Now.AddSeconds(expiresIn);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
