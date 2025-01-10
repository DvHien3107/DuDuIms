using Pos.Application.Services.Scoped;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Table.POS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Event
{
    public class StoreChangeEvent
    {
        private readonly IPOSService _posService;
        public StoreChangeEvent(IPOSService posService)
        {
            _posService = posService;
        }
       
    }
}
