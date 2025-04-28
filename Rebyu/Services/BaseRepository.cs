using LiteDB;
using Rebyu.Interfaces;
using System.Collections.Generic;

namespace Rebyu.Services;

public class BaseRepository<T> : IBaseRepository<T>
{
    public ILiteDatabase DB { get; }
    public ILiteCollection<T> Collection { get; }

    protected BaseRepository(ILiteDatabase db)
    {
        DB = db;
        Collection = db.GetCollection<T>();
    }

    public virtual T Create(T entity)
    {
        var newId = Collection.Insert(entity);
        return Collection.FindById(newId.AsInt32);
    }

    public virtual IEnumerable<T> All() => 
        Collection.FindAll();

    public virtual T FindById(string id) => 
        Collection.FindById(id);

    public virtual void Update(T entity) => Collection.Upsert(entity);

    public virtual bool Delete(string id) => 
        Collection.Delete(id);
}
