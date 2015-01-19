using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using UmbracoAuthTokens.Data;

namespace UmbracoAuthTokens
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        /// <summary>
        /// When umbraco has started up
        /// </summary>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //When Umbraco Started lets check for DB table exists
            var db = applicationContext.DatabaseContext.Database;

            //If table does not exist
            if (!db.TableExist("identityAuthTokens"))
            {
                //Create Table - do not override
                db.CreateTable<UmbracoAuthToken>(false);
            }

            //Add event to saving/chaning pasword on Umbraco backoffice user
            UserService.SavingUser += UserService_SavingUser;

            //Add event to saving/chaning pasword on Umbraco member
            MemberService.Saving += MemberService_Saving;

            //Continue as normal
            base.ApplicationStarted(umbracoApplication, applicationContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MemberService_Saving(IMemberService sender, SaveEventArgs<IMember> e)
        {
            //Saved entites (Could be more than one member saved. Very unlikely?)
            var member = e.SavedEntities.FirstOrDefault();

            //Found a member that has been saved
            if (member != null)
            {
                //Check if the password property (RawPasswordValue) is dirty aka has beeen changed
                var passIsDirty = member.IsPropertyDirty("RawPasswordValue");

                //Password has been changed
                if (passIsDirty)
                {
                    //Check if user already has token in DB (token created on first login/auth to API)
                    var hasAuthToken = UserAuthTokenDbHelper.GetAuthToken(member.Id);

                    //invalidate token (Only if token exists in DB)
                    //We have found an existing token
                    if (hasAuthToken != null)
                    {
                        //Generate AuthToken DB object
                        var newToken = new UmbracoAuthToken();
                        newToken.IdentityId = member.Id;
                        newToken.IdentityType = IdentityAuthType.Member.ToString();

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


        /// <summary>
        /// When we save a user, let's check if backoffice user has changed their password
        /// </summary>
        void UserService_SavingUser(IUserService sender, SaveEventArgs<IUser> e)
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
                        newToken.IdentityId = user.Id;
                        newToken.IdentityType = IdentityAuthType.User.ToString();

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
