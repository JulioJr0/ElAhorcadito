using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class ProgresoTemas
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public int IdTema { get; set; }

    public int? PalabrasCompletadas { get; set; }

    public DateTime FechaUltimaActividad { get; set; }

    public virtual Temas IdTemaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
