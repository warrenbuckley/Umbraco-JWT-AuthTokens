using Umbraco.Core.Models.Membership;
using Umbraco.Web.WebApi;

namespace UmbracoAuthTokens.Controllers
{
    public abstract class UmbracoAuthTokenApiController : UmbracoApiController
    {
        /// <summary>
        /// When a user has been authorised with a JWT Auth token
        /// This is the backofficer user that represents the authorised user
        /// that can then be used in the API controller for Umbraco API Service Calls 
        /// such as creating content
        /// </summary>
        public IUser AuthorisedBackofficeUser
        {
            get
            {
                return ControllerContext.RouteData.Values["umbraco-user"] as IUser; 
            }
        }
    }
}
