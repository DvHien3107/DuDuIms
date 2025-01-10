using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.ClickUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Services.NextGen
{
    public static class SyncClickUp
    {
        public static string SyncClickUpRequest(string OrderCode)
        {
            WebDataModel db = new WebDataModel();

            GetSyncClickUpData GetSyncClickUpData = db.Database.SqlQuery<GetSyncClickUpData>($"exec P_GetSyncClickUpData '{OrderCode}'").FirstOrDefault();
            string x = "{"
                +"\"name\": \""+ GetSyncClickUpData.CustomerName + "\","
                +"\"description\": \"Deploy ticket create by Ims v2\","
                +"\"assignees\": null,"
                +"\"parent\": null,"
                +"\"links_to\": null,"
                +"\"custom_fields\": ["
                    +"{"
                        +"\"id\": \"acac5f6b-1e79-4f06-b680-9ba55ada2251\","
                        +"\"value\": \""+ GetSyncClickUpData.OwnerEmail + "\""
                    +"}," 
                    + "{"
                        + "\"id\": \"4aaaed4f-7640-407d-96a7-8f8ed42a4c33\","
                        + "\"value\": \"" + GetSyncClickUpData.ActivatedLicense + "\""
                    + "},"
                    //+ "{"
                    //    +"\"id\": \"1f31bba7-3474-47b8-9bfa-973db0f36357\","
                    //    +"\"value\": \""+ GetSyncClickUpData.OwnerPhone + "\""
                    //+ "},"
                    //+ "{"
                    //    + "\"id\": \"24f28e8c-7e70-46c2-8e83-208c976d2675\","
                    //    + "\"value\": \"" + GetSyncClickUpData.BusinessPhone + "\""
                    //+ "},"
                    + "{"
                        + "\"id\": \"f7114869-3621-4c44-90bc-3d01c62726f1\","
                        + "\"value\": \"" + GetSyncClickUpData.MangoLoginEmail + "\""
                    + "},"
                    + "{"
                        + "\"id\": \"81e90c56-c54f-4d37-8b98-ade0a9204e8d\","
                        + "\"value\": \"" + GetSyncClickUpData.GoLiveDate + "\""
                    + "},"
                    + "{"
                        + "\"id\": \"a59c6ec3-601d-456b-b807-e31d71b2eea7\","
                        + "\"value\": \"" + GetSyncClickUpData.SalonNote + "\""
                    + "},"
                    + "{"
                        + "\"id\": \"b68d7fd1-05c4-447b-aeac-08ba32ae5224\","
                        + "\"value\": \"" + GetSyncClickUpData.OwnerName + "\""
                    + "}"
                    + "]"
                +"}";
            return x;
        }

    }

   

}
