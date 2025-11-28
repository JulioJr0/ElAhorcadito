using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class Palabras
{
    public int Id { get; set; }

    public int IdTema { get; set; }

    public string Palabra { get; set; } = null!;

    public virtual Temas IdTemaNavigation { get; set; } = null!;
}
