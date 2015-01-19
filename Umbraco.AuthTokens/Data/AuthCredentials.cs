namespace UmbracoAuthTokens.Data
{
    /// <summary>
    /// Simple model of the Umbraco backoffice Username & Password 
    /// we HTTP POST to our controller method 'Authorise'
    /// </summary>
    public class AuthCredentials
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
