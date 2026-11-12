using Microsoft.EntityFrameworkCore;

namespace LaborLawAssistant.Data;

public class PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options) : AppDbContext(options);
