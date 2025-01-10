using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.ViewControler
{
    public class ContractViewService
    {
        

        ///// <summary>
        /////tinh toan so tien thanh toan cu the cho tung product
        ///// </summary>
        ///// <param name="contrPaym">contract payment</param>
        ///// <param name="depositBalance">So tien con lai can duoc tinh vao product chua thanh toan: -1 break</param>
        ///// <param name="applyComm">tinh comm cho nhan vien</param>
        ///// <returns></returns>
        //public void PaymentDetailCalcAndComm(S_ContractPayment contrPaym, decimal depositBalance = -1, bool applyComm = false, DateTime? paidDate = null)
        //{
        //    WebDataModel db = new WebDataModel();
        //    using (var trans = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            List<S_ContractPaymentDetail> contrPaymDetailCanPaidCommList = new List<S_ContractPaymentDetail>();
        //            int maxContrPaymtDtId = db.S_ContractPaymentDetail.Count() == 0 ? 0 : db.S_ContractPaymentDetail.Max(d => d.Id);
        //            decimal remainingDeposit = depositBalance > 0 ? depositBalance : (contrPaym.Amount ?? 0);

        //            var contr = db.S_Contracts.Find(contrPaym.ContractId);
        //            decimal tax = ((contr.Tax ?? 0) * (contr.TotalApplyTax ?? 0)) / 100;


        //            #region check tax - uu tien tra tien thue dau tien
        //            if (tax > 0 && remainingDeposit > 0)
        //            {
        //                foreach (var item in db.S_ContractDetail.Where(d => d.ContractPurchaseTypeId == null && d.TotalRemainingAmount > 0 && d.ContractId == contr.Id).ToList())
        //                {

        //                    decimal paymentAmount = (item.TotalRemainingAmount ?? 0);
        //                    if (remainingDeposit < item.TotalRemainingAmount)
        //                    {
        //                        paymentAmount = remainingDeposit;
        //                    }
        //                    if (remainingDeposit > 0)
        //                    {
        //                        maxContrPaymtDtId++;
        //                        //update ContractPaymentDetail: chi tiet lich su thanh toan duoc ap dung cho contract payment.
        //                        S_ContractPaymentDetail contrPaymDt = new S_ContractPaymentDetail { Id = maxContrPaymtDtId, ContractPaymentId = contrPaym.Id, PaidAmount = paymentAmount, RemainingAmountAtNow = (item.TotalRemainingAmount ?? 0) - paymentAmount, ContractDetailId = item.Id };
        //                        db.S_ContractPaymentDetail.Add(contrPaymDt);

        //                        contrPaymDetailCanPaidCommList.Add(contrPaymDt);

        //                        S_ContractDetail cd = db.S_ContractDetail.Find(item.Id);
        //                        //tong tien khach da thanh toan duoc tinh cho contrpaymt nay
        //                        cd.TotalPaidAmount = (cd.TotalPaidAmount ?? 0) + paymentAmount;
        //                        //so tien con lai
        //                        cd.TotalRemainingAmount = (cd.TotalAmount ?? 0) - (cd.TotalPaidAmount ?? 0);
        //                        db.Entry(cd).State = EntityState.Modified;

        //                        remainingDeposit -= paymentAmount;
        //                    }


        //                }
        //            }

        //            #endregion

        //            #region check shipping
        //            if (remainingDeposit > 0)
        //            {
        //                var contrDetailShipping = (from d in db.S_ContractDetail
        //                                           join t in db.S_ContractPurchaseType on d.ContractPurchaseTypeId equals t.Id
        //                                           where d.ContractId == contr.Id && t.Shipping == true && d.TotalRemainingAmount > 0
        //                                           select d).ToList();
        //                foreach (var item in contrDetailShipping)
        //                {
        //                    decimal paymentAmount = (item.TotalRemainingAmount ?? 0);
        //                    if (remainingDeposit < item.TotalRemainingAmount)
        //                    {
        //                        paymentAmount = remainingDeposit;
        //                    }
        //                    if (remainingDeposit > 0)
        //                    {
        //                        maxContrPaymtDtId++;
        //                        S_ContractPaymentDetail contrPaymDt = new S_ContractPaymentDetail { Id = maxContrPaymtDtId, ContractPaymentId = contrPaym.Id, PaidAmount = paymentAmount, RemainingAmountAtNow = (item.TotalRemainingAmount ?? 0) - paymentAmount, ContractDetailId = item.Id };
        //                        db.S_ContractPaymentDetail.Add(contrPaymDt);

        //                        contrPaymDetailCanPaidCommList.Add(contrPaymDt);

        //                        S_ContractDetail cd = db.S_ContractDetail.Find(item.Id);
        //                        cd.TotalPaidAmount = (cd.TotalPaidAmount ?? 0) + paymentAmount;
        //                        cd.TotalRemainingAmount = (cd.TotalAmount ?? 0) - (cd.TotalPaidAmount ?? 0);
        //                        db.Entry(cd).State = EntityState.Modified;

        //                        remainingDeposit -= paymentAmount;

        //                    }


        //                }
        //            }

        //            #endregion


        //            #region tinh paid paidAmount cho cac contractdetail con lai

        //            if (remainingDeposit > 0)
        //            {
        //                var contrDetailOthers = (from d in db.S_ContractDetail
        //                                         join t in db.S_ContractPurchaseType on d.ContractPurchaseTypeId equals t.Id
        //                                         where d.ContractId == contr.Id && t.Shipping != true && d.TotalRemainingAmount > 0
        //                                         select d).ToList();
        //                foreach (var item in contrDetailOthers)
        //                {
        //                    if (remainingDeposit > 0)
        //                    {
        //                        maxContrPaymtDtId++;
        //                        decimal paymentAmount = (item.TotalRemainingAmount ?? 0);
        //                        if (remainingDeposit < item.TotalRemainingAmount)
        //                        {
        //                            paymentAmount = remainingDeposit;
        //                        }

        //                        S_ContractPaymentDetail contrPaymDt = new S_ContractPaymentDetail
        //                        {
        //                            Id = maxContrPaymtDtId,
        //                            PaidAmount = paymentAmount,
        //                            ContractDetailId = item.Id,
        //                            ContractPaymentId = contrPaym.Id,
        //                            RemainingAmountAtNow = (item.TotalRemainingAmount ?? 0) - paymentAmount
        //                        };
        //                        db.S_ContractPaymentDetail.Add(contrPaymDt);
        //                        contrPaymDetailCanPaidCommList.Add(contrPaymDt);

        //                        S_ContractDetail cd = db.S_ContractDetail.Find(item.Id);
        //                        cd.TotalPaidAmount = (cd.TotalPaidAmount ?? 0) + paymentAmount;
        //                        cd.TotalRemainingAmount = (cd.TotalAmount ?? 0) - (cd.TotalPaidAmount ?? 0);
        //                        db.Entry(cd).State = EntityState.Modified;

        //                        remainingDeposit -= paymentAmount;

        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }

        //                }
        //            }

        //            #endregion


        //            db.SaveChanges();
        //            trans.Commit();
        //            trans.Dispose();

        //            //applycontract
        //            if (applyComm == true)
        //            {
        //                foreach (var item in contrPaymDetailCanPaidCommList)
        //                {
        //                    StaffCommission(item, paidDate);
        //                }
        //            }

        //        }
        //        catch (Exception)
        //        {
        //            trans.Dispose();
        //            throw;
        //        }


                



        //    }


        //}



        #region kiem tra dieu kien nhan hoa hong


        /// <summary>
        /// kkiem tra dieu kien nhan comm theo co chem MLM phien ban 2.0 update 20190327
        /// ******************LUU Y *************************
        /// tu level 1- level 3(manager)
        /// cung chuc vu khong duoc nhan comm, tim kiem nv co level cao hon kiem tra, neu chuc vu cao hon thi tinh comm.
        /// tu level 3(manager) tro len
        /// neu 1 nguoi nao do dat level 3 duoc goi la break away : ho co van phong cua rieng ho(team cua ho) va bat dau tinh comm cho cac level cao hon nhu sau
        /// Neu nguoi level cao hon la Manager (cung chuc vu): huong xuong 1 level.
        /// tiep theo la director: co the huong xuong 2 level.
        /// tiep theo la VP: co the huong xuong 3 level.
        /// neu vuot qua level cho phep, se khong duoc huong.=> stop.
        /// ***TUC LA BAT DAU TU MANAGER CHI XET LEN 3 LEVEL.NEU NV THOA DIEU KIEN THI NHAN COMMISSION THEO CHUC VU DO. NGUOC LAI STOP.
        /// ***TRUONG HOP NGUOI LEVEL DUOI CO CHUC VU CAO HON NGUOI LEVEL TREN THI BO QUA NGAY NGUOI DO VA XET NGUOI TIEP THEO TRONG 3 LEVEL.
        /// ***TRUONG HOP TRONG TEAM KHONG CO MANAGER, NGUOI DUNG DAU LA DIRECTOR OR VP, NGUOI NAY VAN SE DUOC NHAN THEO %COMM THEO CHUC VU MANAGER TU TEAM CUA MINH
        /// ***TRUONG HOP NGUOI NAO DO NGHI VIEC, BO QUA NGUOI DO. MOI CO CHE KHONG THAY DOI.
        /// ***KHONG DUOC DOI TEAM
        /// *************************************************
        /// </summary>
        /// <param name="salesPerson">Nguoi mang ve hop dong</param>
        /// <param name="contrDate"></param>
        /// <param name="breakawayMemberNo">xac dinh nguoi manager cua team(co the level >= 3)</param>
        /// <returns></returns>
        public List<P_Member> CheckCommissionLevel(WebDataModel db, P_Member salesPerson, DateTime contrDate, out string breakawayMemberNo)
        {
            List<P_Member> listEmpReceiveComm = new List<P_Member>();
            breakawayMemberNo = "-1";
            var currentLevelPerson = salesPerson;
            int currentLevelPerson_levelNo = GetLevelNumber(db.P_MemberLevel.Where(m=>m.MemberNumber == salesPerson.MemberNumber).ToList(), contrDate);

            //kiem tra tinh hop le cua nguoi sales mang ve hop dong co thuoc MLM hay khong
            if (currentLevelPerson_levelNo > 0)
            {
                listEmpReceiveComm.Add(salesPerson);
            }
            else
            {
                //nguoi nay khong set level, nen khong ap dung co che commission MLM
                return listEmpReceiveComm;
            }


            //kiem tra level tu 1(distributor) -> 3(manager)
            if (currentLevelPerson_levelNo < 3)
            {
                //comm cho cac level trong team(office)
                //truong hop khong tim duoc nguoi manager, thi nguoi co chuc vu cao hon se nhan comm manager cua team do.
                bool find_loop = true;
                while (find_loop)
                {
                    var upLevelPerson = db.P_Member.Where(e => e.MemberNumber == currentLevelPerson.ReferedByNumber && e.MemberType.Equals("distributor")).FirstOrDefault();
                    int upLevelPerson_levelNo = GetLevelNumber(db.P_MemberLevel.Where(l=>l.MemberNumber == upLevelPerson.MemberNumber).ToList(), contrDate);


                    if (upLevelPerson == null || upLevelPerson_levelNo == -1)
                    {
                        return listEmpReceiveComm;
                    }
                    else if (upLevelPerson_levelNo > currentLevelPerson_levelNo)
                    {
                        //truong hop nv nay nghi viec(active = false) van dua vao list de dam bao co che. nhung se khong chi tra comm cho nguoi nay.

                        listEmpReceiveComm.Add(upLevelPerson);
                        if (upLevelPerson_levelNo >= 3)
                        {
                            //khi nguoi nay la MANAGER(OFFICE) HOAC CAO HON.
                            breakawayMemberNo = upLevelPerson.MemberNumber;
                            currentLevelPerson = upLevelPerson;
                            currentLevelPerson_levelNo = upLevelPerson_levelNo;
                            find_loop = false;
                        }
                    }
                    else
                    {
                        currentLevelPerson = upLevelPerson;
                        currentLevelPerson_levelNo = upLevelPerson_levelNo;

                        continue;
                    }

                }
            }


            //comm cho cac level cao hon manager ~ OFFICE.
            //neu van la manager huong xuong 1 cap.
            //neu la direct co the huong xuong 2 cap.
            //neu la vp co the huong xuong 3 cap.

            //=>>> theo co che: toi da co 3 cap de xet duyet tinh tu manager (Office).
            for (int i = 1; i <= 3; i++)
            {
                var upLevelPerson = db.P_Member.Where(e => e.MemberNumber == currentLevelPerson.ReferedByNumber).FirstOrDefault();
                int upLevelPerson_levelNo = GetLevelNumber(db.P_MemberLevel.Where(l => l.MemberNumber == upLevelPerson.MemberNumber).ToList(), contrDate);
                //xet dieu kien(1):
                //neu van la manager huong xuong 1 cap.
                //neu la direct co the huong xuong 2 cap.
                //neu la vp co the huong xuong 3 cap.
                //note: level cao hon co the co MLM level(chuc vu) ngang nhau, nhung phai thoa dk tren.

                if (upLevelPerson == null)
                {
                    return listEmpReceiveComm;
                }
                else if (upLevelPerson_levelNo >= currentLevelPerson_levelNo)
                {
                    if ((i == 1 && upLevelPerson_levelNo >= 3) || (i == 2 && upLevelPerson_levelNo >= 4) || upLevelPerson_levelNo == 5)
                    {
                        listEmpReceiveComm.Add(upLevelPerson);
                    }

                }
                //nguoi level cao hon co chuc vu(MLM Level) thap hon -> bo qua
                //hoac
                //khong thoa dieu kien(1).
                currentLevelPerson = upLevelPerson;
                currentLevelPerson_levelNo = upLevelPerson_levelNo;
                continue;

            }

            return listEmpReceiveComm;
        }


        /// <summary>
        /// get level number cua nv hieu luc vao thoi diem [contract date.]
        /// </summary>
        /// <param name="salesLevel"></param>
        /// <param name="contrDate"></param>
        /// <returns></returns>
        public int GetLevelNumber(List<P_MemberLevel> salesLevel, DateTime contrDate)
        {
            if (salesLevel == null)
            {
                return -1;
            }
            foreach (var item in salesLevel.OrderByDescending(l => l.EffectiveDate).ToList())
            {
                if (item.EffectiveDate > contrDate)
                {
                    continue;
                }
                else if (item.EffectiveDate <= contrDate)
                {
                    return item.LevelNumber ?? -1;
                }
                else
                {
                    return -1;
                }
            }

            return -1;

        }

        ///// <summary>
        ///// lay du lieu comm theo level phu hop voi ngay tao hop dong.
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="contrDate"></param>
        ///// <returns></returns>
        //public List<S_CommLevelSetting> GetLevelComm(WebDataModel db, DateTime contrDate)
        //{
        //    var listDate = (from l in db.S_CommLevelSetting
        //                    group l by l.EffectiveDate into lgroup
        //                    from d in lgroup.DefaultIfEmpty()
        //                    select new
        //                    {
        //                        Date = d.EffectiveDate
        //                    }).Distinct().OrderByDescending(l => l.Date).ToList();
        //    foreach (var item in listDate)
        //    {
        //        if (item.Date > contrDate)
        //        {
        //            continue;
        //        }
        //        else if (item.Date <= contrDate)
        //        {
        //            return db.S_CommLevelSetting.Where(l => l.EffectiveDate == item.Date).ToList();
        //        }
        //        else
        //        {
        //            return db.S_CommLevelSetting.Where(l => l.EffectiveDate == null).ToList();
        //        }
        //    }

        //    return new List<S_CommLevelSetting>();

        //}

     

        #endregion




        ///// <summary>
        /////Tinh comm sau khi da tinh toan paid paidAmount cho moi purchase type
        ///// </summary>
        ///// <param name="contrPaymDetail"></param>
        ///// <returns></returns>
        //public void StaffCommission(S_ContractPaymentDetail contrPaymDetail, DateTime? paidDate = null)
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();
        //        var contrDetail = (from d in db.S_ContractDetail
        //                           join con in db.S_Contracts on d.ContractId equals con.Id
        //                           where d.Id == contrPaymDetail.ContractDetailId
        //                           select new
        //                           {
        //                               ContractPurchaseTypeId = d.ContractPurchaseTypeId,
        //                               Portal_SalesPersonNumber = con.Portal_SalesPersonNumber
        //                           }).FirstOrDefault();

        //        if (contrDetail.ContractPurchaseTypeId != null)
        //        {
        //            ////check call center
        //            var salesPerson = SData.PortalEmployees.Where(e => e.MemberNumber == contrDetail.Portal_SalesPersonNumber).FirstOrDefault();

        //            if (salesPerson.CallCenter == true && salesPerson.CallCenterManager != true)
        //            {
        //                UpdateCommTeleMarketingMemberUpFront(contrPaymDetail, paidDate);
        //            }
        //            else
        //            {
        //                UpdateCommissionForMemberThenCustomerPay(contrPaymDetail, paidDate);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return;
        //    }
            

        //}



        ///// <summary>
        ///// Update commission for employee then customer pay
        ///// </summary>
        ///// <param name="contrPaymDetail"></param>
        ///// <returns></returns>
        //public void UpdateCommissionForMemberThenCustomerPay(S_ContractPaymentDetail contrPaymDetail, DateTime? paidDate = null)
        //{

        //    //update Employee Commistion 
        //    WebDataModel db = new WebDataModel();
        //    //using (var trans = db.Database.BeginTransaction())
        //    //{
        //    try
        //    {
        //        var sysInfo = db.S_SystemConfiguration.FirstOrDefault();
        //        var contrDetail = (from d in db.S_ContractDetail
        //                           join con in db.S_Contracts on d.ContractId equals con.Id
        //                           join cpt in db.S_ContractPurchaseType on d.ContractPurchaseTypeId equals cpt.Id
        //                           where d.Id == contrPaymDetail.ContractDetailId
        //                           select new
        //                           {
        //                               d.ContractId,
        //                               con.ContractDate,
        //                               con.CreatedAt,
        //                               d.ContractPurchaseTypeId,
        //                               con.Portal_SalesPersonNumber,
        //                               d.TotalAmount,
        //                               CustomerNumber = con.Portal_CustomerNumber,
        //                               d.ContractPurchaseTypeName,
        //                               ReferalMemberNumber = con.Portal_SalesReferalMemberNumber,
        //                               ReferalName = con.SalesReferalName,
        //                               Supporter_MemberNumber = con.Portal_SupportPersonNumber,
        //                               SupporterName = con.SupportPersonName

        //                           }).FirstOrDefault();
        //        //config
        //        string webcode = ConfigurationManager.AppSettings["webcode"];
        //        decimal paidAmount = contrPaymDetail.PaidAmount ?? 0;
        //        decimal paidPercent = (paidAmount * 100) / (contrDetail.TotalAmount ?? 0);
        //        decimal payMaxOut = sysInfo.MaxPayout ?? 0;
        //        IEnumerable<S_CommLevelSetting> commLevels = GetLevelComm(db, contrDetail.ContractDate ?? contrDetail.CreatedAt.Value);
        //        EmployeesSimpleModel salesPerson = SData.PortalEmployees.Where(x => x.MemberNumber == contrDetail.Portal_SalesPersonNumber).FirstOrDefault();
        //        DateTime contrDate = contrDetail.ContractDate == null ? contrDetail.CreatedAt.Value : contrDetail.ContractDate.Value;

        //        int breakawayMemberNo = -1;
        //        var levelCommValues = GetLevelComm(db, contrDate);
        //        var listMemberReceiveComm = CheckCommissionLevel(salesPerson, contrDate, out breakawayMemberNo);
        //        //

        //        if (levelCommValues.Count == 0)
        //        {
        //            return;
        //        }
        //        for (int i = 0; i < listMemberReceiveComm.Count; i++)
        //        {

        //            if (i == 0)
        //            {
        //                //xet nguoi truc tiep mang ve hop dong va doi tuong lien quan.
        //                decimal commPercent = 0;
        //                commPercent = levelCommValues.Where(l => l.Portal_LevelNumber == listMemberReceiveComm[i].LevelNumber).Select(l => l.CommPercent_Directly).FirstOrDefault() ?? 0;
        //                decimal totalComm = Math.Round(((((paidAmount) * payMaxOut) / 100) * commPercent) / 100, 2);

        //                #region for supporter(if)

        //                string commentsp = string.Empty;

        //                if (contrDetail.Supporter_MemberNumber > 0)
        //                {
        //                    decimal commPercentSupporter = sysInfo.Comm_PerTotalCommissionAmountForContractSupporter ?? 0;
        //                    if (commPercentSupporter > 0)
        //                    {

        //                        decimal commAmountForSupporter = Math.Round((totalComm * (sysInfo.Comm_PerTotalCommissionAmountForContractSupporter ?? 0)) / 100, 2);
        //                        CommViewModel empCommSupporter = new CommViewModel
        //                        {
        //                            ContractPaymentDetailId = contrPaymDetail.Id,
        //                            Recipient_MemberNumber = contrDetail.Supporter_MemberNumber,
        //                            RecipientName = contrDetail.SupporterName,
        //                            SalesPersonName = salesPerson.FullName,
        //                            ContractId = contrDetail.ContractId ?? 0,
        //                            Comment = "Commission for supporter (" + commPercentSupporter + " % from sales)",
        //                            CommissionAmount = commAmountForSupporter,

        //                            CustomerPaidAmount = paidAmount,
        //                            DateEarned = paidDate == null ? DateTime.Now : paidDate,
        //                            ReadyToPay = true,
        //                            CustomerNumber = contrDetail.CustomerNumber,
        //                            Product = contrDetail.ContractPurchaseTypeName,
        //                            WebCode = webcode
        //                        };
        //                        Dictionary<string, object> resultSupporter = StevePortalService.CallSteveRESTApi("/api/commission", "post", empCommSupporter);
        //                        if (resultSupporter["Result"].ToString() != "1")
        //                        {
        //                            throw new Exception(resultSupporter["Msg"].ToString());
        //                        }

        //                        totalComm = totalComm - commAmountForSupporter;
        //                        commentsp = " (" + commPercentSupporter + "% commission has been shared for supporter)";
        //                    }
        //                }

        //                #endregion

        //                #region comm for sales - directly

        //                CommViewModel empComm = new CommViewModel
        //                {
        //                    ContractPaymentDetailId = contrPaymDetail.Id,
        //                    Recipient_MemberNumber = salesPerson.MemberNumber,
        //                    RecipientName = salesPerson.FullName,
        //                    ContractId = contrDetail.ContractId ?? 0,
        //                    SalesPersonName = salesPerson.FullName
        //                };

        //                if (commPercent > 0)
        //                {
        //                    empComm.Comment = "Commission for member";
        //                }
        //                else
        //                {
        //                    empComm.Comment = "Bonus for member";
        //                }
        //                empComm.CommissionAmount = totalComm;
        //                empComm.Comment = empComm.Comment + " - " + commentsp;

        //                empComm.CustomerPaidAmount = contrPaymDetail.PaidAmount;
        //                empComm.DateEarned = paidDate == null ? DateTime.Now : paidDate;
        //                empComm.ReadyToPay = true;
        //                empComm.DirectComm = true;
        //                empComm.CustomerNumber = contrDetail.CustomerNumber;
        //                empComm.Product = contrDetail.ContractPurchaseTypeName;
        //                empComm.WebCode = webcode;
        //                Dictionary<string, object> result = StevePortalService.CallSteveRESTApi("/api/commission", "post", empComm);
        //                if (result["Result"].ToString() != "1")
        //                {
        //                    throw new Exception(result["Msg"].ToString());
        //                }


        //                #endregion

        //                #region kiem tra nguoi gioi thieu hop dong neu co.
        //                //
        //                if (contrDetail.ReferalMemberNumber > 0)
        //                {
        //                    var commPercentForReferal = sysInfo.Comm_ForContractReferral ?? 0;
        //                    if (commPercentForReferal > 0)
        //                    {

        //                        CommViewModel empCommForReferal = new CommViewModel();
        //                        empCommForReferal.ContractPaymentDetailId = contrPaymDetail.Id;
        //                        empCommForReferal.Recipient_MemberNumber = contrDetail.Supporter_MemberNumber;
        //                        empCommForReferal.RecipientName = contrDetail.SupporterName;
        //                        empCommForReferal.SalesPersonName = salesPerson.FullName;
        //                        empCommForReferal.ContractId = contrDetail.ContractId ?? 0;
        //                        empCommForReferal.Comment = "Commission for referal contract person (" + commPercentForReferal + "%)";
        //                        empCommForReferal.CommissionAmount = Math.Round((((paidAmount * payMaxOut) / 100) * commPercentForReferal) / 100, 2);

        //                        empCommForReferal.CustomerPaidAmount = paidAmount;
        //                        empCommForReferal.DateEarned = paidDate == null ? DateTime.Now : paidDate;
        //                        empCommForReferal.ReadyToPay = true;
        //                        empCommForReferal.CustomerNumber = contrDetail.CustomerNumber;
        //                        empCommForReferal.Product = contrDetail.ContractPurchaseTypeName;
        //                        empCommForReferal.WebCode = webcode;
        //                        Dictionary<string, object> resultReferal = StevePortalService.CallSteveRESTApi("/api/commission", "post", empCommForReferal);
        //                        if (resultReferal["Result"].ToString() != "1")
        //                        {
        //                            throw new Exception(resultReferal["Msg"].ToString());
        //                        }

        //                    }


        //                }
        //                #endregion

        //            }
        //            else
        //            {
        //                //bat dau xet nguoi co level cao hon
        //                //
        //                // lam sao de biet rang duoi nguoi nay la office/manager.
        //                //i > 3, chac chan level nay tu manager tro len.
        //                //vi nv trong 1 office dieu kien nhan comm phai co level number cao hon nguoi duoi ho.
        //                //=> den level thu 3. it nhat nguoi do la manager or cao hon neu team nay khong co manager
        //                //do vay bat dau tu level thu 4 tro di(neu co), ho se nhan comm percent office, chu khong phai la comm percent team.
        //                //

        //                decimal commPercent = 0;
        //                if (listMemberReceiveComm[i].MemberNumber == breakawayMemberNo)
        //                {
        //                    commPercent = levelCommValues.Where(l => l.Portal_LevelNumber == listMemberReceiveComm[i].LevelNumber).Select(l => l.CommPercent_ManagementOffice).FirstOrDefault() ?? 0;
        //                }
        //                else
        //                {
        //                    commPercent = levelCommValues.Where(l => l.Portal_LevelNumber == listMemberReceiveComm[i].LevelNumber).Select(l => l.CommPercent).FirstOrDefault() ?? 0;

        //                }

        //                decimal commAmount = Math.Round((((paidAmount * payMaxOut) / 100) * commPercent) / 100, 2);

        //                CommViewModel empComm = new CommViewModel();
        //                empComm.ContractPaymentDetailId = contrPaymDetail.Id;
        //                empComm.Recipient_MemberNumber = listMemberReceiveComm[i].MemberNumber;
        //                empComm.RecipientName = listMemberReceiveComm[i].FullName;
        //                empComm.SalesPersonName = salesPerson.FullName;
        //                empComm.ContractId = contrDetail.ContractId ?? 0;
        //                empComm.CommissionAmount = commAmount;
        //                empComm.Comment = "Commission for member";
        //                empComm.CustomerPaidAmount = contrPaymDetail.PaidAmount;
        //                empComm.DateEarned = paidDate == null ? DateTime.Now : paidDate;
        //                empComm.ReadyToPay = true;
        //                empComm.CustomerNumber = contrDetail.CustomerNumber;
        //                empComm.Product = contrDetail.ContractPurchaseTypeName;
        //                empComm.WebCode = webcode;
        //                Dictionary<string, object> result = StevePortalService.CallSteveRESTApi("/api/commission", "post", empComm);
        //                if (result["Result"].ToString() != "1")
        //                {
        //                    throw new Exception(result["Msg"].ToString());
        //                }



        //            }

        //        }

        //        //db.SaveChanges();
        //        // trans.Commit();
        //        // trans.Dispose();

        //        return;

        //    }
        //    catch (Exception)
        //    {
        //        // trans.Dispose();
        //        throw;
        //    }
        //    //}


        //}
        

    }


    public class CommissionLevelModel
    {
        public int Level { get; set; }
        public decimal CommPercent { get; set; }
        public decimal CommPercentForMonthlyFee { get; set; }
        public decimal Bonus { get; set; }
    }



}