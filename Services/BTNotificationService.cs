using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IEmailSender _emailSender;

        public BTNotificationService(ApplicationDbContext context, 
                                     IBTCompanyInfoService infoService,
                                     IEmailSender emailSender)
        {
            _context = context;
            _infoService = infoService;
            _emailSender = emailSender;
        }
        public async Task AdminsNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                List<BTUser> admins = await _infoService.GetMembersInRoleAsync("Admin", companyId);

                foreach (BTUser bTUser in admins)
                {
                    notification.RecipientId = bTUser.Id;

                    // await SaveNotificationAsync(notification);
                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task EmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser currentUser = await _context.Users.FindAsync(notification.RecipientId);

            //Send Email
            string currentUserEmail = currentUser.Email;
            string message = notification.Message;
            try
            {
                await _emailSender.SendEmailAsync(currentUserEmail, emailSubject, message);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                            .Include(n => n.Recipient)
                                                            .Include(n => n.Sender)
                                                            .Include(n => n.Ticket)
                                                                .ThenInclude(t => t.Project)
                                                            .Where(n => n.RecipientId == userId).ToListAsync();
            return notifications;
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                          .Include(n => n.Recipient)
                                                          .Include(n => n.Sender)
                                                          .Include(n => n.Ticket)
                                                              .ThenInclude(t => t.Project)
                                                          .Where(n => n.SenderId == userId).ToListAsync();
            return notifications;
        }

        public async Task MembersNotificationAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (BTUser bTUser in members)
                {
                    notification.RecipientId = bTUser.Id;
                    // await SaveNotificationAsync(notification);

                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task SMSNotificationAsync(string phone, Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
