using System;
using System.Collections.Generic;
using System.Globalization;
using JWT;

namespace UmbracoAuthTokens.Data
{
    public static class UmbracoAuthTokenFactory
    {
        static string _secretKey = UmbracoAuthTokenSecret.GetSecret();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static UmbracoAuthToken GenerateUserAuthToken(UmbracoAuthToken authToken)
        {
            //Date Time
            var dateCreated = DateTime.UtcNow;
            var dateCreatedToString = dateCreated.ToString("u");

            //Create JSON payload for JWT token
            var payload = new Dictionary<string, object>() {
                { "user_id", authToken.UserId },
                { "username", authToken.UserName },
                { "user_type", authToken.UserType },
                { "date_created", dateCreatedToString }
            };

            //Encode the JWT token with JSON payload, algorithm & our secret in constant
            var encodedToken = JsonWebToken.Encode(payload, _secretKey, JwtHashAlgorithm.HS256);

            //Return same object we passed in (Now with Date Created & Token properties updated)
            authToken.DateCreated = dateCreated;
            authToken.AuthToken = encodedToken;

            //Return the updated object
            return authToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public static UmbracoAuthToken DecodeUserAuthToken(string jwtToken)
        {
            //Object to return
            var userAuth = new UmbracoAuthToken();

            //Decode & verify token was signed with our secret
            var jsonPayload = JsonWebToken.DecodeToObject(jwtToken, _secretKey) as IDictionary<string, object>;

            //Just the presence of the token & being deserialised with correct SECRET key is a good sign
            if (jsonPayload != null)
            {
                //Do DateTime conversion from u type back into DateTime object
                DateTime dateCreated;
                DateTime.TryParseExact(jsonPayload["date_created"].ToString(), "u", null, DateTimeStyles.AdjustToUniversal, out dateCreated);

                //Get the details of the user from the JWT payload
                userAuth.UserId = Convert.ToInt32(jsonPayload["user_id"]);
                userAuth.UserName = jsonPayload["username"].ToString();
                userAuth.UserType = jsonPayload["user_type"].ToString();
                userAuth.DateCreated = dateCreated;
                userAuth.AuthToken = jwtToken;
            }

            //Return the object
            return userAuth;
        }

    }
}
