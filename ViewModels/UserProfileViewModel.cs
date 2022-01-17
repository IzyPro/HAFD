using System;
using System.ComponentModel.DataAnnotations;
using HAFD.Enums;

namespace HAFD.ViewModels
{
    public class UserProfileViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public byte[] Image { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public GenderEnum Gender { get; set; }

        [Required]
        public UserStatusEnum UserStatus { get; set; }
    }
}
