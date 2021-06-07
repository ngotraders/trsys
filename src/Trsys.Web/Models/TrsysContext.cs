using Microsoft.EntityFrameworkCore;

namespace Trsys.Web.Models
{
    public partial class TrsysContext : DbContext
    {
        public TrsysContext(DbContextOptions<TrsysContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Stream> Streams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Position)
                    .HasName("PK_Events");

                entity.HasIndex(e => new { e.StreamIdInternal, e.Created }, "IX_Messages_StreamIdInternal_Created");

                entity.HasIndex(e => new { e.StreamIdInternal, e.Id }, "IX_Messages_StreamIdInternal_Id")
                    .IsUnique();

                entity.HasIndex(e => new { e.StreamIdInternal, e.StreamVersion }, "IX_Messages_StreamIdInternal_Revision")
                    .IsUnique();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.JsonData).IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.StreamIdInternalNavigation)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.StreamIdInternal)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Events_Streams");
            });

            modelBuilder.Entity<Stream>(entity =>
            {
                entity.HasKey(e => e.IdInternal);

                entity.HasIndex(e => e.Id, "IX_Streams_Id")
                    .IsUnique();

                entity.HasIndex(e => new { e.IdOriginal, e.IdInternal }, "IX_Streams_IdOriginal");

                entity.HasIndex(e => new { e.IdOriginalReversed, e.IdInternal }, "IX_Streams_IdOriginalReversed");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(42)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.IdOriginal)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.IdOriginalReversed)
                    .HasMaxLength(1000)
                    .HasComputedColumnSql("(reverse([IdOriginal]))", false);

                entity.Property(e => e.Position).HasDefaultValueSql("((-1))");

                entity.Property(e => e.Version).HasDefaultValueSql("((-1))");
            });
        }
    }
}
