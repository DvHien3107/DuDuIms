using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.AppLB.Smartsheet
{
    public class SmartsheetHandle
    {
        // The API identifies columns by Id, but it's more convenient to refer to column names
        Dictionary<string, long> columnMap = new Dictionary<string, long>(); // Map from friendly column name to column Id 
        string Smartsheet_ACHSheetId = ConfigurationManager.AppSettings.Get("Smartsheet_ACHSheetId");
        string Smartsheet_Accesstoken = ConfigurationManager.AppSettings.Get("Smartsheet_Accesstoken");
        SmartsheetClient smartsheet = null;

        public string URLSheet { get; private set; }

        public SmartsheetHandle()
        {
            // Initialize client. Uses API access token from environment variable SMARTSHEET_ACCESS_TOKEN
            smartsheet = new SmartsheetBuilder()
                .SetAccessToken("ei92y5pm3trtas0zfpamr72fru")
                .Build();

        }

        /// <summary>
        /// add new record on ach sheet
        /// </summary>
        /// <returns></returns>
        public string AddNewRowACH(List<ACHModel> ach)
        {
            try
            {
                Sheet sheet = smartsheet.SheetResources.GetSheet(long.Parse(Smartsheet_ACHSheetId), null, null, null, null, null, null, null);
                //Console.WriteLine("Loaded " + sheet.Rows.Count + " rows from sheet: " + sheet.Name + " url:" + sheet.Permalink);
                URLSheet = sheet.Permalink;
                // Build column map for later reference
                foreach (Column column in sheet.Columns)
                    columnMap.Add(column.Title, (long)column.Id);

                List<Row> rowsToAddNew = new List<Row>();
                List<Discussion> listDiscustion = new List<Discussion>();
                foreach (var item in ach)
                {
                    rowsToAddNew.Add(AddNewRow(item, out Discussion discustion));
                    listDiscustion.Add(discustion);
                   

                }
                var rowsResult = smartsheet.SheetResources.RowResources.AddRows(sheet.Id.Value, rowsToAddNew);

                for (int i = 0; i < listDiscustion.Count; i++)
                {
                    if (listDiscustion[i] == null)
                    {
                        continue;
                    }
                    smartsheet.SheetResources.RowResources.DiscussionResources.CreateDiscussion(sheet.Id.Value, rowsResult[i].Id.Value, listDiscustion[i]);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }

        private Row AddNewRow(ACHModel ach, out Discussion discussion)
        {
            Row newrow = new Row();
            discussion = null;
            var cells = new List<Cell>();
            foreach (var item in columnMap)
            {
                switch (item.Key)
                {
                    case "Account #":
                        cells.Add(NewCell(item.Value, "#" + ach.AccountID));
                        break;
                    case "Routing Number":
                        cells.Add(NewCell(item.Value, ach.RoutingNum));
                        break;
                    case "Account Number":
                        cells.Add(NewCell(item.Value, ach.AccountNum));
                        break;
                    case "Amount":
                        cells.Add(NewCell(item.Value, ach.Amount));
                        break;
                    case "Type":
                        cells.Add(NewCell(item.Value, ach.Type));
                        break;
                    default:
                        break;
                }

            }

            if (!string.IsNullOrWhiteSpace(ach.Comment))
            {
               
                var discus = new Discussion
                {
                    Comment = new Comment
                    {
                        Text = ach.Comment
                    },
                    Comments = null
                };
                discussion = discus;
               
            }
            newrow.Cells = cells;
            newrow.ToTop = true;

            return newrow;
        }

        private Cell NewCell(long columnId, string value)
        {
            var newCell = new Cell
            {
                ColumnId = columnId,
                Value = value
            };

            return newCell;

        }


    }

    public class ACHModel
    {
        public string AccountID { get; set; }
        public string RoutingNum { get; set; }
        public string AccountNum { get; set; }
        public string Amount { get; set; }
        public string Type { get; set; }
        public string Comment { get; set; }
    }



}