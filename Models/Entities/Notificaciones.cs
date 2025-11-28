using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class Notificaciones
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string TipoNotificacion { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public string? Mensaje { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool? Leida { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
