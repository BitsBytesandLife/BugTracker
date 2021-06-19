using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTNotificationService _notificationService;

        public BTHistoryService(ApplicationDbContext context, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService, IBTNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _notificationService = notificationService;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {


            Notification notification = new();
           // int companyId = User.Identity.GetCompanyId().Value;
           // BTUser currentUser = await _userManager.GetUserAsync(User);
            
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };

                await _context.TicketHistory.AddAsync(history);
                await _context.SaveChangesAsync();

            }
            else 
            {
                //Check Title
                if (oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Title: {newTicket.Title}"
                    };

                    await _context.TicketHistory.AddAsync(history);
                }
                //Check Desprip
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Description: {newTicket.Description}"
                    };

                    await _context.TicketHistory.AddAsync(history);
                }
                //Check Type
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketType",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Type: {newTicket.TicketType.Name}"
                    };

                    await _context.TicketHistory.AddAsync(history);
                }
                //Check Prior
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketPriority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Priority: {newTicket.TicketPriority.Name}"
                    };

                    //notification = new()
                    //{
                    //    TicketId = newTicket.Id,
                    //    Title = "New Dev Ticket",
                    //    Message = $"You have have a new ticket: {newTicket?.Title}, was Created by {currentUser?.FullName}",
                    //    Created = DateTimeOffset.Now,
                    //    SenderId = currentUser?.Id,
                    //    RecipientId = newTicket.DeveloperUserId

                    //};

                    await _context.TicketHistory.AddAsync(history);
                }
                //Check Status
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketStatus",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Status: {newTicket.TicketStatus.Name}"
                    };

                    await _context.TicketHistory.AddAsync(history);
                }
                //Check Dev User
                if (oldTicket.DeveloperUser != newTicket.DeveloperUser)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Developer User: {newTicket.DeveloperUser.FullName}"
                    };

                    await _context.TicketHistory.AddAsync(history);
                }

            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {

            Company company = await _context.Company
                                        .Include(c => c.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
            List<TicketHistory> ticketHistories = new();
            ticketHistories = company.Projects
                                    .SelectMany(p => p.Tickets)
                                    .SelectMany(t => t.History).ToList();

            return ticketHistories;
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId)
        {

            Project project = await _context.Project
                                    .Include(p => p.Tickets)
                                        .ThenInclude(t => t.History)
                                    .FirstOrDefaultAsync(p => p.Id == projectId);
            
            List<TicketHistory> ticketHistories = new();

            ticketHistories = project.Tickets.SelectMany(t => t.History).ToList();
            
            return ticketHistories;
        }
    }
}
