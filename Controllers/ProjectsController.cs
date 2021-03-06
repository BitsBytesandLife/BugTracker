using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Extentions;
using BugTracker.Models.ViewModels.Enums;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _infoService;
        private readonly UserManager<BTUser> _userManager;
        public ProjectsController(ApplicationDbContext context, IBTProjectService projectService, IBTCompanyInfoService infoService, UserManager<BTUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _infoService = infoService;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Project.Include(p => p.Company).Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Members)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.OwnerUser)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.DeveloperUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            BTUser currentUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;
            //ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name");

            if (User.IsInRole(Roles.Admin.ToString()) || User.IsInRole(Roles.ProjectManager.ToString()))
            {
                ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");
            }
            else
            {
                //ToDo: SweetAlet 
                //Only PM or Admin can make Project
                return RedirectToAction("Index");
                
            }
            
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFileData,ImageFileContentType,Archived")] Project project)

        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId")] Project project)
        {
            if (ModelState.IsValid)
            {
               BTUser currentUser = await _userManager.GetUserAsync(User);

                //int companyId = User.Identity.GetCompanyId().Value; 

                project.CompanyId = currentUser.CompanyId;
                project.Company = currentUser.Company;
               
                _context.Add(project);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Dashboard", "Home");
            }
            //ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFileData,ImageFileContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.FindAsync(id);
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize (Roles = "Admin, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignUsers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                    .FirstOrDefault(p => p.Id == id);

            model.Project = project;
            List<BTUser> developer = await _infoService.GetMembersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitter = await _infoService.GetMembersInRoleAsync(Roles.Submitter.ToString(), companyId);

            //List<BTUser> users = await _projectService.UsersNotOnProjectAsync(id, companyId);
            List<BTUser> users = developer.Concat(submitter).ToList();
            //List<BTUser> members = project.Members.ToList();
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUsers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id))
                                                .Select(m => m.Id).ToList();

                    foreach (string id in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                    }

                    foreach (string id in model.SelectedUsers)
                    {
                        await _projectService.AddProjectManagerAsync(id, model.Project.Id);
                    }
                    //goto project details
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    //send an error message
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AssignProjectManager(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                    .FirstOrDefault(p => p.Id == id);

            model.Project = project;
            List<BTUser> projectManager = await _infoService.GetMembersInRoleAsync(Roles.ProjectManager.ToString(), companyId);
          

            //List<BTUser> users = await _projectService.UsersNotOnProjectAsync(id, companyId);
            //List<BTUser> users = projectManager.ToList();
            //List<BTUser> members = project.Members.ToList();
           // List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new SelectList(projectManager, "Id", "FullName");

            return View(model);
        }





        [HttpGet]
        public async Task<IActionResult> RemoveUsers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;


            
          Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                    .FirstOrDefault(p => p.Id == id);




            model.Project = project;
            List<BTUser> developer = await _infoService.GetMembersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitter = await _infoService.GetMembersInRoleAsync(Roles.Submitter.ToString(), companyId);

            //List<BTUser> users = await _projectService.UsersNotOnProjectAsync(id, companyId);
            List<BTUser> users = developer.Concat(submitter).ToList();
            //List<BTUser> members = project.Members.ToList();
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUsers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id))
                                              .Select(m => m.Id).ToList();

                    foreach (string id in model.SelectedUsers)
                    {

                        
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                        
                    }
                    //goto project details
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    //send an error message
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> project = await _projectService.GetAllProjectsByCompany(companyId);

            return View(project);
        }


        public async Task<IActionResult> MyProjects() 
        {
            string currentUser = (await _userManager.GetUserAsync(User)).Id;
            List<Project> myProjects = await _projectService.ListUserProjectsAsync(currentUser);

            return View(myProjects);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int id)
        {
            AssignPMViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                                        .FirstOrDefault(p => p.Id == id);

            model.Project = project;

            List<BTUser> projectManager = await _infoService.GetMembersInRoleAsync(Roles.ProjectManager.ToString(), companyId);

            model.ProjectManagersSelectList = new SelectList(projectManager, "Id", "FullName");

            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {

            if (ModelState.IsValid)
            {
               await _projectService.AddProjectManagerAsync(model.ProjectManagerId , model.Project.Id);

                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }
            else 
            { 
                //Send an error maybe SweetAlert 
            }
            return View();
        }




        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
