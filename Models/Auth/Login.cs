using codetest.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace codetest.Models
{
    [Serializable]
    public class Login : IEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        public string UserLogin { get; set; }

        [BsonRequired]
        public string Password { get; set; }

        public List<string> Roles { get; set; }

        public bool? RememberMe { get; set; }

        public override string ToString()
        {
            return $"UserLogin: {this.UserLogin}";
        }
    }
}
