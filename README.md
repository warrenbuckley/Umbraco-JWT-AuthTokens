Umbraco JWT AuthTokens
======================

This is a repository for providing a secure based API to perform backoffice actions using JWT Auth tokens. https://oromand.github.io/Umbraco-JWT-AuthTokens/

## What are JWTs?
They are an auth token that allows you to send a piece of JSON encoded as a token and are the more modern approach to deal with auth in applications 
especially as we build applications across different devices. The videos below will do a lot better trying to explain it than I can do.

## Why do this?
I needed to create this POC for an upcoming pet project I am currently hacking & building. I needed a way to authenticate to any Umbraco backoffice using 
the same credentials as the Umbraco backoffice user & ensure they have access to specific section/s on that user. The user will only need to login once and we 
then store the auth token in local storage or cookies and use that token from storage or cookie for any future secured/protected API calls.

## More Resources on JWTs
**NG Europe talk from 0Auth.com guys**<br/>
https://www.youtube.com/watch?v=lDb_GANDR8U&list=UUEGUP3TJJfMsEM_1y8iviSQ

**Another good talk on JWT**<br/>
https://www.youtube.com/watch?v=vIGZxeQUUFU#t=83

**Debugger tool**<br/>
http://jwt.io

**Single Class File Library & Nuget Package I use for JWT dedcoding<br/>
Authored by FireBase, Twilio & others**<br/>
http://www.nuget.org/packages/JWT/<br/>
https://github.com/johnsheehan/jwt/blob/master/JWT/JWT.cs

## Explaining how my implementation works
1. A user will do a HTTP post of their backoffice Umbraco username & password to a normal API Controller
  1. Controller verifies credentials
  2. If a token already exists for the user (matches against user id) in custom PetaPoco DB table
  3. If no token exists create a new token for the user & store in the DB table
  4. Return existing or newly created token in the response

2. A user can then store that token say in LocalStorage or Cookie

3. User calls secured API sends bearer auth token in HTTP header for request (From LocalStorage or cookie)
  1. Server finds token in request
  2. Tries to decode the token with secret
  3. If token not encoded with same secret Send 401
  4. If token can be decoded correctly find user id in JSON
  5. Check if user has the same token stored in the DB
  6. If so process method on API controller

4. User changes password in Umbraco backoffice
  1. If no token exists in DB - nothing happens
  2. If token exists in DB - generate new token with new datetime stamp to make it unique from last time

## Why do you not just store the username & password in the JWT?
The payload of the JSON object in the JWT can be decoded easily, paste in a token into jwt.io and you can see it easily. 
However the part to do with JWT is that we verify that the AuthToken is using the signed secret/string to ensure our server created the JWT & it's validity.

So in my implementation I only store the username, user ID, user role and Created Date of the token. The date ensures that the token is different every time 
a new one is generated for easy revoking.

## Do the tokens expire?
My implementation allows the tokens to work indefinitely until the user in the Umbraco backoffice changes their password.
Which creates a new token and thus revoking access to the API for any clients or services using it.

However it is easily possible to store an expiry date in the JSON payload of the Auth Token and when decoding it verifying the expiry date on it.

How to use this
======================
Here are some simple instructions on how to use this to secure an API Controller in your Umbraco website with JWT Auth Tokens.

## Creating your API Controller Class
You will need to implement the following class `UmbracoAuthTokenApiController` similar to how you would use `UmbracoApiController` or `UmbracoAuthorizedApiController`

See the Umbraco Documentation for Umbraco API Controller Reference:<br/>
http://our.umbraco.org/documentation/Reference/WebApi/<br/>
http://our.umbraco.org/documentation/Reference/WebApi/authorization

Implementing the `UmbracoAuthTokenApiController` in conjuction with the `UmbracoAuthToken` attribute to decorate the API class allows us to access a new property in the Web API controller called `AuthorisedBackofficeUser` which is the authorised Umbraco backoffice user from the JWT token, this can be used with the normal Umbraco Services APIs to Create Content and assign the correct user to the creation of that node.

## Authorising the user & obtaining a JWT token
In this project there is a built in Web API URL route for you to do a HTTP POST with the Username and Password. Your application to gain a JWT token must POST to this URL:
http://yoursite.co.uk/umbraco/TokenAuth/SecureApi/Authorise

## Secured API Controller
The following is a very simple secued Umbraco API controller, obviously your needs and uses will be much better than the very basic example shown here.

The `UmbracoAuthToken` attribute has an optional parameter `HasAccessToSections` which is an string array of Umbraco backoffice section aliases that the attempted backoffice Umbraco user should have access to.

For example this checks the following user has access to the `content` and the `settings` sections, if they do not have access to **both** sections then the WebAPI will return 401 Unauthorised HTTP Error Code.

`[UmbracoAuthToken("content", "settings")]`

### Full Example
```cs
using System;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using UmbracoAuthTokens.Attributes;
using UmbracoAuthTokens.Controllers;

namespace UmbracoAuthTokens.TestApi
{
    [PluginController("Secured")]
    [UmbracoAuthToken("content", "settings")]
    public class ContentApiController : UmbracoAuthTokenApiController
    {
        /// <summary>
        /// A simple GET that shows the backoffice user's name & email address from the JWT Auth Token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string SecurePing()
        {
            return string.Format("Secure Pong from {0} {1}", AuthorisedBackofficeUser.Name, AuthorisedBackofficeUser.Email);
        }


        /// <summary>
        /// Note this is NOT a great example. As you would POST data to create a node.
        /// As opposed to a GET with this hardcoded values
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IContent CreateNewRootNode()
        {
            var newNodeName = string.Format("New Node {0}", DateTime.Now.ToShortDateString());
            var parentNodeId = -1;
            var contentTypeAlias = "Home";

            return Services.ContentService.CreateContentWithIdentity(newNodeName, parentNodeId, contentTypeAlias, AuthorisedBackofficeUser.Id);
        }
    }
}
```
## Release Notes
* 1.0.0.0 - Initial Release
* 1.1.0.0 - Update to DB schema and adds abbility to use JWT's with members as well now




