using System;
using System.Collections.Generic;

namespace PrviProjekat.Models
{
    public class Narudzba
    {
        public int Id { get; set; }
        public int? ZaposleniId { get; set; }  // za upis u bazu

        public string Zaposleni { get; set; }  // za prikaz
        public string ImeKupca { get; set; }


        public DateTime Datum { get; set; }
        public string Napomena { get; set; }
        public string Status { get; set; }
        public decimal Ukupno { get; set; }

        public List<NarudzbaStavka> Stavke { get; set; } = new();
    }
}
