using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extentions;
using BugTracker.Models.ViewModels.Enums;
using BugTracker.Models.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTHistoryService _historyService;
        private readonly IBTNotificationService _notificationService;

        public TicketsController(ApplicationDbContext context, IBTTicketService ticketService, IBTCompanyInfoService companyInfoService, UserManager<BTUser> userManager, IBTProjectService projectService, IBTHistoryService historyService, IBTNotificationService notificationService)
        {
            _context = context;
            _ticketService = ticketService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
            _projectService = projectService;
            _historyService = historyService;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ticket.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Attachments)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult>  Create()
        {
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id");
            // ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Id");
            //todo: filter list
            BTUser btUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;


            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else 
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriority, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ProjectId,TicketPriorityId,TicketTypeId,Description")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);


                ticket.Created = DateTimeOffset.Now;

                string userId = _userManager.GetUserId(User);
                ticket.OwnerUserId = userId;

                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;
                 
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                #region Add History
                Ticket newTicket = await _context.Ticket
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.Project)
                                                 .Include(t => t.DeveloperUser)
                                                 .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                await _historyService.AddHistoryAsync(null, newTicket, btUser.Id);
                #endregion

                #region Notification
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;
                

               
                Notification notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket",
                    Message = $"New Ticket: {ticket?.Title}, was created by {btUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = btUser?.Id,
                    RecipientId = projectManager?.Id

                };
                if (projectManager != null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                    await _notificationService.EmailNotificationAsync(notification, "New ticket Added");
                }
                else
                {
                    await _notificationService.AdminsNotificationAsync(notification, companyId);
                }
               
                #endregion

                return RedirectToAction("Details","Projects",new {  id =ticket.ProjectId});
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            //ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriority, "Id", "Id", ticket.TicketPriorityId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Id", ticket.TicketStatusId);
            //ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
            //return View(ticket);
            return View();
        }
      
       
        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriority, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Created,Updated,Archived,ArchivedDate,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,OwnerUserId,DeveloperUserId,Description")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                BTUser currentUser = await _userManager.GetUserAsync(User);
                BTUser currentProjectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                Notification notification = new();

                Ticket oldTicket = await _context.Ticket
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.Project)
                                                 .Include(t => t.DeveloperUser)
                                                 .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);
                try
                {
                    ticket.Updated = DateTimeOffset.Now;

                    if (ticket.Archived == true)
                    {
                        ticket.ArchivedDate = DateTimeOffset.Now;
                        ticket.TicketStatusId = 1;
                    }
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                   
                    notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = "New Ticket",
                        Message = $"New Ticket: {ticket?.Title}, was Created by {currentUser?.FullName}",
                        Created = DateTimeOffset.Now,
                        SenderId = currentUser?.Id,
                        RecipientId = currentProjectManager?.Id

                    };
                    if (currentProjectManager!= null)
                    {
                        await _notificationService.SaveNotificationAsync(notification);
                        
                    }
                    else
                    {
                        await _notificationService.AdminsNotificationAsync(notification, companyId);
                    }
                    if (ticket.DeveloperUserId != null)
                    {

                        notification = new()
                        {
                            TicketId = ticket.Id,
                            Title = "New Ticket",
                            Message = $"New Ticket: {ticket?.Title}, was updated by {currentUser?.FullName}",
                            Created = DateTimeOffset.Now,
                            SenderId = currentUser?.Id,
                            RecipientId = ticket.DeveloperUserId

                        };

                        await _notificationService.SaveNotificationAsync(notification);
                    }

                  

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //Add History 
                Ticket newTicket = await _context.Ticket
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.Project)
                                                 .Include(t => t.DeveloperUser)
                                                 .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id);

                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriority, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketType, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AllTickets()
        {
            Ticket model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _companyInfoService.GetAllTicketsAsync(companyId);

            return View(tickets);
        }

        public async Task<IActionResult> MyTickets()
        {
            string currentUser = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            List<Ticket> developerTickets = await _ticketService.GetAllTicketsByRoleAsync(Roles.Developer.ToString(), currentUser);
            List<Ticket> submitterTickets = await _ticketService.GetAllTicketsByRoleAsync(Roles.Submitter.ToString(), currentUser);
            
            tickets = developerTickets.Concat(submitterTickets).ToList();
            
            return View(tickets);
        }
        [HttpGet]
        public async Task<IActionResult> AssignTicket(int? ticketId)
        {
            if (!ticketId.HasValue)
            {
                return NotFound();
            }

            AssignDevViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Ticket = (await _ticketService
                            .GetAllTicketsByCompanyAsync(companyId))
                            .FirstOrDefault(t => t.Id == ticketId);
            
            model.Developers = new SelectList(await _projectService
                                   .DevelopersOnProjectAsync(model.Ticket.ProjectId), "Id", "FullName");
                                         
            return View(model);
        }

        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachment.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AssignTicket(AssignDevViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.DeveloperId))
            {
                
                int companyId = User.Identity.GetCompanyId().Value;
                Notification notification = new();

                BTUser currentUser = await _userManager.GetUserAsync(User);
                BTUser currentDev = (await _companyInfoService.GetAllMembersAsync(companyId))
                                        .FirstOrDefault(m => m.Id == viewModel.DeveloperId);
                BTUser PM = await _projectService.GetProjectManagerAsync(viewModel.Ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Include(t => t.DeveloperUser)
                                                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);
                await _ticketService.AssignTicketAsync(viewModel.Ticket.Id, viewModel.DeveloperId);

                Ticket newTicket = await _context.Ticket
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .Include(t => t.Project)
                                               .Include(t => t.DeveloperUser)
                                               .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);
                notification = new()
                {
                    TicketId = newTicket.Id,
                    Title = "New Dev Ticket",
                    Message = $"You have have a new ticket: {newTicket?.Title}, was Created by {currentUser?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = currentUser?.Id,
                    RecipientId = newTicket.DeveloperUserId

                };

                await _notificationService.SaveNotificationAsync(notification);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, currentUser.Id); 
            }
            return RedirectToAction("Details", "Tickets", new { id = viewModel.Ticket.Id });
        }
    }
}
