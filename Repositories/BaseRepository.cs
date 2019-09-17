using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using codetest.Models.Interfaces;
using codetest.MongoDB.Interfaces;
using MongoDB.Driver;
using Serilog;
using System.Reflection;

namespace codetest.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected IMongoCollection<T> MongoCollection { get; set; }

        private IMongoDatabase _mongoDatabase;

        public BaseRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            Initialize();
        }

        protected void Initialize()
        {
            GetCollection();
            if (MongoCollection != null) return;       
            Log.Debug("Creating mongo index");
            CreateIndex();
        }

        protected void GetCollection()
        {
            Type type = typeof(T);
            if (MongoCollection != null) return;
            Log.Debug($"Getting MongoCollection {typeof(T)}");
            MongoCollection = _mongoDatabase.GetCollection<T>(type.Name);
        }

        protected virtual void CreateIndex()
        {
            try
            {
                List<IndexKeysDefinition<T>> keys = new List<IndexKeysDefinition<T>>();
                PropertyInfo[] Tproperties = typeof(T).GetProperties();
                for (int i = 0; i < Tproperties.Length; i++)
                {
                    keys.Add(Builders<T>.IndexKeys.Descending(x => Tproperties[i].GetValue(x)));
                }
                var indexDefinition = Builders<T>.IndexKeys.Combine(keys);

                MongoCollection.Indexes.CreateOne(indexDefinition);
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(T)}");
                Log.Error(exception.StackTrace);
            }
        }

        public virtual void AddSync(T entity)
        {
            MongoCollection.InsertOne(entity);
        }

        public virtual async Task Add(T entity)
        {
            await MongoCollection.InsertOneAsync(entity);
        }

        public virtual async Task AddMany(ICollection<T> entities)
        {
            await MongoCollection.InsertManyAsync(entities);
        }

        public virtual void AddManySync(ICollection<T> entities)
        {
            MongoCollection.InsertMany(entities);
        }

        public virtual async Task BulkInsert(ICollection<T> entities)
        {
            var stores = new List<WriteModel<T>>();

            stores.AddRange(entities.Select(x => new InsertOneModel<T>(x)));

            await MongoCollection.BulkWriteAsync(stores);
        }

        public virtual void BulkInsertSync(ICollection<T> entities)
        {
            var stores = new List<WriteModel<T>>();

            stores.AddRange(entities.Select(x => new InsertOneModel<T>(x)));

            MongoCollection.BulkWrite(stores);
        }

        public async Task<long> Count(Expression<Func<T, bool>> filter)
        {
            return await MongoCollection.CountDocumentsAsync(filter);
        }

        public long CountSync(Expression<Func<T, bool>> filter)
        {
            return MongoCollection.CountDocuments(filter);
        }

        public async Task Delete(T entity)
        {
            await MongoCollection.DeleteOneAsync(Builders<T>.Filter.Eq(e => e.Id, entity.Id));
        }

        public virtual async Task ReplaceOne(T entity, T newValue)
        {
            try
            {
                UpdateObj(entity, newValue);
                await MongoCollection.ReplaceOneAsync<T>(x => x.Id == entity.Id, entity);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(T)}");
                Log.Error(exception.StackTrace);
            }
        }

        public virtual async Task ReplaceOne(Expression<Func<T, bool>> filter, T newValue)
        {
            try
            {
                T oldValue = GetSingleByExpressionSync(filter);
                UpdateObj(oldValue, newValue);
                await MongoCollection.ReplaceOneAsync<T>(filter, oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(T)}");
                Log.Error(exception.StackTrace);
            }
        }

        public virtual void ReplaceOneSync(object id, T newValue)
        {
            try
            {
                T oldValue = GetSingleByExpressionSync(x => x.Id == id.ToString());
                UpdateObj(oldValue, newValue);
                MongoCollection.ReplaceOne(FilterId(id), oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(T)}");
                Log.Error(exception.StackTrace);
            }
        }

        

        public virtual void ReplaceOneSync(Expression<Func<T, bool>> filter, T newValue)
        {
            try
            {
                T oldValue = GetSingleByExpressionSync(filter);
                UpdateObj(oldValue,newValue);
                MongoCollection.ReplaceOne<T>(filter, oldValue);        //the oldValue now is supposed to be updated with the newValue :)
            }
            catch (Exception exception)
            {
                Log.Error($"Error creating index {typeof(T)}");
                Log.Error(exception.StackTrace);
            }
        }

        //made to replace repetitive logic in replace functions
        protected virtual void UpdateObj(T oldValue, T newValue)
        {
            PropertyInfo[] Tproperties = typeof(T).GetProperties();
            for (int i = 1; i < Tproperties.Length; i++)
            {
                if (Tproperties[i].Name != "Id")
                    Tproperties[i].SetValue(oldValue, Tproperties[i].GetValue(newValue));
            }
        }

        protected static FilterDefinition<T> FilterId(object key)
        {
            return Builders<T>.Filter.Eq("Id", key);
        }

        public void DeleteSync(T entity)
        {
            MongoCollection.DeleteOne(Builders<T>.Filter.Eq(e => e.Id, entity.Id));
        }

        public async Task Delete(Expression<Func<T, bool>> filter)
        {
            await MongoCollection.DeleteOneAsync(filter);
        }

        public void DeleteSync(Expression<Func<T, bool>> filter)
        {
            MongoCollection.DeleteOne(filter);
        }

        public ICollection<T> GetByExpressionSync(Expression<Func<T, bool>> filter, SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return findFluent.ToList();
        }

        public async Task<ICollection<T>> GetByExpression(Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null, ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return await findFluent.ToListAsync();
        }

        public async Task<ICollection<T>> GetAll(SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(_ => true, sort, projection);
            return await findFluent.ToListAsync();
        }

        public ICollection<T> GetAllSync(SortDefinition<T> sort = null, ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(_ => true, sort, projection);
            return findFluent.ToList();
        }

        public async Task<ICollection<T>> GetByPageAndQuantity(
            int page,
            int quantity,
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return await findFluent.Limit(quantity).Skip((page - 1) * quantity).ToListAsync();
        }

        public ICollection<T> GetByPageAndQuantitySync(
            int page,
            int quantity,
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return findFluent.Limit(quantity).Skip((page - 1) * quantity).ToList();
        }

        public async Task<T> GetSingleByExpression(
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null,
            int index = 1)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return await findFluent.Limit(1).Skip(index - 1).FirstOrDefaultAsync();
        }

        public T GetSingleByExpressionSync(
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null,
            int index = 1)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return findFluent.Limit(1).Skip(index - 1).FirstOrDefault();
        }

        public async Task<bool> Any(
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null,
            int index = 1)
        {
            var findFluent = BuildFindFluent(filter, sort, projection);
            return await findFluent.AnyAsync();
        }

        public bool AnySync(
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null,
            int index = 1)
        {
            var test = filter.Compile();
            var findFluent = BuildFindFluent(filter, sort, projection);
            return findFluent.Any();
        }

        public IEnumerable<T> QueryDb(Func<T, bool> filter)
        {
            var retult = MongoCollection.AsQueryable()
                .Where(filter);

            return retult.Distinct();
        }

        protected IFindFluent<T, T> BuildFindFluent(
            Expression<Func<T, bool>> filter,
            SortDefinition<T> sort = null,
            ProjectionDefinition<T> projection = null)
        {
            var fluent = MongoCollection.Find(filter);
            if (projection != null)
            {
                fluent.Options.Projection = Builders<T>.Projection.Combine(projection);
            }

            if (sort != null)
            {
                fluent.Sort(sort);
            }

            return fluent;
        }
    }
}