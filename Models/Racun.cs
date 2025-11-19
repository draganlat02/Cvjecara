using System;

namespace PrviProjekat.Models
{
    public class Racun
    {
        public int Id { get; set; }

        public decimal Iznos { get; set; }
        public DateTime? DatumPlacanja { get; set; }

        // FK vrijednosti koje se čuvaju u bazi
        public int? TipPlacanjaId { get; set; }
        public int? StatusRacunaId { get; set; }

        // Za prikaz (nije obavezno, ali koristiš isti pattern u Narudzba.cs)
        public string TipPlacanjaOpis { get; set; }
        public string StatusRacunaOpis { get; set; }
    }
}
