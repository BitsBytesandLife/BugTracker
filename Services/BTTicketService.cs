using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels.Enums;
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

            Ticket ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);
            try
            {
                if (ticket != null)
                {


                    try
                    {

                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                        ticket.DeveloperUserId = userId;
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
                Debug.WriteLine($"*** ERROR *** - Error No Ticket found. --> {ex.Message}");

            }
        }

        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            List<Project> projects = await _projectService.ListUserProjectsAsync(userId);
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets;
        }
        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {

            try
            {
                List<Ticket> tickets = await _context.Project.Include(p => p.Company)
                                                .Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.History)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .ToListAsync();
                return tickets;

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Ticket by Compamy found. --> {ex.Message}");
                throw;
            }


        }
        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;



            return await _context.Project.Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets)
                                            .Include(t => t.Attachments)
                                            .Include(t => t.Comments)
                                            .Include(t => t.DeveloperUser)
                                            .Include(t => t.OwnerUser)
                                            .Include(t => t.TicketPriority)
                                            .Include(t => t.TicketStatus)
                                            .Include(t => t.TicketType)
                                            .Include(t => t.Project)
                                            .Where(t => t.TicketPriorityId == priorityId).ToListAsync();
        }
        public async Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            List<Ticket> tickets = new();

            try
            {
                if (string.Compare(role, Roles.Developer.ToString()) == 0)
                {
                    tickets = await _context.Project.Include(p => p.Company)
                                                    .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.History)
                                                        .Include(t => t.DeveloperUser)
                                                        .Include(t => t.OwnerUser)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.Project)
                                                            .ThenInclude(p => p.Members)
                                                        .Include(t => t.Project)
                                                            .ThenInclude(p => p.ProjectPriority)
                                                        .Where(t => t.DeveloperUserId == userId).ToListAsync();
                }
                else if (string.Compare(role, Roles.Submitter.ToString()) == 0)
                {
                    tickets = await _context.Ticket
                                .Include(t => t.Attachments)
                                .Include(t => t.Comments)
                                .Include(t => t.DeveloperUser)
                                .Include(t => t.OwnerUser)
                                .Include(t => t.TicketPriority)
                                 .Include(t => t.TicketStatus)
                                 .Include(t => t.TicketType)
                                 .Include(t => t.Project)
                                    .ThenInclude(p => p.Members)
                                  .Include(t => t.Project)
                                  .ThenInclude(p => p.ProjectPriority)
                                  .Where(t => t.OwnerUserId == userId).ToListAsync();

                }

                else if (string.Compare(role, Roles.ProjectManager.ToString()) == 0)
                {
                    tickets = await GetAllPMTicketsAsync(userId);
                }

                return tickets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No TicketsByRole found. --> {ex.Message}");
                throw;
            }
            
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
            // List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            // List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();
            // return await _context.Ticket.Where(t => t.Archived == true).ToListAsync();

            try
            {
                List<Ticket> tickets = await _context.Project.Include(p => p.Company)
                                                .Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                    .Include(t => t.Attachments)
                                                    .Include(t => t.Comments)
                                                    .Include(t => t.History)
                                                    .Include(t => t.DeveloperUser)
                                                    .Include(t => t.OwnerUser)
                                                    .Include(t => t.TicketPriority)
                                                    .Include(t => t.TicketStatus)
                                                    .Include(t => t.TicketType)
                                                    .Include(t => t.Project)
                                                    .Where(t => t.Archived == true)
                                                    .ToListAsync();
                return tickets;

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"*** Error *** - No Project Tickets by Role found -> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {
            List<Ticket> tickets = new();
            try
            {
                tickets = (await GetAllTicketsByRoleAsync(role, userId)).Where(t => t.ProjectId == projectId).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Project Tickets By Role found. --> {ex.Message}");
                throw;
                throw;
            }
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            try
            {
                BTUser developer = new();

                Ticket ticket = await _context.Ticket.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket?.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }

                return developer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No TicketDeveloper found. --> {ex.Message}");
                throw;
            }
           

        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {

            try
            {
                TicketPriority priority = await _context.TicketPriority.FirstOrDefaultAsync(t => t.Name == priorityName);
                return priority?.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Ticket Priority found. --> {ex.Message}");
                throw;
            }
           
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {

            try
            {
                TicketStatus status = await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == statusName);
                return status?.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Ticket Status found. --> {ex.Message}");
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketType.FirstOrDefaultAsync(t => t.Name == typeName);
                return type?.Id;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"*** ERROR *** - Error No Ticket Type found. --> {ex.Message}");
                throw;
            }  
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string typeName, int conpamyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(conpamyId, typeName))
                            .Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Project Ticket by Status found. --> {ex.Message}");
                throw;
            }
        }


        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int conpamyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(conpamyId, typeName))
                            .Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Project Ticket by Type found. --> {ex.Message}");
                throw;
            }
        }


        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string typeName, int conpamyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(conpamyId, typeName))
                            .Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error No Project Ticket by Priority found. --> {ex.Message}");
                throw;
            }
        }

    }

}


