using Microsoft.EntityFrameworkCore;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackAlbum> TrackAlbums => Set<TrackAlbum>();
    public DbSet<TrackArtist> TrackArtists => Set<TrackArtist>();
    public DbSet<TrackGenre> TrackGenres => Set<TrackGenre>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.SecretWordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.IsBlocked).HasDefaultValue(false);
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.Artist)
                .WithMany(x => x.Albums)
                .HasForeignKey(x => x.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Title, x.ArtistId }).IsUnique();
        });

        modelBuilder.Entity<AlbumArtist>(entity =>
        {
            entity.HasKey(x => new { x.AlbumId, x.ArtistId });
            entity.HasOne(x => x.Album)
                .WithMany(x => x.AlbumArtists)
                .HasForeignKey(x => x.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Artist)
                .WithMany(x => x.AlbumArtists)
                .HasForeignKey(x => x.ArtistId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable("AlbumArtists");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AudioFilePath).HasMaxLength(500);
            entity.HasOne(x => x.Category).WithMany(x => x.Tracks).HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany<Album>()
                .WithMany()
                .UsingEntity<TrackAlbum>(
                    j => j.HasOne(x => x.Album).WithMany(x => x.TrackAlbums).HasForeignKey(x => x.AlbumId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne(x => x.Track).WithMany(x => x.TrackAlbums).HasForeignKey(x => x.TrackId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(x => new { x.TrackId, x.AlbumId });
                        j.ToTable("TrackAlbums");
                    });
            entity.HasMany(x => x.Artists).WithMany(x => x.Tracks)
                .UsingEntity<TrackArtist>(
                    j => j.HasOne(x => x.Artist).WithMany(x => x.TrackArtists).HasForeignKey(x => x.ArtistId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne(x => x.Track).WithMany(x => x.TrackArtists).HasForeignKey(x => x.TrackId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(x => new { x.TrackId, x.ArtistId });
                        j.ToTable("TrackArtists");
                    });
            entity.HasMany(x => x.Genres).WithMany(x => x.Tracks)
                .UsingEntity<TrackGenre>(
                    j => j.HasOne(x => x.Genre).WithMany(x => x.TrackGenres).HasForeignKey(x => x.GenreId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne(x => x.Track).WithMany(x => x.TrackGenres).HasForeignKey(x => x.TrackId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(x => new { x.TrackId, x.GenreId });
                        j.ToTable("TrackGenres");
                    });
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.Playlists).HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaylistTrack>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Playlist).WithMany(x => x.PlaylistTracks).HasForeignKey(x => x.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Track).WithMany(x => x.PlaylistTracks).HasForeignKey(x => x.TrackId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}