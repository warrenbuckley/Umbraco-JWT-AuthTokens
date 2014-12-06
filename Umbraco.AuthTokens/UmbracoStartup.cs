using System.Linq;
using Umbraco.AuthTokens.Data;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.AuthTokens
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        /// <summary>
        /// When umbraco
        /// </summary>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //When Umbraco Started lets check for DB table exists
            var db = applicationContext.DatabaseContext.Database;

            //If table does not exist
            if (!db.TableExist("UserAuthTokens"))
            {
                //Create Table - do not override
                db.CreateTable<UmbracoAuthToken>(false);
            }

            //TODO: Generate a strong random secret/key to set as Env variable


            //Add event to saving/chaning pasword on Umbraco backoffice user
            UserService.SavingUser += UserService_SavingUser;

            //Continue as normal
            base.ApplicationStarted(umbracoApplication, applicationContext);
        }


        /// <summary>
        /// When we save a user, let's check if backoffice user has changed their password
        /// </summary>
        void UserService_SavingUser(IUserService sender, Core.Events.SaveEventArgs<Core.Models.Membership.IUser> e)
        {  
            //Saved entites (Could be more than one user saved. Very unlikely?)
            var user = e.SavedEntities.FirstOrDefault();

            //Found a user that has been saved
            if (user != null)
            {
                //Check if the password property (RawPasswordValue) is dirty aka has beeen changed
                var passIsDirty = user.IsPropertyDirty("RawPasswordValue");

                //Password has been changed
                if (passIsDirty)
                {
                    //Check if user already has token in DB (token created on first login/auth to API)
                    var hasAuthToken = UserAuthTokenDbHelper.GetAuthToken(user.Id);

                    //invalidate token (Only if token exists in DB)
                    //We have found an existing token
                    if (hasAuthToken != null)
                    {
                        //Generate AuthToken DB object
                        var newToken = new UmbracoAuthToken();
                        newToken.UserId = user.Id;
                        newToken.UserName = user.Username;
                        newToken.UserType = user.UserType.Alias;


                        //Generate a new token for the user
                        var authToken = UmbracoAuthTokenFactory.GenerateUserAuthToken(newToken);

                        //NOTE: We insert authToken as opposed to newToken
                        //As authToken now has DateTime & JWT token string on it now

                        //Store in DB (inserts or updates existing)
                        UserAuthTokenDbHelper.InsertAuthToken(authToken);
                    }
                }
            }
        }
    }
}
