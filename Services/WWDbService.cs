using System;
using System.Collections.Generic;
using System.Linq;
using Superpower.Model;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Webwonders.Extensions.Models;

namespace Webwonders.Extensions.Services
{


    public interface IWWDbService
    {
        T Select<T>(int id) where T : WWDbBase;
        T Select<T>(int d, string sql) where T : WWDbBase;
        T Select<T>(IUmbracoDatabase db, int id) where T : WWDbBase;
        IEnumerable<T> Select<T>() where T : WWDbBase;
        IEnumerable<T> Select<T>(string sql) where T : WWDbBase;
        IEnumerable<T> Select<T>(IUmbracoDatabase db, string sql) where T : WWDbBase;
        T Insert<T>(T value) where T : WWDbBase;
        T Insert<T>(IUmbracoDatabase db, T value) where T : WWDbBase;
        T Update<T>(T value) where T : WWDbBase;
        T Update<T>(IUmbracoDatabase db, T value) where T : WWDbBase;
        void Delete<T>(T value) where T : WWDbBase;
        void Delete<T>(IUmbracoDatabase db, T value) where T : WWDbBase;
        int BeginTransaction(IUmbracoDatabase db);
        void CompleteTransaction(IUmbracoDatabase db);
        void CompleteTransaction(IUmbracoDatabase db, int transactionState);
        void AbortTransaction(IUmbracoDatabase db);
        void AbortTransaction(IUmbracoDatabase db, int transactionState);
    }


    public class WWDbService : IWWDbService
    {

        private readonly IScopeProvider _scopeProvider;

        public WWDbService(IScopeProvider ScopeProvider)
        {
            _scopeProvider = ScopeProvider;
        }


        /// <summary>
        /// Query table by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">id of record to fetch</param>
        /// <returns>record or null</returns>
        public T Select<T>(int id) where T : WWDbBase
        {
            return Select<T>(id, null);
        }



        /// <summary>
        /// Query table by id, with extra sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public T Select<T>(int id, string sql) where T : WWDbBase 
        {
            T result = null;
            if (id > 0)
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    IUmbracoDatabase db = scope.Database;
                    string sqlString = $"WHERE Deleted IS NULL AND Id = @0 {sql}";
                    result = db.SingleOrDefault<T>(sqlString, id);

                    scope.Complete();
                }

            }
            return result;
        }



        /// <summary>
        /// Query table by id within an existing scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">database of scope</param>
        /// <param name="id">id to find</param>
        /// <returns>record or null</returns>
        public T Select<T>(IUmbracoDatabase db, int id) where T : WWDbBase
        {
            T result = null;
            if (db != null && id > 0)
            {
                string sqlString = "WHERE Deleted IS NULL AND Id = @0";
                result = db.SingleOrDefault<T>(sqlString, id);
            }
            return result;

        }


        /// <summary>
        /// Query table for all records that are not deleted 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>all non-deleted records or null</returns>
        public IEnumerable<T> Select<T>() where T : WWDbBase
        {
            return Select<T>("");
        }



        /// <summary>
        /// Query table for all records that are not deleted and pass optional sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">optional sql filter, start with AND or OR</param>
        /// <returns>all non-deleted records that pass sql filter or null</returns>
        public IEnumerable<T> Select<T>(string sql) where T : WWDbBase
        {
            IEnumerable<T> result = Enumerable.Empty<T>();
            using (IScope scope = _scopeProvider.CreateScope())
            {
                IUmbracoDatabase db = scope.Database;
                string sqlString = $"WHERE Deleted IS NULL {sql}";
                result = db.Query<T>(sqlString); // Query filters on database, fetch on client. Use Query

                scope.Complete();
            }
            return result;
        }



        /// <summary>
        /// Query table for all records that are not deleted and pass optional sql
        /// within an existing scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">database of scope</param>
        /// <param name="sql">optional sql filter, start with AND or OR</param>
        /// <returns>all non-deleted records that pass sql filter or null</returns>
        public IEnumerable<T> Select<T>(IUmbracoDatabase db, string sql) where T : WWDbBase
        {
            IEnumerable<T> result = Enumerable.Empty<T>();

            if (db != null)
            {
                string sqlString = $"WHERE Deleted IS NULL {sql}";
                result = db.Query<T>(sqlString); // Query filters on database, fetch on client. Use Query
            }

            return result;

        }




        /// <summary>
        /// Insert a record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">record to insert</param>
        /// <returns>the inserted record</returns>
        public T Insert<T>(T value) where T : WWDbBase
        {
            if (value != null)
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    IUmbracoDatabase db = scope.Database;

                    value.Created = DateTime.Now;
                    db.Insert<T>(value); // value gets populated back

                    scope.Complete();
                }
            }
            return value;
        }



        /// <summary>
        /// Insert a record within an existing scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">IUmbracoDatabase database of current scope</param>
        /// <param name="value">record to insert</param>
        /// <returns></returns>
        public T Insert<T>(IUmbracoDatabase db, T value) where T : WWDbBase
        {
            if (db != null && value != null)
            {
                value.Created = DateTime.Now;
                db.Insert<T>(value);
            }

            return value;
        }


        /// <summary>
        /// Update a record
        /// </summary>
        /// <typeparam name="T">record to update</typeparam>
        /// <param name="value">the updated record</param>
        /// <returns></returns>
        public T Update<T>(T value) where T : WWDbBase
        {
            if (value != null)
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    IUmbracoDatabase db = scope.Database;

                    // Save current Created value
                    T current = Select<T>(value.Id);
                    if (current != null)
                    {
                        value.Created = current.Created;
                    }

                    // Adjust Modified value
                    value.Modified = DateTime.Now;

                    // Update new values
                    db.Update(value);

                    scope.Complete();
                }
            }
            return value;

        }



        /// <summary>
        /// update a record within an existing scope
        /// </summary>
        /// <typeparam name="T">record to update</typeparam>
        /// <param name="db">IUmbracoDatabase database of current scope</param>
        /// <param name="value">updated record</param>
        /// <returns></returns>
        public T Update<T>(IUmbracoDatabase db, T value) where T : WWDbBase
        {
            if (db != null && value != null)
            {

                // Save current Created value
                T current = Select<T>(db, value.Id);
                if (current != null)
                {
                    value.Created = current.Created;
                }

                // Adust Modified value
                value.Modified = DateTime.Now;

                // Update new values
                db.Update(value);

            }
            return value;
        }




        /// <summary>
        /// Delete record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">record to delete</param>
        /// <returns></returns>
        public void Delete<T>(T value) where T : WWDbBase
        {
            if (value != null)
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    IUmbracoDatabase db = scope.Database;

                    value.Deleted = DateTime.Now;
                    db.Delete<T>(value);

                    scope.Complete();
                }
            }
        }


        /// <summary>
        /// Delete record within an existing scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">IUmbracoDatabase database of current scope</param>
        /// <param name="value">record to delete</param>
        /// <returns></returns>
        public void Delete<T>(IUmbracoDatabase db, T value) where T : WWDbBase
        {
            if (db != null && value != null)
            {
                value.Deleted = DateTime.Now;
                db.Delete<T>(value);
            }
        }


        public int BeginTransaction(IUmbracoDatabase db)
        {
            int result = (int)TransactionState.Unspecified;

            if (db != null)
            {
                if (db.InTransaction)
                {
                    result = (int)TransactionState.ExistingTransaction;
                }
                else
                {
                    result = (int)TransactionState.NewTransaction;
                    db.BeginTransaction();
                }
            }
            return result;
        }


        public void CompleteTransaction(IUmbracoDatabase db)
        {
            if (db != null)
            {
                db.CompleteTransaction();
            }
        }


        public void CompleteTransaction(IUmbracoDatabase db, int transactionState)
        {
            if (transactionState == (int)TransactionState.NewTransaction && db != null)
            {
                db.CompleteTransaction();
            }
        }


        public void AbortTransaction(IUmbracoDatabase db)
        {
            if (db != null)
            {
                db.AbortTransaction();
            }
        }


        public void AbortTransaction(IUmbracoDatabase db, int transactionState)
        {
            if (transactionState == (int)TransactionState.NewTransaction && db != null)
            {
                db.AbortTransaction();
            }
        }


    }
}
