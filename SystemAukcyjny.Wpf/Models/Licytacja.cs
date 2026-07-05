using System;

namespace SystemAukcyjny.Wpf.Models
{
    public class Licytacja
    {
        public int IdLicytacji { get; set; }
        public decimal KwotaLicytacji { get; set; }
        public DateTime DataZlozenia { get; set; }
        public int IdAukcji { get; set; }
        public int IdUzytkownika { get; set; }

        public virtual Aukcja Aukcja { get; set; } = null!;
        public virtual Uzytkownik Licytujacy { get; set; } = null!;
    }
}
