using Microsoft.EntityFrameworkCore;

namespace testpushnotification.Data;

public class SubscriptionDbContext: DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options):base(options) { }


    public DbSet<ClientSubscription> Subscriptions {get;set;}

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
            .HasKey(e => e.ClientId);
    }
}

public class ClientSubscription
{
    public string ClientId {get;set;} = "";
    public string Endpoint {get;set;}
    public string Auth {get;set;}
    public string P256DH {get;set;}
    public DateTimeOffset? Expires {get;set;}
    public DateTimeOffset? DateCreated {get;set;}
    public DateTimeOffset? DateModified {get;set;}
}