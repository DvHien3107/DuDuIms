using DataTables.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace EnrichcousBackOffice.AppLB
{
    public static class DataTablesExtensions
    {
        public static IQueryable<T> SortAndPage<T>(this IQueryable<T> source, IDataTablesRequest request)
        {
            return source.OrderByDataTable(request.Columns).Page(request);
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> source, IDataTablesRequest request)
        {
            return source.Skip(request.Start).Take(request.Length);
        }

        public static IQueryable<T> OrderByDataTable<T>(this IQueryable<T> source, IEnumerable<IColumn> sortModels)
        {
            var expression = source.Expression;
            var count = 0;
            foreach (var item in sortModels.Where(x => x.Sort != null).OrderBy(x => x.Sort.Order))
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var selector = Expression.PropertyOrField(parameter, item.Name);
                var method = item.Sort.Direction == SortDirection.Descending ?
                    (count == 0 ? nameof(Queryable.OrderByDescending) : nameof(Queryable.ThenByDescending)) :
                    (count == 0 ? nameof(Queryable.OrderBy) : nameof(Queryable.ThenBy));
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }
            return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
        }
    }
}