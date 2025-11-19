namespace PrviProjekat.Models
{
    public enum TipKorisnika
    {
        Administrator,
        Referent
    }

    public class Korisnik
    {
        public int Id { get; set; }              // idZAPOSLENI
        public string Ime { get; set; }          // Ime
        public string Prezime { get; set; }      // Prezime
        public string Kontakt { get; set; }      // Kontakt
        public TipKorisnika Tip { get; set; }    // Pozicija -> Tip
        public string Username { get; set; }     // Username
        public string Password { get; set; }     // Password (plain text za sada)

        public string Tema { get; set; } = "Light";

        public string Jezik { get; set; } = "bs-BA";
    }
}
