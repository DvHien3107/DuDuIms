using Enrich.Dto.Base.Parameters;
using Enrich.Dto.Parameters;
using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Interface.Mappers
{
    /// <summary>
    /// Defines a simple interface to map objects.
    /// </summary>
    public interface IBaseMapper
    {

        object GetMapEngine();

        Action<object, object> BeforeMap { set; get; }
        Action<object, object> AfterMap { set; get; }

        /// <summary>
        /// Converts an object to another. Creates a new object of <see cref="TDestination"/>.
        /// </summary>
        /// <typeparam name="TDestination">Type of the destination object</typeparam>
        /// <param name="source">Source object</param>
        TDestination Map<TDestination>(object source);
        TDestination Map<TDestination>(object source, Action<TDestination> after = null);
        TDestination Map<TDestination>(object source, Action<object, object> beforeMap, Action<object, object> afterMap);


        /// <summary>
        /// Execute a mapping from the source object to the existing destination object
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="destination">Destination object</param>
        /// <returns>Returns the same <see cref="destination"/> object after mapping operation</returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

        QueryParameter CreateSqlQueryParameter(QueryPagingRequest queryRequest);

        FindByIdParameter CreateFindByIdParameter(FindByIdRequest findByIdRequest);

        QueryParameter CreateSearchSqlQueryParameter<TItem>(BaseSearchRequest request, string aliasFieldId = "Id");
    }
}
