using System;
using System.Data;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using Inner.Libs.Helpful.Infra;

namespace EnrichcousBackOffice.Services
{
    public abstract class IServicesBase: Disposable
    {
        public WebDataModel DB
        {
            get
            {
                _db = _db ?? new WebDataModel();
                return (WebDataModel) _db;
            }
        }
        protected static P_Member auth = Authority.GetCurrentMember();
        static IServicesBase() => Authority.observable["IServicesBase"] = _auth => auth = _auth;
        ~IServicesBase()
        {
            // Dispose();
        }
        protected IServicesBase(WebDataModel db) : base(db)
        {
            _db = db;
        }
        protected IServicesBase(bool trans = false, IsolationLevel _lockLevel = IsolationLevel.ReadUncommitted) : base(trans, _lockLevel)
        {
            BeginTransaction(DB, trans, _lockLevel);
        }
        public override void Dispose()
        {
            try
            {
                base.Dispose();
                GC.SuppressFinalize(this);
                auth = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected string MemberName => auth?.FullName ?? "IMS System";
        protected string MemberNumber => auth?.MemberNumber ?? "IMS System";
    }
}