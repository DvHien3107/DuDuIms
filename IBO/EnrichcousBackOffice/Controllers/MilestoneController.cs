using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MilestoneController : Controller
    {
        /// <summary>
        /// Get init data
        /// </summary>
        /// <returns></returns>
        public JsonResult InitMilestone()
        {
            return Json(MilestoneServices.ProjectMilestoneInfo());
        }

        /// <summary>
        /// SaveMilestone
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public JsonResult SaveMilestone(MilestoneRequest request)
        {
            Dictionary<string, ProjectMilestone> result = new Dictionary<string, ProjectMilestone>();
            var msg = "";
            try
            {
                using (WebDataModel db = new WebDataModel())
                {
                    if (request.ProjectInfo == null || string.IsNullOrEmpty(request.ProjectInfo.Id ?? ""))
                    {
                        throw new Exception("Please select your Project!");
                    }
                    // Group
                    T_Project_Milestone group = db.T_Project_Milestone.Find(request.ProjectInfo.Id) ?? new T_Project_Milestone();
                    group.Id = group.Id ?? MilestoneServices.MakeId(request.ProjectInfo.Id.MD5Hash());
                    group.Name = request.ProjectInfo.Name;
                    group.Type = MilestoneServices.PROJECT;
                    // Merger update
                    db.T_Project_Milestone.AddOrUpdate(group);
                    // Make response
                    result.Add("project", new ProjectMilestone {
                        Id = group.Id,
                        Name = group.Name
                    });

                    // Category
                    if (request.CategoryInfo != null)
                    {
                        if (string.IsNullOrEmpty(request.CategoryInfo.Id ?? "")) {
                            throw new Exception("Please select your Category!");
                        }
                        T_Project_Milestone category = db.T_Project_Milestone.Where(ctr => ctr.Id == request.CategoryInfo.Id)
                            .FirstOrDefault(ctr => ctr.ParentId == group.Id) ?? new T_Project_Milestone();
                        T_Project_Milestone categoryDuplicate = db.T_Project_Milestone.FirstOrDefault(vs => vs.Type == MilestoneServices.CATEGORY && vs.Name == request.CategoryInfo.Name);
                        if (category.Name == request.CategoryInfo.Name && string.IsNullOrEmpty(category.Name) == false || categoryDuplicate != null) {
                            throw new Exception("Category already exits!");
                        }
                        msg = $"Category \"{request.CategoryInfo.Name}\" has been {(string.IsNullOrEmpty(category.Id) ? "created" : "updated")}!";
                        category.Id = category.Id ?? MilestoneServices.MakeId(group.Id, request.CategoryInfo.Id);
                        category.Name = request.CategoryInfo.Name;
                        category.Type = MilestoneServices.CATEGORY;
                        category.ParentId = group.Id;
                        category.ParentName = group.Name;
                        // Merger update
                        db.T_Project_Milestone.AddOrUpdate(category);
                        // Make response
                        result.Add("category", new ProjectMilestone {
                            Id = category.Id,
                            Name = category.Name
                        });
                    }
                    
                    // Version
                    if (request.VersionInfo != null && request.VersionInfo.Id != null)
                    {
                        if (string.IsNullOrEmpty(request.VersionInfo.Id ?? ""))
                        {
                            throw new Exception("Please select your Version!");
                        }
                        T_Project_Milestone version = db.T_Project_Milestone.Where(vs => vs.Id == request.VersionInfo.Id)
                            .FirstOrDefault(vs => vs.ParentId == group.Id) ?? new T_Project_Milestone();
                        T_Project_Milestone versionDuplicate = db.T_Project_Milestone.FirstOrDefault(vs => vs.Type == MilestoneServices.VERSION && vs.Name == request.VersionInfo.Name);
                        if (version.Name == request.VersionInfo.Name && string.IsNullOrEmpty(version.Name) == false || versionDuplicate != null)  {
                            throw new Exception("Version already exits!");
                        }
                        msg = $"Version \"{request.VersionInfo.Name}\" has been {(string.IsNullOrEmpty(version.Id) ? "created" : "updated")} !";
                        version.Id = version.Id ?? MilestoneServices.MakeId(group.Id, request.VersionInfo.Id);
                        version.Name = request.VersionInfo.Name;
                        version.Type = MilestoneServices.VERSION;
                        version.ParentId = group.Id;
                        version.ParentName = group.Name;
                        // Merger update
                        db.T_Project_Milestone.AddOrUpdate(version);
                        // Make response
                        result.Add("version", new ProjectMilestone
                        {
                            Id = version.Id,
                            Name = version.Name
                        });
                    }
                    // Save
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return new Func.JsonStatusResult("Process fail : " + e.Message, HttpStatusCode.ExpectationFailed);
            }
            return Json(new Dictionary<string,object>{
                { "message" , msg },
                { "rs-save" , result },
                { "refresh", MilestoneServices.ProjectMilestoneInfo() }
            });
        }

        public JsonResult DeleteMilestone(ProjectMilestone CategoryInfo)
        {

            try
            {
                using (WebDataModel db = new WebDataModel())
                {
                    var category = db.T_Project_Milestone.Find(CategoryInfo.Id);
                    if (category == null)
                    {
                        return Json(new { status = false, message = "category not found !" });
                    }
                    db.T_Project_Milestone.Remove(category);
                    db.SaveChanges();
                    return Json(new { status = true, message = "delete category success !" });
                }



            }
            catch (Exception e)
            {
                return Json(new { status = false, message = "delete category fail !" });
            }

        }
        /// <summary>
        /// SaveMilestone
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public JsonResult NewProject(MilestoneRequest request)
        {
            ProjectMilestone result;
            try
            {
                result = MilestoneServices.ModifiedProject(request);
            }
            catch (Exception e)
            {
                return new Func.JsonStatusResult("Process fail : " + e.Message, HttpStatusCode.ExpectationFailed);
            }
            return Json(result);
        }
    }
}