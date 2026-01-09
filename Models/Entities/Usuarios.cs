using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class Usuarios
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public bool? SonidosActivados { get; set; }

    public bool? RecordatorioDiario { get; set; }

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual ICollection<ProgresoTemas> ProgresoTemas { get; set; } = new List<ProgresoTemas>();

    public virtual ICollection<PushSubscriptions> PushSubscriptions { get; set; } = new List<PushSubscriptions>();

    public virtual Rachas? Rachas { get; set; }

    public virtual ICollection<RefreshTokens> RefreshTokens { get; set; } = new List<RefreshTokens>();
}
