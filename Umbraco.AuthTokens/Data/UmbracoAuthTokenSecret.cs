using System;
using System.Web.Security;

namespace Umbraco.AuthTokens.Data
{
    public static class UmbracoAuthTokenSecret
    {
        private const string SecretEnvVariable = "Umbraco.AuthToken";

        //TODO: Check EnvVariable works well with Azure Websites (No access to server itself)
        //Same with Umbraco.io/Umbraco.com perhaps? If they have a UI to set, add & edit them?

        /// <summary>
        /// This sets the secret as an Environment Variable
        /// </summary>
        /// <param name="secret">Secret string to set</param>
        public static void SetSecret(string secret)
        {
            Environment.SetEnvironmentVariable(SecretEnvVariable, secret, EnvironmentVariableTarget.Machine);
        }

        /// <summary>
        /// Goes & fetchs the secret from the Machine Environment Variables
        /// </summary>
        /// <returns>Returns the string secret</returns>
        public static string GetSecret()
        {
            var secret = Environment.GetEnvironmentVariable(SecretEnvVariable, EnvironmentVariableTarget.Machine);

            //If it does not exist or is null/empty then we set a new one
            if (string.IsNullOrEmpty(secret))
            {
                //Lets create a random strong password & set env variable
                secret =  Membership.GeneratePassword(50, 5);

                //Set it as the Env Var
                SetSecret(secret);
            }

            //Return the secret
            return secret;
        }
    }
}
