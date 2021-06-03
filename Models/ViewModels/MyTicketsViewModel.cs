using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class MyTicketsViewModel
    {
        public Ticket Ticket { get; set; }
        public List<Ticket> myDeveloperTickets  { get; set; }
        public List<Ticket> mySubmitterTickets { get; set; }

    }
}
