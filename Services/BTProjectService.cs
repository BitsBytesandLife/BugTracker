using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly ILogger<BTProjectService> _logger;

        public BTProjectService(UserManager<BTUser> userManager, ApplicationDbContext context, IBTRolesService rolesService, ILogger<BTProjectService> logger)
        {
            _userManager = userManager;
            _context = context;
            _rolesService = rolesService;
            _logger = logger;
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            //remove the current PM if necessary
            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error removing current PM. -Error: {ex:Message}");
                    return false;
                }
            }

            //Add the new PM
            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error adding new PM. - Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            try
            {
                if (user != null)
                {
                    Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);

                    if (!await IsUserOnProject(userId, projectId))
                    {
                        try
                        {
                            project.Members.Add(user);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }
                    else
                    {
                        return false;

                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"*** ERROR *** - Error Adding user to project. --> {ex.Message}");
                return false;
            }
           
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {   //List an Developers on Project

           Project projects = await _context.Project
                            .Include(p => p.Members)
                            .FirstOrDefaultAsync(u => u.Id == projectId);

            List<BTUser> developers = new();

            foreach (var user in projects.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, "Developer"))
                {
                    developers.Add(user);
                }
            }

            
            return developers;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            //Get All Projects By Company

            //SQL 
            //SELECT *
            //FROM PROJECT as P
            //INNER JOIN COMPANY as C
            //WHERE P.CompanyId = C.companyId

            List<Project> projects = new();

            projects = await _context.Project
                             .Include(p => p.Members)
                             .Include(p => p.ProjectPriority)
                             .Include(p => p.Tickets)
                                .ThenInclude(t => t.OwnerUser)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.DeveloperUser)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.Comments)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.Attachments)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.History)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.TicketPriority)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.TicketStatus)
                            .Include(p => p.Tickets)
                                .ThenInclude(t => t.TicketType)
                            .Where(p => p.CompanyId == companyId).ToListAsync();

            return projects;

            //return await _context.Project.Where(p => p.CompanyId == companyId).ToListAsync();

        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            int priorityId = await LookUpProjectPriorityId(priorityName);
            return await _context.Project.Where(p => p.CompanyId == companyId && p.ProjectPriorityId == priorityId).ToListAsync();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            //Archived Projects By Company
            return await _context.Project.Where(p => p.CompanyId == companyId && p.Archived == true).ToListAsync();

        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            //Get Members Without PM
            List<BTUser> developers = await DevelopersOnProjectAsync(projectId);
            List<BTUser> submitters = await SubmittersOnProjectAsync(projectId);
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, "Admin");

            List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

            return teamMembers;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            //Get Project Manager
            Project project = await _context.Project
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(u => u.Id == projectId);
            foreach (BTUser member in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(member, "ProjectManager"))
                {
                    return member;
                }
            }
            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Project
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(u => u.Id == projectId);

            List<BTUser> members = new();

            foreach (var user in project.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, role))
                {
                    members.Add(user);
                }
            }
            return members;
        }


        public async  Task<bool> IsUserOnProject(string userId, int projectId)
        {
            Project project = await _context.Project
                                    .FirstOrDefaultAsync(u => u.Id == projectId);

            bool results = project.Members.Any(u => u.Id == userId);

            return results;

        }

        public async Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            //List all the Projets 
            // take project by userId return a list
            List<Project> userProjects = (await _context.Users
                                           .Include(u => u.Projects)
                                                .ThenInclude(p => p.Company)
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.Members)
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                            .Include(u => u.Projects)
                                                .ThenInclude(p => p.ProjectPriority)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                            .Include(u => u.Projects)
                                                .ThenInclude(t => t.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketPriority)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                .Include(u => u.Projects)
                                                    .ThenInclude(t => t.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
            return userProjects;
        }

        public async Task<int> LookUpProjectPriorityId(string priorityName)
        {
            return (await _context.ProjectPriority.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            //Remove a Project Manager
            Project project = await _context.Project
                                        .Include(p => p.Members)
                                        .FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                foreach (BTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {

                        await RemoveUserFromProjectAsync(member.Id, project.Id);
                    }
                }
            }
            catch
            {

                throw;
            }

        }

        //async Task<bool>
        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);

                if (await IsUserOnProject(userId, projectId))
                {
                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" ERROR  - Error removing user to project.  --> {ex.Message}");
            }
        }


        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser btUser in members)
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error removing users from project.  --> {ex.Message}");
            }
        }

        public async Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(u => u.Id == projectId);

            List<BTUser> submitters = new();

            foreach (var user in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, "Submitter"))
                {
                    submitters.Add(user);
                }
            }

            return submitters;
        }

        public async Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            //List  Users Not On Project
            //List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) 
            //                                                    && u.CompanyId == companyId).ToListAsync();
            //return users;

            List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();

            return users.Where(u => u.CompanyId == companyId).ToList();

        }
    }
}
