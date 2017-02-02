using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JWT;
using Umbraco.Core;
using Umbraco.Core.Models;
using UmbracoAuthTokens.Data;
 
namespace UmbracoAuthTokens.Attributes
{
    //KUDOS to Stephan Gay
    //This is from his BasicAuth attribute in Models Builder
    //https://github.com/zpqrtbnk/Zbu.ModelsBuilder/blob/master/Zbu.ModelsBuilder.AspNet/ModelsBuilderAuthFilter.cs
 
    public class UmbracoMemberAuthToken : ActionFilterAttribute
    {
        private string _isInGroup;
 
        /// <summary>
        /// Assign this attribute to protect a WebAPI call for an Umbraco member
        /// </summary>
        /// <param name="isInGroup">This is the group alias that the member must be in. This is an optional param</param>
        public UmbracoMemberAuthToken(string isInGroup)
        {
            _isInGroup = isInGroup;
        }
 
        /// <summary>
        /// When the attribute is decorated on an Umbraco WebApi Controller
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                //Auth the member from the request (HTTP headers)
                var member = Authenticate(actionContext.Request);
 
                //Member details not correct (as member obj null)
                if (member == null)
                {
                    //Return a HTTP 401 Unauthorised header
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
 
                //Set the user in route data so the WebAPI controller can use this user object & do what needed with it
                actionContext.ControllerContext.Request.Properties.Add("umbraco-member", member);
 
                //If we have any optional member groups to check for
                if (_isInGroup.Any())
                {
                    //Check that the user has access to the one or more provided sections (Contains ALL)
                    var hasAccess = member != null && ApplicationContext.Current.Services.MemberService.GetMembersByGroup(_isInGroup).Contains(member);
 
                    //If member is NOT in member group specified
                    if (!hasAccess)
                    {
                        //Return a HTTP 401 Unauthorised header
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    }
                }
 
            }
            catch (Exception)
            {
                //Return a HTTP 401 Unauthorised header
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
 
            //Continue as normal
            base.OnActionExecuting(actionContext);
        }
 
        /// <summary>
        /// Try and auth the user from the HTTP headers on the request to the API
        /// Look for bearer token aka JWT token & try to verify & deserialise it
        /// </summary>
        /// <param name="request"></param>
        /// <returns>If success auth'd return the associated Umbraco backoffice user</returns>
        private static IMember Authenticate(HttpRequestMessage request)
        {
            //Try to get the Authorization header in the request
            var ah = request.Headers.Authorization;
 
            //If no Auth header sent or the scheme is not bearer aka TOKEN
            if (ah == null || ah.Scheme.ToLower() != "bearer")
            {
                //Return null (by returning null, base method above will return it as HTTP 401)
                return null;
            }
 
            //Get the JWT token from auth HTTP header param  param (Base64 encoded - username:password)
            var jwtToken = ah.Parameter;
 
            try
            {
                //Decode & verify token was signed with our secret
                var decodeJwt = UmbracoAuthTokenFactory.DecodeUserAuthToken(jwtToken);
 
                //Ensure our token is not null (was decoded & valid)
                if (decodeJwt != null)
                {
                    //Just the presence of the token & being deserialised with correct SECRET key is a good sign
                    //Get the member from userService from it's id
                    var member = ApplicationContext.Current.Services.MemberService.GetById(decodeJwt.IdentityId);
 
                    //If user is NOT Approved OR the user is Locked Out
                    if (!member.IsApproved || member.IsLockedOut)
                    {
                        //Return null (by returning null, base method above will return it as HTTP 401)
                        return null;
                    }
 
                    //Verify token is what we have on the user
                    var isTokenValid = UserAuthTokenDbHelper.IsTokenValid(decodeJwt);
 
                    //Token matches what we have in DB
                    if (isTokenValid)
                    {
                        //Lets return the member
                        return member;
                    }
 
                    //Token does not match in DB
                    return null;
                }
 
 
                //JWT token could not be serialised to AuthToken object
                return null;
            }
            catch (SignatureVerificationException ex)
            {
                //Bubble exception up
                throw ex;
            }
 
        }
    }
}