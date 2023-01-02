using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace ServerlessFuncs.User
{
    public class UserProfileFetcher
    {

        public async Task<UserProfileNew> FetchUserProfile(string userObjectID)
        {
            var profile = new UserProfileNew();
            profile.UserName = await FetchUserName(userObjectID);
            return profile;
        }
        public async Task<string> FetchUserName(string userObjectID)
        {

            IConfidentialClientApplication clientApp = GetClientApp();

            GraphServiceClient graphServiceClient = GetGraphServiceClient(clientApp);

            Microsoft.Graph.User user = await graphServiceClient.Users[userObjectID]
                .Request()
                .Select("identities")
                .GetAsync();


            if (user.Identities != null)
            {
                foreach (var identity in user.Identities)
                {
                    if (identity.SignInType == "userName")
                    {
                        return identity.IssuerAssignedId;
                    }
                }
            }

            return null;

        }




        private IConfidentialClientApplication GetClientApp()
        {
            string CLIENT_ID = Environment.GetEnvironmentVariable("SpaAppClientID");
            string TENANT_ID = Environment.GetEnvironmentVariable("SpaAppTenantID");
            string CLIENT_SECRET = Environment.GetEnvironmentVariable("SpaAppClientSecret");

            Trace.WriteLine($"Client secret is: {CLIENT_SECRET}");

            var clientApp = ConfidentialClientApplicationBuilder
                .Create(CLIENT_ID)
                .WithTenantId(TENANT_ID)
                .WithClientSecret(CLIENT_SECRET)
                .Build();

            return clientApp;
        }


        private GraphServiceClient GetGraphServiceClient(IConfidentialClientApplication clientApp)
        {
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            GraphServiceClient graphServiceClient =
               new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {
                   // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                   var authResult = await clientApp
                       .AcquireTokenForClient(scopes)
                       .ExecuteAsync();

                   // Add the access token in the Authorization header of the API request.
                   requestMessage.Headers.Authorization
                   = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
               }));

            return graphServiceClient;
        }
    }
}

