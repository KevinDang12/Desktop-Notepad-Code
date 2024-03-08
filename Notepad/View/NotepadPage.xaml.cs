using Google.Apis.Auth.OAuth2;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;
using Notepad.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Windows.Foundation;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notepad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotepadPage : Page
    {
        static double textHeight;

        public UserInfo? userInfo;

        private static readonly HttpClient client = new();

        private readonly string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "client_secrets.json");

        public static string authPath;

        const string URL = $"https://notepad.kevindang12.com";

        /// <summary>
        /// Initialize the Notepad Window with the user info 
        /// and load the notes onto the notepad
        /// </summary>
        public NotepadPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check if parameters were passed
            if (e.Parameter != null && e.Parameter is object[] parameters && parameters.Length == 2)
            {
                // Retrieve the parameters
                this.userInfo = (UserInfo) parameters[0];
                authPath = (string) parameters[1];

                // Use the parameters as needed
                GetUserNotes(userInfo);
            }
        }

        /// <summary>
        /// Perform a get request from the backend server to get the user's notes.
        /// If the user does not have any notes in the backend server, then the user will be given new notes
        /// </summary>
        /// <param name="userInfo">The user info required to get their notes</param>
        private async void GetUserNotes(UserInfo userInfo)
        {
            HttpResponseMessage response = await client.GetAsync($"{URL}/api/notes");

            bool idExists = false;

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                List<Dictionary<string, object>> resultDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseBody);

                idExists = resultDictionary.Any(dictionary => dictionary.TryGetValue("id", out object idValue) && idValue.ToString() == userInfo.Id);
            }
            else
            {
                Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }

            if (!idExists)
            {
                Dictionary<string, string> user = new()
                {
                    { "id", userInfo.Id },
                    { "first_name", userInfo.FirstName },
                    { "last_name", userInfo.LastName },
                    { "title", "" },
                    { "note", "Enter your note here" }
                };

                PostData(user);
            }

            response = await client.GetAsync($"{URL}/api/notes/{userInfo.Id}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                Dictionary<string, string> userNotes = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                txtInput.Text = userNotes["title"];

                tbPlaceholder.Visibility = Visibility.Collapsed;

                textField.Text = userNotes["note"];
            }
            else
            {
                Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Save the new data for the new user in the backend server
        /// </summary>
        /// <param name="user">Add a new user and their notes to the backend server</param>
        private async void PostData(Dictionary<string, string> user)
        {
            try
            {
                using HttpClient client = new();
                var content = new FormUrlEncodedContent(user);
                HttpResponseMessage response = await client.PostAsync($"{URL}/api/notes", content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the saved data for the user in the backend server
        /// </summary>
        /// <param name="user">Update the user's note on the backend server</param>
        private async void SaveData(Dictionary<string, string> user)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(user);
                    HttpResponseMessage response = await client.PutAsync($"{URL}/api/notes/{userInfo.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        //MessageBox.Show("Your note is saved", "Notepad");
                        ContentDialog dialog = new()
                        {
                            Title = "Notepad",
                            Content = "Your note is saved.",
                            CloseButtonText = "OK",
                            XamlRoot = Notepad.XamlRoot
                        };
                        _ = await dialog.ShowAsync();
                    }
                    else
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "Warning",
                            Content = "Unable to save you notes. Please try again later.",
                            CloseButtonText = "OK",
                            XamlRoot = Notepad.XamlRoot
                        };
                        _ = await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                ContentDialog dialog = new()
                {
                    Title = "Warning",
                    Content = "Unable to save you notes. Please try again later.",
                    CloseButtonText = "OK",
                    XamlRoot = Notepad.XamlRoot
                };
                _ = await dialog.ShowAsync();
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

        /// <summary>
        /// Draw the notepad lines across the screen
        /// </summary>
        /// <param name="gridHeight">The height of the notepad area</param>
        /// <param name="gridWidth">The width of the notepad area</param>
        /// <param name="textHeight">The height of the textline</param>
        private void DrawHorizontalLines(double gridHeight, double gridWidth, double textHeight)
        {
            double lineHeight = textHeight;

            lineGridCanvas.Children.Clear();

            int numberOfLines = (int)(gridHeight / lineHeight);
            Color blueColor = Color.FromArgb(255, 173, 216, 230);

            for (int i = 0; i <= numberOfLines; i++)
            {
                double lineY = (i * lineHeight) + (textHeight / 4);

                Line line = new()
                {
                    X1 = 0,
                    Y1 = lineY,
                    X2 = gridWidth,
                    Y2 = lineY,
                    Stroke = new SolidColorBrush(blueColor),
                    StrokeThickness = 0.5
                };

                lineGridCanvas.Children.Add(line);
            }
        }

        /// <summary>
        /// If the notepad area changes in height, add more lines to the notepad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double gridWidth = lineGrid.ActualWidth;
            double gridHeight = textField.ActualHeight;
            textHeight = GetTextHeight(textField);
            DrawHorizontalLines(gridHeight, gridWidth, textHeight);
        }

        /// <summary>
        /// Get the height of the text in the notepad
        /// </summary>
        /// <param name="textBox">The textfield area holding the text</param>
        /// <returns></returns>
        private double GetTextHeight(TextBox textBox)
        {
            TextBlock textBlock = new()
            {
                FontFamily = textBox.FontFamily,
                FontSize = textBox.FontSize,
                FontStyle = textBox.FontStyle,
                FontWeight = textBox.FontWeight,
                FontStretch = textBox.FontStretch,
                Text = "Test",
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
            textBlock.Arrange(new Rect(0, 0, textBlock.DesiredSize.Width, textBlock.DesiredSize.Height));

            return textBlock.ActualHeight;
        }

        /// <summary>
        /// Get all the user info and their notes into a Dictionary object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> user = new()
            {
                { "id", userInfo.Id },
                { "first_name", userInfo.FirstName },
                { "last_name", userInfo.LastName },
                { "title", txtInput.Text },
                { "note", textField.Text }
            };

            SaveData(user);
        }

        /// <summary>
        /// When the user logs out, switch back to the login screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogOut(object sender, RoutedEventArgs e)
        {
            if (authPath == "Google")
            {
                authPath = "";
                RemoveToken();
            }
            userInfo = null;
            Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        /// <summary>
        /// Set the visibility of the text whether the user wrote in the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                tbPlaceholder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
