using System.Collections.Generic;

namespace SystemAukcyjny.Wpf.Models
{
    public class Uzytkownik
    {
        public int IdUzytkownika { get; set; }
        public string Login { get; set; } = null!;
        public string Haslo { get; set; } = null!;
        public string Email { get; set; } = null!;

        public virtual ICollection<Aukcja> Aukcje { get; set; } = new List<Aukcja>();
        public virtual ICollection<Licytacja> Licytacje { get; set; } = new List<Licytacja>();
    }
}
