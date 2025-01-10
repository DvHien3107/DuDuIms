using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public interface IItemState
    {
        int Id { get; set; }

        ItemStateType ItemState { get; set; }
    }


    public class ItemStateDto : IdDto, IItemState
    {
        public ItemStateType ItemState { get; set; } = ItemStateType.Unchanged;
    }
}
