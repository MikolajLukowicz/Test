using System.Collections.Generic;

namespace SystemAukcyjny.Wpf.Models
{
    public class Kategoria
    {
        public int IdKategorii { get; set; }
        public string NazwaKategorii { get; set; } = null!;
        public string? OpisKategorii { get; set; }

        public virtual ICollection<Aukcja> Aukcje { get; set; } = new List<Aukcja>();
    }
}
