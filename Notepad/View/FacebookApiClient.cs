using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Notepad.View
{
    /// <summary>
    /// Allows access to the Facebook API to retrieve user data
    /// </summary>
    public class FacebookApiClient
    {
        private const string GraphApiBaseUrl = "https://graph.facebook.com/v12.0/";

        /// <summary>
        /// Make an API request to get the user data from Facebook such as the ID, first name, and last name
        /// </summary>
        /// <param name="accessToken">Get the user info using the access token</param>
        /// <returns>The Facebook User info object containing the ID, first name and last name</returns>
        public async Task<FacebookUserInfo> GetUserInformationAsync(string accessToken)
        {
            try
            {
                // Make a request to the Facebook Graph API to get user information
                using (HttpClient client = new HttpClient())
                {
                    string fields = "id,first_name,last_name";

                    string requestUrl = $"{GraphApiBaseUrl}me?fields={fields}&access_token={accessToken}";

                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        string content = await response.Content.ReadAsStringAsync();

                        // Deserialize the JSON string to UserInfo object
                        FacebookUserInfo userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return userInfo;
                    }
                    else
                    {
                        Debug.WriteLine($"Error retrieving user information: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }

            return null;
        }
    }
}
