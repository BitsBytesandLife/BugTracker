using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        public BTTicketService(ApplicationDbContext context, UserManager<BTUser> userManager, IBTRolesService rolesService = null, IBTProjectService projectService = null)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
            _projectService = projectService;
        }
        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            //Get your user
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            try
            {
                if (user != null)
                {
                    Ticket ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);

                    try
                    {
                        if (ticket.DeveloperUser != null)
                        {
                            //remove dev users if its there
                            ticket.DeveloperUser = null;
                        }


                        ticket.DeveloperUserId = user.Id;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {

                        Debug.WriteLine($"*** ERROR *** - Error No Ticket was assigned. --> {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No user found. --> {ex.Message}");

            }
        }

        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            BTUser user = await _context.Users.Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                .FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(user, "ProjectManager"))
                {
                    tickets = user.Projects.SelectMany(p => p.Tickets).ToList();
                }
                return null;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"*** Error *** - User has no Tickets in the Project Role -> {ex.Message}");
            }

            return tickets;


        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

            List<Project> projects = await _context.Project.Include(p => p.Tickets).Where(p => p.CompanyId == companyId).ToListAsync();
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets.Where(t => t.TicketPriorityId == priorityId).ToList();
        }

        public async Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            BTUser user = await _context.Users.Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                .FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
            {
                if ((await _rolesService.ListUserRolesAsync(user)).Contains(role))
                {
                    tickets = user.Projects.SelectMany(p => p.Tickets).ToList();
                }
                return null;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"*** Error *** - Users has no tickets in the this Role -> {ex.Message}");
            }

            return tickets;

         
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {

            int ticketStatusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            List<Project> projects = await _context.Project.Include(p => p.Tickets).Where(p => p.CompanyId == companyId).ToListAsync();
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets.Where(t => t.TicketStatusId == ticketStatusId).ToList();
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int ticketTypeId = (await LookupTicketTypeIdAsync(typeName)).Value;
            List<Project> projects = await _context.Project.Include(p => p.Tickets).Where(p => p.CompanyId == companyId).ToListAsync();
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets.Where(t => t.TicketTypeId == ticketTypeId).ToList();
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();
            return await _context.Ticket.Where(t => t.Archived == true).ToListAsync();
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {

            BTUser user = await _context.Users.Include(u => u.Projects)
                                                   .ThenInclude(t => t.Tickets)
                                               .FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();


            try
            {
                if ((await _rolesService.ListUserRolesAsync(user)).Contains(role))
                { 
                    tickets = user.Projects.SelectMany(p => p.Tickets).ToList();
                    //tickets = await _context.Ticket.Include(t => t.ProjectId == projectId).ToListAsync();

                }
                return null;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"*** Error *** - User has no Tickets in the Project Role -> {ex.Message}");
            }

           
            return await _context.Ticket.Include(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {

            BTUser btUser = new();

            Ticket ticket = await _context.Ticket.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t => t.Id == ticketId);

            return ticket.DeveloperUser;

        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            return (await _context.TicketPriority.FirstOrDefaultAsync(t => t.Name == priorityName)).Id;
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            return (await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == statusName)).Id;
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            return (await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == typeName)).Id;
        }



    }

}


