using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Myproject.Repositories;
using Myproject.Models;
using System.Net;
using System.Text;
using System.Linq.Expressions;
using Myproject.HtmlHelpers;
namespace Myproject.Areas.Administrator.Controllers
{
    [AdminSessionControl]
    public class BaseController<T> : Controller,IRepository<T> where T:class,new ()
    {
        dbDataContext db = new dbDataContext();

        
        public virtual ActionResult Index()
        {
            var items = GetAll(1);

            return View(items);
        }

        public virtual ActionResult ListByID(int ParentID, string id, int PageIndex)
        {
            var items = GetAllByID(ParentID, id, PageIndex);

            return View("Index", items);
        }

        public virtual ActionResult Page(int PageIndex)
        {
            var items = GetAll(PageIndex);
            string name = typeof(T).Name;

            return PartialView(name + "List", items);
        }

        public virtual ActionResult Create()
        {

            return PartialView(new T());
        }

    

        [HttpPost]
        public virtual ActionResult Create(T item, int PageIndex, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return PartialView(item);
            }

            try
            {
                add(item);
                string name = typeof(T).Name;

                var items = GetAll(PageIndex);
                return PartialView(name + "List", items);
            }
            catch (Exception ex)
            {
                return Content(GetErrorMessage(ex));
            }
        }

        public virtual ActionResult Edit(int id)
        {
            T item = GetByID(id);
            return PartialView(item);
        }

        [HttpPost]
        public virtual ActionResult Edit(int id, T item, int PageIndex, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return PartialView(item);
            }

            try
            {
                T itemData = GetByID(id);

                UpdateModel(itemData);
                Save();

                string name = typeof(T).Name;

                var items = GetAll(PageIndex);
                return PartialView(name + "List", items);
            }
            catch (Exception ex)
            {
                return Content(GetErrorMessage(ex));
            }
        }

        public virtual ActionResult Search()
        {
            return PartialView(new T());
        }
        [HttpPost]
        public virtual ActionResult Search(T item)
        {
            if (!ModelState.IsValidField("Name"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;

                return PartialView(item);
            }

            var name = item.GetType().GetProperty("Name").GetValue(item, null);
            var items = SearchData(name.ToString());

            ViewData["Search"] = true;
            string listName = typeof(T).Name;
            return PartialView(listName + "List", items);
        }
        public virtual ActionResult Delete(int PageIndex, int id)
        {
            try
            {
                T item = GetByID(id);
                deleteData(item);

                string name = typeof(T).Name;

                var items = GetAll(PageIndex);
                return PartialView(name + "List", items);
            }
            catch (Exception ex)
            {
                return Content(GetErrorMessage(ex));
            }
        }

        public string GetErrorMessage(Exception ex)
        {
            StringBuilder errorMessage = new StringBuilder(200);

            errorMessage.AppendFormat("<div class=\"validation-summary-errors\" title=\"Server Error\">{0}</div>", ex.GetBaseException().Message);
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return errorMessage.ToString();
        }

        public void add(T item)
        {
            //db.Categories.InsertOnSubmit(Category); gibi
            db.GetTable<T>().InsertOnSubmit(item);
            Save();
        }

        public void deleteData(T item)
        {
            db.GetTable<T>().DeleteOnSubmit(item);
            Save();
        }

        public IQueryable<T> GetAllByID(int ParentID, string columnName, int PageIndex)
        {

            int pageSize = 25;
            this.RouteData.Values["PageIndex"] = PageIndex;

            PageIndex = PageIndex - 1;

            var itemParameter = Expression.Parameter(typeof(T), "item");
            //c=>c.ID==id   item=>item.ID==id

            var whereExpression = Expression.Lambda<Func<T, bool>>
                (
                    Expression.Equal
                    (
                        Expression.Property
                        (
                            itemParameter, columnName
                        ),
                        Expression.Constant(ParentID)
                    ),
                    new[] { itemParameter }
                );

            int totalCount = db.GetTable<T>().Where(whereExpression).Count();

            int totalPage = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["TPage"] = totalPage;

            return db.GetTable<T>().Where(whereExpression).Skip(PageIndex * pageSize).Take(pageSize);
        }

        public IQueryable<T> GetAll(int PageIndex)
        {
            int pageSize = 25;
            this.RouteData.Values["PageIndex"] = PageIndex;

            PageIndex = PageIndex - 1;
            int totalCount = db.GetTable<T>().Count();
            int totalPage = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["TPage"] = totalPage;

            return db.GetTable<T>().Skip(PageIndex * pageSize).Take(pageSize);
        }

        public IQueryable<T> SearchData(string name)
        {
            this.RouteData.Values["PageIndex"] = 1;
            ViewData["TPage"] = 1;

            var itemParameter = Expression.Parameter(typeof(T), "item");
            //c=>c.ID==id   item=>item.Name==name

            var whereExpression = Expression.Lambda<Func<T, bool>>
                (
                    Expression.Equal
                    (
                        Expression.Property
                        (
                            itemParameter, "Name"
                        ),
                        Expression.Constant(name)
                    ),
                    new[] { itemParameter }
                );

            return db.GetTable<T>().Where(whereExpression);
        }

        public T GetByID(int id)
        {
            var itemParameter = Expression.Parameter(typeof(T), "item");
            //c=>c.ID==id   item=>item.ID==id

            var whereExpression = Expression.Lambda<Func<T, bool>>
                (
                    Expression.Equal
                    (
                        Expression.Property
                        (
                            itemParameter, "ID"
                        ),
                        Expression.Constant(id)
                    ),
                    new[] { itemParameter }
                );

            return db.GetTable<T>().SingleOrDefault(whereExpression);
        }

        public void Save()
        {
            db.SubmitChanges();
        }

     
    }
}
