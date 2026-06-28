using Microsoft.EntityFrameworkCore;
using SystemAukcyjny.Wpf.Models;

namespace SystemAukcyjny.Wpf.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }

        public DbSet<Uzytkownik> Uzytkownicy { get; set; }
        public DbSet<Kategoria> Kategorie { get; set; }
        public DbSet<Aukcja> Aukcje { get; set; }
        public DbSet<Licytacja> Licytacje { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Uzytkownik>(entity =>
            {
                entity.ToTable("Uzytkownik");
                entity.HasKey(e => e.IdUzytkownika);
                entity.Property(e => e.IdUzytkownika).HasColumnName("id_uzytkownika");
                entity.Property(e => e.Login).HasColumnName("login").HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.Haslo).HasColumnName("haslo").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Kategoria>(entity =>
            {
                entity.ToTable("Kategoria");
                entity.HasKey(e => e.IdKategorii);
                entity.Property(e => e.IdKategorii).HasColumnName("id_kategorii");
                entity.Property(e => e.NazwaKategorii).HasColumnName("nazwa_kategorii").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.NazwaKategorii).IsUnique();
                entity.Property(e => e.OpisKategorii).HasColumnName("opis_kategorii").HasColumnType("text");
            });

            modelBuilder.Entity<Aukcja>(entity =>
            {
                entity.ToTable("Aukcja");
                entity.HasKey(e => e.IdAukcji);
                entity.Property(e => e.IdAukcji).HasColumnName("id_aukcji");
                entity.Property(e => e.Tytul).HasColumnName("tytul").HasMaxLength(150).IsRequired();
                entity.Property(e => e.OpisAukcji).HasColumnName("opis_aukcji").HasColumnType("text");
                entity.Property(e => e.CenaWywolawcza).HasColumnName("cena_wywolawcza").HasColumnType("decimal(10,2)");
                entity.Property(e => e.KupTeraz).HasColumnName("kup_teraz").HasColumnType("decimal(10,2)");
                entity.Property(e => e.DataRozpoczecia).HasColumnName("data_rozpoczecia").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DataZakonczenia).HasColumnName("data_zakonczenia");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Aktywna");
                entity.Property(e => e.IdUzytkownika).HasColumnName("id_uzytkownika");
                entity.Property(e => e.IdKategorii).HasColumnName("id_kategorii");

                entity.HasOne(d => d.Wystawca)
                    .WithMany(p => p.Aukcje)
                    .HasForeignKey(d => d.IdUzytkownika)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Kategoria)
                    .WithMany(p => p.Aukcje)
                    .HasForeignKey(d => d.IdKategorii)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Licytacja>(entity =>
            {
                entity.ToTable("Licytacja");
                entity.HasKey(e => e.IdLicytacji);
                entity.Property(e => e.IdLicytacji).HasColumnName("id_licytacji");
                entity.Property(e => e.KwotaLicytacji).HasColumnName("kwota_licytacji").HasColumnType("decimal(10,2)");
                entity.Property(e => e.DataZlozenia).HasColumnName("data_zlozenia").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IdAukcji).HasColumnName("id_aukcji");
                entity.Property(e => e.IdUzytkownika).HasColumnName("id_uzytkownika");

                entity.HasOne(d => d.Aukcja)
                    .WithMany(p => p.Licytacje)
                    .HasForeignKey(d => d.IdAukcji)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Licytujacy)
                    .WithMany(p => p.Licytacje)
                    .HasForeignKey(d => d.IdUzytkownika)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
