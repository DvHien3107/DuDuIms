using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{

    //refer: https://github.com/JeremySkinner/FluentValidation/wiki/a.-Index
    public abstract class BaseEnrichValidator<T> : AbstractValidator<T>
    {
    }
}
