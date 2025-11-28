using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class RefreshTokens
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string Token { get; set; } = null!;

    public DateTime Expiracion { get; set; }

    public DateTime? Creado { get; set; }

    public bool? Usado { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
