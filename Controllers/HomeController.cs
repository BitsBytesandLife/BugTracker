using BugTracker.Data;
using BugTracker.Extentions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public HomeController(ILogger<HomeController> logger, 
                              ApplicationDbContext context, 
                              IBTProjectService projectService, 
                              IBTCompanyInfoService infoService,
                              IBTTicketService ticketService,
                              IBTRolesService rolesService,
                              UserManager<BTUser> userManager)
        {
            _logger = logger;
            _context = context;
            _projectService = projectService;
            _infoService = infoService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {


            int companyId = User.Identity.GetCompanyId().Value;
            BTUser currentUser = await _userManager.GetUserAsync(User);


            DashboardViewModel dashBoardViewModel = new()
            {

                Projects = await _projectService.ListUserProjectsAsync(currentUser.Id),
                Tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId),
                Users = await _infoService.GetAllMembersAsync(companyId),
                CurrentCompany = await _infoService.GetCompanyInfoByIdAsync(companyId)
            };

            return View(dashBoardViewModel);
            //DashboardViewModel dashBoard = new();

            //int companyId = User.Identity.GetCompanyId().Value;
            //BTUser currentUser = await _userManager.GetUserAsync(User);

            //List<Project> projects = await _projectService.ListUserProjectsAsync(currentUser.Id);
            //dashBoard.Projects = projects;

            //List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            //dashBoard.Tickets = tickets;

            //List<BTUser> users = await  _infoService.GetAllMembersAsync(companyId);

            //Company currentCompany = await _infoService.GetCompanyInfoByIdAsync(companyId);
            //dashBoard.CurrentCompany = currentCompany;

            //return View(dashBoard);
        }


        public IActionResult LandingPage()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<JsonResult> PieChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> DonutMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Random rnd = new();

            List<Project> projects = (await _projectService.GetAllProjectsByCompany(companyId)).OrderBy(p => p.Id).ToList();

            DonutViewModel chartData = new();
            chartData.labels = projects.Select(p => p.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> tickets = new();
            List<string> colors = new();

            foreach (Project prj in projects)
            {
                tickets.Add(prj.Tickets.Count());

                // This code will randomly select a color for each element of the data 
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = tickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> UserChart()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Random rnd = new();

            List<Project> projects = (await _projectService.GetAllProjectsByCompany(companyId)).OrderBy(p => p.Id).ToList();

            DonutViewModel chartData = new();
            chartData.labels = projects.Select(p => p.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> members = new();
            List<string> colors = new();

            foreach (Project prj in projects)
            {
                members.Add(prj.Members.Count());

                // This code will randomly select a color for each element of the data 
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = members.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }

    }
}
