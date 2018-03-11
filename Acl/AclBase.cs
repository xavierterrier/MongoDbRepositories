using MongoDB.Bson;
using Com.Xterr.MongoDbRepositories.Model;

namespace Com.Xterr.MongoDbRepositories.Acl
{
    public abstract class AclBase<T> where T : BaseModel
    {

        public abstract bool CanCreate(T entity, string userId);


        public abstract bool CanWrite(string entityId, string userId);

        public abstract bool CanRead(T entity, string userId);
        
        public abstract bool CanRead(string entityId, string userId);

    }
}