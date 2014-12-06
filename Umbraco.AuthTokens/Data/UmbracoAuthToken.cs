using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.AuthTokens.Data
{
    [TableName("userAuthTokens")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    public class UmbracoAuthToken
    {
        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("userName")]
        public string UserName { get; set; }

        [Column("userType")]
        public string UserType { get; set; }

        [Column("dateCreated")]
        public DateTime DateCreated { get; set; }

        [Column("authToken")]
        public string AuthToken { get; set; }
    }
}
