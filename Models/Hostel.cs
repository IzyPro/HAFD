using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HAFD.Models
{
    public class Hostel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Corner { get; set; }
        [Required]
        public string Room { get; set; }
        public bool IsAvailable { get; set; }
    }
}
