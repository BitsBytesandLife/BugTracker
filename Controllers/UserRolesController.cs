using BugTracker.Data;
using BugTracker.Extentions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
   // [Authorize(Roles="Admin")]
    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _infoService;

        public UserRolesController(ApplicationDbContext context,
                                   UserManager<BTUser> userManager,
                                   IBTRolesService rolesService, IBTCompanyInfoService infoService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
            _infoService = infoService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUsersRolesViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            //TODO: Company Users
            //List<BTUser> users = _context.Users.ToList();
            List<BTUser> users = await _infoService.GetAllMembersAsync(companyId);

            foreach (var user in users)
            {
                ManageUsersRolesViewModel vm = new();
                vm.BTUser = user;
                var selected = await _rolesService.ListUserRolesAsync(user);
                vm.Roles = new MultiSelectList(_context.Roles, "Name", "Name", selected);
                model.Add(vm);
            }

            return View(model);
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUsersRolesViewModel member )
        {
            BTUser user = _context.Users.Find(member.BTUser.Id);

            IEnumerable<string> roles = await _rolesService.ListUserRolesAsync(user);
            //Homework
            await _rolesService.RemoveUserFromRolesAsync(user, roles);
            string userRole = member.SelectedRoles.FirstOrDefault();

            if (Enum.TryParse(userRole, out Roles rolesValue))
            {
                await _rolesService.AddUserToRoleAsync(user, userRole);
                return RedirectToAction("ManageUserRoles");
            }

            return View();
        }

    }
}
