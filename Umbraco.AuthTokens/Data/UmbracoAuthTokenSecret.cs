using System;

namespace Umbraco.AuthTokens.Data
{
    public class UmbracoAuthTokenSecret
    {
        private const string SecretEnvVariable = "Umbraco.AuthToken";

        /// <summary>
        /// This sets the secret as an Environment Variable
        /// </summary>
        /// <param name="secret">Secret string to set</param>
        public void SetSecret(string secret)
        {
            Environment.SetEnvironmentVariable(SecretEnvVariable, secret, EnvironmentVariableTarget.Machine);
        }

        /// <summary>
        /// Goes & fetchs the secret from the Machine Environment Variables
        /// </summary>
        /// <returns>Returns the string secret</returns>
        public string GetSecret()
        {
            return Environment.GetEnvironmentVariable(SecretEnvVariable, EnvironmentVariableTarget.Machine);
        }
    }
}
