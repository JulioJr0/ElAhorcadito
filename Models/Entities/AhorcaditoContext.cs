using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ElAhorcadito.Models.Entities;

public partial class AhorcaditoContext : DbContext
{
    public AhorcaditoContext()
    {
    }

    public AhorcaditoContext(DbContextOptions<AhorcaditoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notificaciones> Notificaciones { get; set; }

    public virtual DbSet<Palabras> Palabras { get; set; }

    public virtual DbSet<ProgresoTemas> ProgresoTemas { get; set; }

    public virtual DbSet<PushSubscriptions> PushSubscriptions { get; set; }

    public virtual DbSet<Rachas> Rachas { get; set; }

    public virtual DbSet<RefreshTokens> RefreshTokens { get; set; }

    public virtual DbSet<Temas> Temas { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Notificaciones>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notificaciones");

            entity.HasIndex(e => e.Leida, "idx_leida");

            entity.HasIndex(e => new { e.IdUsuario, e.FechaCreacion }, "idx_usuario_fecha");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida)
                .HasDefaultValueSql("'0'")
                .HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(255)
                .HasColumnName("mensaje");
            entity.Property(e => e.TipoNotificacion)
                .HasMaxLength(50)
                .HasColumnName("tipo_notificacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_notificacion_usuario");
        });

        modelBuilder.Entity<Palabras>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("palabras");

            entity.HasIndex(e => e.IdTema, "fk_palabra_tema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdTema).HasColumnName("id_tema");
            entity.Property(e => e.Palabra)
                .HasMaxLength(100)
                .HasColumnName("palabra");

            entity.HasOne(d => d.IdTemaNavigation).WithMany(p => p.Palabras)
                .HasForeignKey(d => d.IdTema)
                .HasConstraintName("fk_palabra_tema");
        });

        modelBuilder.Entity<ProgresoTemas>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("progreso_temas");

            entity.HasIndex(e => e.IdTema, "fk_progreso_tema");

            entity.HasIndex(e => new { e.IdUsuario, e.IdTema }, "uk_progreso_usuario_tema").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaUltimaActividad)
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultima_actividad");
            entity.Property(e => e.IdTema).HasColumnName("id_tema");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.PalabrasCompletadas)
                .HasDefaultValueSql("'0'")
                .HasColumnName("palabras_completadas");

            entity.HasOne(d => d.IdTemaNavigation).WithMany(p => p.ProgresoTemas)
                .HasForeignKey(d => d.IdTema)
                .HasConstraintName("fk_progreso_tema");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ProgresoTemas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_progreso_usuario");
        });

        modelBuilder.Entity<PushSubscriptions>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("push_subscriptions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.IdUsuario, "fk_push_usuario");

            entity.HasIndex(e => e.Activo, "idx_activo");

            entity.HasIndex(e => e.Endpoint, "idx_endpoint").IsUnique();

            entity.HasIndex(e => e.FechaCreacion, "idx_fecha_creacion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Auth)
                .HasMaxLength(100)
                .HasColumnName("auth");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(500)
                .HasColumnName("endpoint");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaUltimaNotificacion)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_ultima_notificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.P256dh)
                .HasMaxLength(200)
                .HasColumnName("p256dh");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.PushSubscriptions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_push_usuario");
        });

        modelBuilder.Entity<Rachas>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rachas");

            entity.HasIndex(e => e.IdUsuario, "id_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiasConsecutivos)
                .HasDefaultValueSql("'0'")
                .HasColumnName("dias_consecutivos");
            entity.Property(e => e.FechaUltimaRacha)
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultima_racha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.PalabrasTotales)
                .HasDefaultValueSql("'0'")
                .HasColumnName("palabras_totales");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Rachas)
                .HasForeignKey<Rachas>(d => d.IdUsuario)
                .HasConstraintName("fk_racha_usuario");
        });

        modelBuilder.Entity<RefreshTokens>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.IdUsuario, "fk_refresh_token_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Creado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("creado");
            entity.Property(e => e.Expiracion)
                .HasColumnType("datetime")
                .HasColumnName("expiracion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.Usado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("usado");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_refresh_token_usuario");
        });

        modelBuilder.Entity<Temas>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("temas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaGeneracion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.GeneradoPorIa)
                .HasDefaultValueSql("'1'")
                .HasColumnName("generado_por_ia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PromptBase)
                .HasMaxLength(255)
                .HasColumnName("prompt_base");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "nombre_usuario").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RecordatorioDiario)
                .HasDefaultValueSql("'1'")
                .HasColumnName("recordatorio_diario");
            entity.Property(e => e.SonidosActivados)
                .HasDefaultValueSql("'1'")
                .HasColumnName("sonidos_activados");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
