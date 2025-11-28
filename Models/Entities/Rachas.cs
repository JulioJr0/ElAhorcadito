using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class Rachas
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public int? DiasConsecutivos { get; set; }

    public DateTime? FechaUltimaRacha { get; set; }

    public int? PalabrasTotales { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
