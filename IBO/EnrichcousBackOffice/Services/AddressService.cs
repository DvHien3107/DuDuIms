using EnrichcousBackOffice.AppLB;
using System.Collections.Generic;
using EnrichcousBackOffice.Models.CustomizeModel;
using System.Linq;

namespace EnrichcousBackOffice.Services
{
    public partial class AddressService
    {
        public List<Provinces> GetProvinces()
        {
            ReadJson readJson = new ReadJson();
            var provinces = readJson.ReadDataFromFile<List<Provinces>>("/Content/Json_data/address/vn/provinces.json");
            return provinces;
        }

        public Provinces GetProvinceByCode(string code) => GetProvinces().Where(x => x.Code == code).FirstOrDefault();

        public List<Districts> GetDistrictsByProvinceId(int Province_Id)
        {
            if (Province_Id == 0)
            {
                return null;
            }
            ReadJson readJson = new ReadJson();
            var districts = readJson.ReadDataFromFile<List<Districts>>("/Content/Json_data/address/vn/districts.json").Where(x=>x.Province_Id== Province_Id).ToList();

            return districts;
        }
        public List<Wards> GetWardByDistrictId(int DistrictId)
        {
            ReadJson readJson = new ReadJson();
            var wards = readJson.ReadDataFromFile<List<Wards>>("/Content/Json_data/address/vn/wards.json").Where(x=>x.District_Id == DistrictId).ToList();
            return wards;
        }
    }
}