using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProject.Repositories
{
    public interface IRepository<T>
    {
        void add(T item);
        void deleteData(T item);
        IQueryable<T> GetAllByID(int ParentID, string ColumbName, int PageIndex);
        IQueryable<T> GetAll(int PageIndex);
        IQueryable<T> SearchData(string name);
        T GetByID(int id);
        void Save();



    }
}