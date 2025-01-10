using Enrich.Core.Infrastructure.Repository;
using Enrich.Core.Infrastructure.Services;

namespace Enrich.Services.Base
{
    public class GenericService<T> : IGenericService<T>
    {
        protected readonly IGenericRepository<T> _repositoryGeneric;

        protected GenericService(IGenericRepository<T> repositoryGeneric)
        {
            _repositoryGeneric = repositoryGeneric;
        }
    }
}
