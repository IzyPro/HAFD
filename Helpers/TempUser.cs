﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAFD.DTOs;
using HAFD.Models;
using HAFD.ViewModels;

namespace HAFD.Helpers
{
    public class TempUser
    {
        public static RegisterUserViewModel NewUser { get; set; }
        public static LoginResponseDTO loginResponse { get; set; }
        public static List<User> VerificationResult { get; set; }
    }
}
