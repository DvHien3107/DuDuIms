using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.ClickUpConnector
{
	public class ClickUpFieldDto
	{
		public string id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public ClickUpFieldTypeConfig type_config { get; set; }
		public string date_created { get; set; }
		public bool? hide_from_guests { get; set; }
		public object required { get; set; }
	}
	public class ClickUpFieldOption
	{
		public string id { get; set; }
		public string name { get; set; }
		public string color { get; set; }
		public int? orderindex { get; set; }
	}

	public class ClickUpFieldTypeConfig
	{
		public int? @default { get; set; }
		public object placeholder { get; set; }
		public List<ClickUpFieldOption> options { get; set; }
	}
}
