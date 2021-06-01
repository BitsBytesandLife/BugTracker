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

        public BTHistoryService(ApplicationDbContext context, UserManager<BTUser> userManager,IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        public async Task AddHistory(Ticket oldTicket, Ticket newTicket, string userId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                



            }
           

            throw new NotImplementedException();
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistories(int companyId)
        {
            List<TicketHistory> ticketHistories = new();
            List<Ticket> tickets = new();
            List<Project> projects = new();

            projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            tickets = projects.SelectMany(t => t.Tickets).ToList();

            ticketHistories = tickets.SelectMany(t => t.History).ToList();

            return ticketHistories;
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistories(int projectId)
        {
            List<TicketHistory> ticketHistories = new();

            ticketHistories = await _context.TicketHistory
                                    .Include(t => t.Ticket)
                                    .ThenInclude(t => t.ProjectId == projectId)
                                    .ToListAsync();
            return ticketHistories;
        }
    }
}
