using System;
using System.Collections.Generic;

namespace ElAhorcadito.Models.Entities;

public partial class Temas
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? PromptBase { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public bool? GeneradoPorIa { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Palabras> Palabras { get; set; } = new List<Palabras>();

    public virtual ICollection<ProgresoTemas> ProgresoTemas { get; set; } = new List<ProgresoTemas>();
}
