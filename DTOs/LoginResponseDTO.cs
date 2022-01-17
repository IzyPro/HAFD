using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HAFD.Enums;

namespace HAFD.DTOs
{
    public class LoginResponseDTO
    {
        [Required]
        public Guid Id { get; set; }
        public string token { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public byte[] Image { get; set; }

        public bool HasActivePlan { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public GenderEnum Gender { get; set; }

        [Required]
        public UserStatusEnum UserStatus { get; set; }

        [Required]
        public List<string> UserRoles { get; set; }

        public bool ForceChangeOfPassword { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public DateTime? LastPasswordReset { get; set; }
    }
}
