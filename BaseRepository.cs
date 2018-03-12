using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using Com.Xterr.MongoDbRepositories.Model;
using Com.Xterr.MongoDbRepositories.Exceptions;
using Com.Xterr.MongoDbRepositories.Acl;
using static Com.Xterr.MongoDbRepositories.Model.BaseModel;
using Core.Repositories;

namespace Com.Xterr.MongoDbRepositories
{
    /// <summary>
    /// The base repository which ensure the security and model validation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseRepository<T> where T : BaseModel, new()
    {
        private readonly IMongoCollection<T> _collection;
        private readonly AclBase<T> _acl;

        private string _entityType;
        private AuditTraceRepository _audit;

        protected BaseRepository(IMongoCollection<T> collection, AclBase<T> acl, Boolean enableAuditTraces = false)
        {
            _collection = collection;
            _acl = acl;
            _entityType = typeof(T).Name;

            if (enableAuditTraces) {
                _audit = new AuditTraceRepository(collection.Database);
            }
        }

        protected AclBase<T> Acl
        {
            get
            {
                return _acl;
            }
        }

        /// <summary>
        /// Get By Id.
        /// </summary>
        /// <param name="userId">The requester user id (for acl).</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public virtual T GetById(string id, string userId)
        {
            T result;

            result = _collection.Find(Builders<T>.Filter.Eq(x => x.Id, id)).FirstOrDefault();

            if (result != null && !_acl.CanRead(result, userId))
            {
                throw new ForbiddenException(string.Format("User '{0}' does not have the right to read a '{1}' with id '{2}'.", userId, typeof(T), id));
            }

            if (result == null)
            {
                throw new NotFoundException(string.Format("{0} with id {1} does not exist.", typeof(T), id));
            }

            return result;
        }

        /// <summary>
        /// Get entitites.
        /// </summary>
        /// <param name="userId">The requester user id (for acl).</param>
        /// <param name="filter">Specifies a filter expression.</param>
        /// <param name="orderBy">Specifies an order by expression.</param>
        /// <returns></returns>
        public List<T> Get(
            string userId,
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = _collection.AsQueryable<T>();
            query = query.Where(m => !m.IsDeleted);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var unsecuredResults = orderBy != null ? orderBy(query) : query;

            //var debug = unsecuredResults.ToList();

            // Applies ACL
            return unsecuredResults.Where(e => _acl.CanRead(e, userId)).ToList();
        }

        /// <summary>
        /// Create the given model
        /// Can throw NotAuthorizedException or ValidationException
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public T Create(T model, string userId)
        {
            if (!_acl.CanCreate(model, userId))
            {
                throw new ForbiddenException(string.Format("User '{0}' does not have the right to create a '{1}'.", userId, typeof(T)));
            }

            // Validate the model
            var errors = model.Validate(CrudEnum.Create, true);
            if (errors.HasErrors())
            {
                throw new ValidationException(errors);
            }

            // Set Ids
            model.SetId();

            // Set Timestamp
            model.Timestamp = DateTime.Now.ToUniversalTime();

            T newModel = null;

            // Add in Context
            _collection.InsertOne(model);
            newModel = this.GetById(model.Id, userId);

            AuditTrace(userId, newModel.Id, "CREATE", null, model);

            return newModel;
        }

        /// <summary>
        /// Update the given model
        /// Can throw NotAuthorizedException or ValidationException
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public T Update(T model, string userId)
        {
            var errors = model.Validate(CrudEnum.Update, false);
            if (errors.HasErrors())
            {
                throw new ValidationException(errors);
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                throw new Exception($"A model of type {typeof(T)} has no id but model.Validate(CrudEnum.Update) is correct.");
            }

            if (!_acl.CanWrite(model.Id, userId))
            {
                throw new ForbiddenException(string.Format("User '{0}' does not have the right to update a '{1}' with id '{2}'.", userId, typeof(T), model.Id));
            }

            // Set Ids
            model.SetId();

            T oldModel = null;
            T newModel = null;

            var filter = Builders<T>.Filter.Eq(e => e.Id, model.Id);

            oldModel = _collection.Find(filter).FirstOrDefault();

            //oldModel = _collection.FindAs<T>(Query<T>.EQ(e => e.Id, model.Id)).SetFields("Timestamp").FirstOrDefault();

            if (oldModel == null || oldModel.Timestamp != model.Timestamp)
            {
                throw new ConcurrencyException("Current record has been updated by another user.");
            }

            // Set Timestamp
            model.Timestamp = DateTime.Now.ToUniversalTime();

            var result = _collection.ReplaceOne(filter, model);

            if (!result.IsAcknowledged)
            {
                throw new DatabaseException(string.Format("Unable to update document for user {0} in collection '{1}' with documentId '{2}'.", userId, typeof(T), model.Id));
            }

            newModel = this.GetById(model.Id, userId);

            AuditTrace(userId, newModel.Id, "UPDATE", oldModel, newModel);
            
            return newModel;
        }

        /// <summary>
        /// Delete the entity id
        /// Can throw NotAuthorizedException
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        public void Delete(string id, string userId)
        {

            if (!_acl.CanWrite(entityId: id, userId: userId))
            {
                throw new ForbiddenException(string.Format("User '{0}' does not have the right to delete a '{1}' with id '{2}'.", userId, typeof(T), id));
            }

            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var update = Builders<T>.Update
                .Set(m => m.IsDeleted, true).CurrentDate(m => m.Timestamp);

            var result = _collection.UpdateOne(filter, update);

            if (!result.IsAcknowledged)
            {
                throw new DatabaseException(string.Format("Unable to delete (update IsDeleted field) document for user {0} in collection '{1}' with documentId '{2}'.", userId, typeof(T), id));
            }

             AuditTrace(userId, id, "DELETE", null, null);
        }

        protected void AuditTrace(string userId, string entityId, string trace, Object oldModel, Object newModel)
        {
            if (_audit != null) {
                _audit.Trace(userId, _entityType, entityId, trace, oldModel, newModel);
            }
        }
    }
}
