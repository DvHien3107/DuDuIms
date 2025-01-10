using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Register.MangoVietnam.Models
{
    public class OldRegisterModel
    {

        public string Email { get; set; }
        public string Password { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string PhoneNumber { get; set; }
        public OldInfoSalonModel InfoSalon { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Verify { get; set; }
        public string ErrorMessage { get; set; }
        public bool InsertToDb { get; set; }
        public class OldInfoSalonModel
        {
            public string SalonName { get; set; }
            public string Province { get; set; }
            public string District { get; set; }
            public string Address { get; set; }
            public string NumberBranches { get; set; }
            public string NumberEmployees { get; set; }
            //bạn đã hoặc đang sử dụng phần mềm quản lý
            public string Question1 { get; set; }
            //phần cứng
            public string Question2 { get; set; }
            public string ServicePackage { get; set; }
        }
    }
}