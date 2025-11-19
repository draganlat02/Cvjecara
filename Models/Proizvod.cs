using System.ComponentModel.DataAnnotations.Schema;

namespace PrviProjekat.Models
{
    public class Proizvod
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public string Sifra { get; set; }
        public decimal Cijena { get; set; }

        [NotMapped]
        public int Kolicina { get; set; } = 1;

        [NotMapped]
        public decimal Ukupno { get; set; }
        [NotMapped]
        public string KategorijaNaziv { get; set; }


        public string Opis { get; set; }
        public int Kategorija_idKATEGORIJA { get; set; }  // 🔹 ID kategorije
        public bool Dostupan { get; set; }
    }
}
