using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using UmbracoAuthTokens.Data;

namespace UmbracoAuthTokens.TestApi
{
    [PluginController("Test")]
    public class PublicApiController : UmbracoApiController
    {
        /// <summary>
        /// A simple API call to check if our API exists & is installed
        /// http://localhost:49683/umbraco/Test/PublicApi/Ping
        /// </summary>
        [HttpGet]
        public string Ping()
        {
            return "pong";
        }

        /// <summary>
        /// http://localhost:49683/umbraco/Test/PublicApi/Auth
        /// </summary>
        /// <returns>A JWT token if auth is valid</returns>
        [HttpPost]
        public string Auth(AuthCredentials auth)
        {
            //Verify user is valid credentials
            var isValidAuth = Security.ValidateBackOfficeCredentials(auth.Username, auth.Password);

            //Are credentials correct?
            if (isValidAuth)
            {
                //Get the backoffice user from username
                var user = ApplicationContext.Services.UserService.GetByUsername(auth.Username);

                //Check if we have an Auth Token for user
                var hasAuthToken = UserAuthTokenDbHelper.GetAuthToken(user.Id);

                //If the token already exists
                if (hasAuthToken != null)
                {
                    //Lets just return it in the request
                    return hasAuthToken.AuthToken;
                }

                //Else user has no token yet - so let's create one
                //Generate AuthToken DB object
                var newToken = new UmbracoAuthToken();
                newToken.UserId = user.Id;
                newToken.UserName = user.Username;
                newToken.UserType = user.UserType.Alias;

                //Generate a new token for the user
                var authToken = UmbracoAuthTokenFactory.GenerateUserAuthToken(newToken);

                //We insert authToken as opposed to newToken
                //As authToken now has DateTime & JWT token string on it now

                //Store in DB (inserts or updates existing)
                UserAuthTokenDbHelper.InsertAuthToken(authToken);

                //Return the JWT token as the response
                //This means valid login & client in our case mobile app stores token in local storage
                return authToken.AuthToken;
            }

            //Throw unauthorised HTTP error
            var httpUnauthorised = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            throw new HttpResponseException(httpUnauthorised);
        }


        /// <summary>
        /// Simple model of usernmae & password we post to our controller
        /// </summary>
        public class AuthCredentials
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }
    }
}
