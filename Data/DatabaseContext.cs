using System;
using HAFD.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HAFD.Data
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public new DbSet<User> Users { get; set; }
        public DbSet<Hostel> Hostels { get; set; }
    }
}
