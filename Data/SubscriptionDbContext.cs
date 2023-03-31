using Microsoft.EntityFrameworkCore;

namespace testpushnotification.Data;

public class SubscriptionDbContext: DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options):base(options) { }

    public DbSet<ClientSubscription> Subscriptions => Set<ClientSubscription>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=subsdb.sqlite");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ClientSubscription>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<ClientSubscription>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<ClientSubscription>()
            .HasIndex(x => new {x.ClientId, x.Endpoint});
    }
}

public class ClientSubscription
{
    public int Id {get;set;}
    public string ClientId {get;set;} = "";
    public string Endpoint {get;set;} = "";
    public string Auth {get;set;} = "";
    public string P256DH {get;set;} = "";
    public string JwtToken {get;set;} = "";
    
    public DateTimeOffset? Expires {get;set;}
    public DateTimeOffset? DateCreated {get;set;}
    public DateTimeOffset? DateModified {get;set;}
}