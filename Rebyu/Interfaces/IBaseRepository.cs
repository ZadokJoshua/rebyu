using System.Collections.Generic;

namespace Rebyu.Interfaces;

public interface IBaseRepository<T>
{
    IEnumerable<T> All();
    T Create(T entity);
    bool Delete(string id);
    T FindById(string id);
    void Update(T entity);
}