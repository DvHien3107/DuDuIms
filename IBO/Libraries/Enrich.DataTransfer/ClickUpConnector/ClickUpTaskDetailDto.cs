using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.ClickUpConnector
{

	public class ClickUpTaskDetailDto
	{
		public string id { get; set; }
		public object custom_id { get; set; }
		public string name { get; set; }
		public object text_content { get; set; }
		public object description { get; set; }
		public ClickUpTaskStatus status { get; set; }
		public string orderindex { get; set; }
		public string date_created { get; set; }
		public string date_updated { get; set; }
		public object date_closed { get; set; }
		public object date_done { get; set; }
		public bool? archived { get; set; }
		public ClickUpTaskCreator creator { get; set; }
		public List<object> assignees { get; set; }
		public List<ClickUpTaskWatcher> watchers { get; set; }
		public List<object> checklists { get; set; }
		public List<object> tags { get; set; }
		public object parent { get; set; }
		public ClickUpTaskPriority priority { get; set; }
		public string due_date { get; set; }
		public object start_date { get; set; }
		public object points { get; set; }
		public object time_estimate { get; set; }
		public int? time_spent { get; set; }
		public List<ClickUpTaskCustomField> custom_fields { get; set; }
		public List<object> dependencies { get; set; }
		public List<object> linked_tasks { get; set; }
		public string team_id { get; set; }
		public string url { get; set; }
		public ClickUpTaskSharing sharing { get; set; }
		public string permission_level { get; set; }
		public List<object> attachments { get; set; }
		public List<object> comments { get; set; }
		public List<object> subtasks { get; set; }
		public List<object> coverimage { get; set; }
	}

	public class ClickUpTaskStatus
	{
		public string id { get; set; }
		public string status { get; set; }
		public string color { get; set; }
		public int? orderindex { get; set; }
		public string type { get; set; }
	}

	public class ClickUpTaskCreator
	{
		public int? id { get; set; }
		public string username { get; set; }
		public string color { get; set; }
		public string email { get; set; }
		public object profilePicture { get; set; }
	}

	public class ClickUpTaskWatcher
	{
		public int? id { get; set; }
		public string username { get; set; }
		public string color { get; set; }
		public string initials { get; set; }
		public string email { get; set; }
		public object profilePicture { get; set; }
	}

	public class ClickUpTaskOption
	{
		public string id { get; set; }
		public string name { get; set; }
		public string color { get; set; }
		public int orderindex { get; set; }
	}

	public class ClickUpTaskTypeConfig
	{
		public object @default { get; set; }
		public object placeholder { get; set; }
		public List<ClickUpTaskOption> options { get; set; }
	}

	public class ClickUpTaskCustomField
	{
		public string id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public ClickUpTaskTypeConfig type_config { get; set; }
		public string date_created { get; set; }
		public bool? hide_from_guests { get; set; }
		public object value { get; set; }
		public object required { get; set; }
	}

	public class ClickUpTaskPriority
	{
		public string color { get; set; }
		public string id { get; set; }
		public string orderindex { get; set; }
		public string priority { get; set; }
	}


	public class ClickUpTaskSharing
	{
		public bool? public_share_expires_on { get; set; }
		public List<string> public_fields { get; set; }
		public object token { get; set; }
		public bool? seo_optimized { get; set; }
	}
}
