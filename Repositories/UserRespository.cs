using codetest.Models;
using codetest.Repositories.Interfaces;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace codetest.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        { }

        public override void AddSync(User user)
        {
            var now = System.DateTime.Now;
            user.AddedAt = now;
            user.ModifiedAt = now;
            user.CreatedAt = now;

            base.AddSync(user);
        }

        public override Task Add(User user)
        {
            var now = System.DateTime.Now;
            user.AddedAt = now;
            user.ModifiedAt = now;
            user.CreatedAt = now;
            return base.Add(user);
        }

        public override Task AddMany(ICollection<User> users)
        {
            var now = System.DateTime.Now;

            foreach (var user in users)
            {
                user.AddedAt = now;
                user.ModifiedAt = now;
                user.CreatedAt = now;
            }
            return base.AddMany(users);
        }

        public override void AddManySync(ICollection<User> users)
        {
            var now = System.DateTime.Now;

            foreach (var user in users)
            {
                user.AddedAt = now;
                user.ModifiedAt = now;
                user.CreatedAt = now;
            }
            base.AddManySync(users);
        }

        public override Task BulkInsert(ICollection<User> users)
        {
            var now = System.DateTime.Now;

            foreach (var user in users)
            {
                user.ModifiedAt = now;
            }
            return base.BulkInsert(users);
        }

        public override void BulkInsertSync(ICollection<User> users)
        {
            var now = System.DateTime.Now;

            foreach (var user in users)
            {
                user.ModifiedAt = now;
            }
            base.BulkInsertSync(users);
        }

        public override async Task ReplaceOne(User user, User newValue)
        {
            try
            {
                UpdateObj(user, newValue);
                await MongoCollection.ReplaceOneAsync<User>(x => x.Id == user.Id, user);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(User)}");
                Log.Error(exception.StackTrace);
            }
        }

        public override async Task ReplaceOne(Expression<Func<User, bool>> filter, User newValue)
        {
            try
            {
                User oldValue = GetSingleByExpressionSync(filter);
                UpdateObj(oldValue, newValue);
                await MongoCollection.ReplaceOneAsync<User>(filter, oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(User)}");
                Log.Error(exception.StackTrace);
            }
        }

        public override void ReplaceOneSync(object id, User newValue)
        {
            try
            {
                User oldValue = GetSingleByExpressionSync(x => x.Id == id.ToString());
                UpdateObj(oldValue, newValue);
                MongoCollection.ReplaceOne(FilterId(id), oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(User)}");
                Log.Error(exception.StackTrace);
            }
        }

        public override void ReplaceOneSync(Expression<Func<User, bool>> filter, User newValue)
        {
            try
            {
                User oldValue = GetSingleByExpressionSync(filter);
                UpdateObj(oldValue, newValue);
                MongoCollection.ReplaceOne<User>(filter, oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(User)}");
                Log.Error(exception.StackTrace);
            }
        }

        //made to replace repetitive logic in replace functions
        protected override void UpdateObj(User oldValue, User newValue)
        {
            PropertyInfo[] Tproperties = typeof(User).GetProperties();
            for (int i = 1; i < Tproperties.Length; i++)
            {
                if (Tproperties[i].Name != "Id" && Tproperties[i].Name != "ModifiedAt" && Tproperties[i].Name != "CreatedAt" && Tproperties[i].Name != "AddedAt")
                    Tproperties[i].SetValue(oldValue, Tproperties[i].GetValue(newValue));
            }
            oldValue.ModifiedAt = DateTime.UtcNow;
        }


    }
}
