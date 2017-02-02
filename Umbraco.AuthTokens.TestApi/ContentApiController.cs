using System;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using UmbracoAuthTokens.Attributes;
using UmbracoAuthTokens.Controllers;

namespace UmbracoAuthTokens.TestApi
{
    [PluginController("Secured")]
    [UmbracoUserAuthToken("content", "settings")]
    public class ContentApiController : UmbracoUserAuthApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<IContent> GetRootNodes()
        {
            return Services.ContentService.GetRootContent();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string SecurePing()
        {
            return string.Format("Secure Pong from {0} {1}", AuthorisedBackofficeUser.Name, AuthorisedBackofficeUser.Email);
        }


        /// <summary>
        /// 
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
