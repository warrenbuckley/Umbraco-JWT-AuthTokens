﻿using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using UmbracoAuthTokens.Data;
 
namespace UmbracoAuthTokens.Controllers
{
    [PluginController("TokenAuth")]
    public class SecureApiController : UmbracoApiController
    {
        /// <summary>
        /// http://localhost:55778/umbraco/TokenAuth/SecureApi/AuthoriseUser
        /// </summary>
        /// <returns>A JWT token as a string if auth is valid</returns>
        [HttpPost]
        public string AuthoriseUser(AuthCredentials auth)
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
                newToken.IdentityId = user.Id;
                newToken.IdentityType = IdentityAuthType.User.ToString();
 
                //Generate a new token for the user
                var authToken = UmbracoAuthTokenFactory.GenerateAuthToken(newToken);
 
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
        /// http://localhost:55778/umbraco/TokenAuth/SecureApi/AuthoriseMember
        /// </summary>
        /// <returns>A JWT token as a string if auth is valid</returns>
        [HttpPost]
        public string AuthoriseMember(AuthCredentials auth)
        {
             
            //Verify user is valid credentials - using current membership provider
            //Should be native Umbraco one
            var isValidAuth = Membership.ValidateUser(auth.Username, auth.Password);
 
            //Are credentials correct?
            if (isValidAuth)
            {
                //Get the member from username
                var member = ApplicationContext.Services.MemberService.GetByUsername(auth.Username);
 
                //Check if we have an Auth Token for user
                var hasAuthToken = UserAuthTokenDbHelper.GetAuthToken(member.Id);
 
                //If the token already exists
                if (hasAuthToken != null)
                {
                    //Lets just return it in the request
                    return hasAuthToken.AuthToken;
                }
 
                //Else user has no token yet - so let's create one
                //Generate AuthToken DB object
                var newToken = new UmbracoAuthToken();
                newToken.IdentityId = member.Id;
                newToken.IdentityType = IdentityAuthType.Member.ToString();
 
                //Generate a new token for the user
                var authToken = UmbracoAuthTokenFactory.GenerateAuthToken(newToken);
 
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
    }
}