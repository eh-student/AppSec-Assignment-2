using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FreshFarmMarket.Models;

namespace FreshFarmMarket.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserOfTheFreshestFarmestMarket>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
