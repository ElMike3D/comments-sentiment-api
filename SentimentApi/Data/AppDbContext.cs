using Microsoft.EntityFrameworkCore;
using SentimentApi.Models;
using System.Collections.Generic;

namespace SentimentApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Comment> Comments { get; set; }
    }
}
