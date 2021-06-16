using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class AssignPMViewModel
    {
        public Project Project { get; set; }

        public SelectList ProjectManagersSelectList { get; set; }

        public string ProjectManagerId { get; set; }

    }
}
