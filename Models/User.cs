using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HAFD.Enums;
using Microsoft.AspNetCore.Identity;

namespace HAFD.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        public byte[] Image { get; set; }

		[Required]
        public string Department { get; set; }

        [Required]
        public GenderEnum Gender { get; set; }

        [Required]
        public UserStatusEnum UserStatus { get; set; }

        public bool ForceChangeOfPassword { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public DateTime? LastPasswordReset { get; set; }
    }
}
