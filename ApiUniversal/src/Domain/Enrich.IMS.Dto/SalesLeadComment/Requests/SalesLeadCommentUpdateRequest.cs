using Enrich.Dto.Requests;

namespace Enrich.IMS.Dto.SalesLeadComment
{
    public class SalesLeadCommentUpdateRequest : BaseSaveByJsonPatchRequest<SalesLeadCommentDto>
    {
        public SalesLeadCommentUpdateOption UpdateOption { get; set; }
        public SalesLeadCommentUpdateRequest(SalesLeadCommentDto dto, SalesLeadCommentDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public SalesLeadCommentUpdateRequest()
        {
        }
    }

   
   
}
