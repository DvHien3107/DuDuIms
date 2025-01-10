using Enrich.Dto.Requests;

namespace Enrich.IMS.Dto.SalesLeadLog
{
    public class SalesLeadLogUpdateRequest : BaseSaveByJsonPatchRequest<SalesLeadLogDto>
    {
        public SalesLeadLogUpdateOption UpdateOption { get; set; }
        public SalesLeadLogUpdateRequest(SalesLeadLogDto dto, SalesLeadLogDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public SalesLeadLogUpdateRequest()
        {
        }
    }

   
   
}
