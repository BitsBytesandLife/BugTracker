using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DisplayName("Date")]
        public DateTimeOffset InviteDate { get; set; }

        [DisplayName("Code")]
        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; }
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        [DisplayName("Invitee")]
        public string InviteeId { get; set; }
        [DisplayName("Invitor")]
        public string InvitorId { get; set; }
        [DisplayName("Email")]
        public string InviteeEmail { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        public bool IsValid { get; set; }

        public virtual Company Compamy { get; set; }

        public virtual BTUser Invitor { get; set; }

        public virtual BTUser Invitee { get; set; }

        public virtual Project Project { get; set; }

    }
}
