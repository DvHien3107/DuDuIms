using Enrich.Core;
using Enrich.Core.UnitOfWork.Data;
using Enrich.Entities;
using Enrich.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlProviderService _sqlProviderService;

        public EmailTemplateService(IUnitOfWork unitOfWork, ISqlProviderService sqlProviderService)
        {
            _unitOfWork = unitOfWork;
            _sqlProviderService = sqlProviderService;
        }

        public void Insert(EmailTemplate entity)
        {
            _unitOfWork.Repository<EmailTemplate>().Insert(entity);
            _unitOfWork.SaveChanges();
        }
        public void Update(EmailTemplate entity)
        {
            _unitOfWork.Repository<EmailTemplate>().Update(entity);
            _unitOfWork.SaveChanges();
        }

        public void Delete(EmailTemplate entity)
        {
            _unitOfWork.Repository<EmailTemplate>().Delete(entity);
            _unitOfWork.SaveChanges();
        }
        public List<EmailTemplate> GetAllEmailTemplate()
        {
            _unitOfWork.BeginTransaction();

            return _unitOfWork.Repository<EmailTemplate>().TableNoTracking.ToList();            
        }
    }
}
