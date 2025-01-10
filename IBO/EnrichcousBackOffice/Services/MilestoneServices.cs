using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Services
{
    public class MilestoneRequest
    {
        public string Group { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Stage { get; set; }
        public string Process { get; set; }
        public ProjectMilestone ProjectInfo { get; set; }
        public ProjectMilestone CategoryInfo { get; set; }
        public ProjectMilestone VersionInfo { get; set; }
    }
    
    public class ProjectMilestone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ProjectMilestone> Categories { get; set; }
        public List<ProjectMilestone> Versions { get; set; }
    }

    public class MilestoneServices
    {
        public static string PROJECT = ProjectType.PROJECT.Text();
        public static string CATEGORY = ProjectType.CATEGORY.Text();
        public static string VERSION = ProjectType.VERSION.Text();
        /// <summary>
        /// Init ProjectMilestoneInfo
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ProjectMilestone> ProjectMilestoneInfo()
        {
            Dictionary<string, ProjectMilestone> milestoneInfo = new Dictionary<string,ProjectMilestone>();
            using (WebDataModel db = new WebDataModel())
            {
                var stageMilestone = db.T_Project_Milestone;
                stageMilestone.Where(pj => pj.Type == PROJECT).Where(pj => "".Equals(pj.ParentId.Trim() ?? "")).OrderBy(pj => pj.Name).ForEach(_group => {
                    ProjectMilestone group = new ProjectMilestone();
                    group.Id = _group.Id;
                    group.Name = _group.Name;
                    // Select categories
                    List<ProjectMilestone> categories = new List<ProjectMilestone>();
                    stageMilestone.Where(pj => pj.Type == CATEGORY)
                        .Where(pj => pj.ParentId == _group.Id).OrderBy(pj => pj.Name).ForEach(category => {
                            categories.Add(new ProjectMilestone {
                                Id = category.Id,
                                Name = category.Name
                            });
                        });

                    // Select Version
                    List<ProjectMilestone> versions = new List<ProjectMilestone>();
                    stageMilestone.Where(pj => pj.Type == VERSION)
                        .Where(pj => pj.ParentId == _group.Id).OrderBy(pj => pj.Name).ForEach(version => {
                            versions.Add(new ProjectMilestone {
                                Id = version.Id,
                                Name = version.Name
                            });
                        });
                    group.Categories = categories;
                    group.Versions = versions;
                    milestoneInfo.Add(group.Id, group);
                });
            }
            return milestoneInfo;
        }

        /// <summary>
        /// SaveMilestone
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ProjectMilestone ModifiedProject(MilestoneRequest request)
        {
            ProjectMilestone result;
            using (WebDataModel db = new WebDataModel())
            {
                if (request.ProjectInfo == null || string.IsNullOrEmpty(request.ProjectInfo.Id ?? ""))
                {
                    throw new Exception("Please select your Project!");
                }

                if (db.T_Project_Milestone.Where(pj => pj.Name == request.ProjectInfo.Name).ToList().Count > 0)
                {
                    throw new Exception("Project already exits!");
                }

                // Group
                T_Project_Milestone group = db.T_Project_Milestone.Find(request.ProjectInfo.Id) ?? new T_Project_Milestone();
                group.Id = group.Id ?? MakeId(request.ProjectInfo.Id.MD5Hash());
                group.Name = request.ProjectInfo.Name;
                group.Type = PROJECT;
                // Merger update
                db.T_Project_Milestone.AddOrUpdate(group);
                // Make response
                result = new ProjectMilestone
                {
                    Id = group.Id,
                    Name = group.Name
                };
                // Save
                db.SaveChanges();
            }
            return result;
        }


        /// <summary>
        /// MakeHashId 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static string MakeId(params string[]keys)
        {
            string defaultKey = DateTime.Now.ToString("O");
            try
            {
                return String.Join("", new[] {DateTime.Now.ToString("O"), String.Join("", keys) }).MD5Hash();
            }
            catch (Exception)
            {
                return defaultKey.MD5Hash();
            }
        }

        /// <summary>
        /// Update MileStone info
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="Request"></param>
        /// <param name="_db"></param>
        public static void UpdateTicketMileStone(T_SupportTicket ticket, HttpRequestBase Request, WebDataModel _db = null)
        {
            using (WebDataModel db = new WebDataModel())
            {
                // Set project
                ticket.CategoryId = null;
                ticket.CategoryName = null;
                if (!string.IsNullOrEmpty(Request["project"]))
                {
                    ticket.CategoryId = Request["project"].ToString();
                    ticket.CategoryName = db.T_Project_Milestone.Find(ticket.CategoryId)?.Name;
                }

                // Set affected version/milestone
                ticket.MilestoneId = null;
                ticket.MilestoneName = null;
                if (!string.IsNullOrEmpty(Request["affected-milestone"]))
                {
                    ticket.MilestoneId = Request["affected-milestone"].ToString();
                    ticket.MilestoneName = db.T_Project_Milestone.Find(ticket.MilestoneId)?.Name;
                }

                // Set fixed version/milestone
                ticket.FixedMilestoneId = null;
                ticket.FixedMilestoneName = null;
                if (!string.IsNullOrEmpty(Request["fixed-milestone"]))
                {
                    ticket.FixedMilestoneId = Request["fixed-milestone"].ToString();
                    ticket.FixedMilestoneName = db.T_Project_Milestone.Find(ticket.FixedMilestoneId)?.Name;
                }
            }
        }
    }
}