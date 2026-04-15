using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using story_web.Models;

namespace story_web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}
        public DbSet<Story> Stories {get;set;}
        public DbSet<Author> Authors {get;set;}
    }
}