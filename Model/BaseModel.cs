using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Com.Xterr.MongoDbRepositories.Model
{
    abstract public class BaseModel
    {

        public enum CrudEnum
        {
            Read = 0,
            Create = 1,
            Update = 2,
            Delete = 3
        }

        protected BaseModel() {

        }

        //        [Key]
        //        [Column("Id", Order = 0)]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }

        [BsonDateTimeOptions(Representation = BsonType.Document, Kind = DateTimeKind.Utc)]
        public DateTime? Timestamp { get; set; }

        public bool IsDeleted { get; set; }


        // Init model Id and childrens ids
        public virtual void SetId() {
            if (string.IsNullOrEmpty(this.Id))
            {
                this.Id = ObjectId.GenerateNewId().ToString();
            }
        }

        public virtual ValidationError Validate(CrudEnum crudType, bool allowCreateSpecifyId)
        {
         var result = new ValidationError();

            if (crudType != CrudEnum.Create && !Timestamp.HasValue)
            {
                result.InvalidInputs.Add("Timestamp", "Timestamp already set.");
            }

            if (crudType == CrudEnum.Create && !allowCreateSpecifyId && !string.IsNullOrEmpty(this.Id))
            {
                result.InvalidInputs.Add("Id", "Id must be null.");
            }

            if (crudType != CrudEnum.Create && string.IsNullOrEmpty(this.Id))
            {
                result.InvalidInputs.Add("Id", "Id must be specified.");
            }

             return result;
        }

   
    }
}


