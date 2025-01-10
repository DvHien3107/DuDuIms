using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Services.NextGen
{
    public class ConstProduct
    {
        public class Product
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Color { get; set; }
            public int OrderIndex { get; set; }
        }

        public static List<Product> gtListProducts()
        {
            // Your JSON array as a string
            List<Product> result = new List<Product>();
            Product newProduct = new Product
            {
                Id = "60b023cd-d8cd-4073-98b7-e471123c4fb9",
                Name = "Terminal ONLY",
                Color = "#b5bcc2",
                OrderIndex = 0
            };
            result.Add(newProduct);


            return result;
        }
    }
}
