using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [DisplayName("Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }
        [NotMapped]
        [DisplayName("Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile AvatarFormFile { get; set; }

        [DisplayName("File Name")]
        public string AvatarFileName { get; set; }

        [DisplayName("File Data")]
        public byte[] AvatarFileData { get; set; }

        [DisplayName("File Extension")]
        public string AvatarFileContentType { get; set; }

        public int? CompanyId { get; set; }

        public virtual Company Company { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    } 
}
