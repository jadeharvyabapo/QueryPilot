using Microsoft.EntityFrameworkCore;

namespace QueryPilot.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options);

    
    
