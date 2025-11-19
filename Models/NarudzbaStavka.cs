namespace PrviProjekat.Models
{
    public class NarudzbaStavka
    {
        public int ProizvodId { get; set; }
        public string NazivProizvoda { get; set; }
        public int Kolicina { get; set; }
        public decimal Cijena { get; set; }
        public decimal Ukupno => Kolicina * Cijena;
    }
}
