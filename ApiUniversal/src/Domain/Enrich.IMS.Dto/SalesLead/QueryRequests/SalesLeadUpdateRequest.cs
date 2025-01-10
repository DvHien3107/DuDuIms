using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadUpdateRequest : BaseSaveByJsonPatchRequest<SalesLeadDto>
    {
        /// <summary>
        /// use to verify a salesLead (unverify to verify)
        /// </summary>
        public bool IsVerify { get; set; } = false;
        public SalesLeadUpdateOption UpdateOption { get; set; }

        public SalesLeadUpdateRequest(SalesLeadDto dto, SalesLeadDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public SalesLeadUpdateRequest()
        {
        }
    }   
}
