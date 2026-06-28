using System;
using System.Collections.Generic;

namespace SystemAukcyjny.Wpf.Models
{
    public class Aukcja
    {
        public int IdAukcji { get; set; }
        public string Tytul { get; set; } = null!;
        public string? OpisAukcji { get; set; }
        public decimal CenaWywolawcza { get; set; }
        public decimal? KupTeraz { get; set; }
        public DateTime DataRozpoczecia { get; set; }
        public DateTime DataZakonczenia { get; set; }
        public string Status { get; set; } = "Aktywna";
        public int IdUzytkownika { get; set; }
        public int IdKategorii { get; set; }

        public virtual Uzytkownik Wystawca { get; set; } = null!;
        public virtual Kategoria Kategoria { get; set; } = null!;
        public virtual ICollection<Licytacja> Licytacje { get; set; } = new List<Licytacja>();
    }
}
