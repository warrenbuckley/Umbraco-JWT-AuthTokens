using System.Collections.Generic;
using System.Web.Http;
using Umbraco.AuthTokens.Attributes;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.AuthTokens.TestApi
{
    [PluginController("Test")]
    [UmbracoAuthToken("content", "settings")]
    public class ContentApiController : UmbracoApiController
    {
        /// <summary>
        /// Gets the Umbraco Backoffice user from the JWT Auth token
        /// Can use with normal Umbraco APIs then, for saving content etc...
        /// </summary>
        public IUser BackOfficeUser
        {
            get
            {
                return ControllerContext.RouteData.Values["umbraco-user"] as IUser;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<IContent> GetRootNodes()
        {
            return Services.ContentService.GetRootContent();

        }

        [HttpGet]
        public string SecurePing()
        {
            return "Secure Pong";
        }

    }
}
