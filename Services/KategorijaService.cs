using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PrviProjekat.Database;
using PrviProjekat.Models;

namespace PrviProjekat.Services
{
    public static class KategorijaService
    {
        public static List<Kategorija> GetAll()
        {
            var kategorije = new List<Kategorija>();

            try
            {
                using (var conn = MySqlDb.GetConnection())
                {
                    string query = "SELECT idKATEGORIJA, Naziv, Opis FROM kategorija";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kategorije.Add(new Kategorija
                            {
                                idKATEGORIJA = reader.GetInt32("idKATEGORIJA"),
                                Naziv = reader.GetString("Naziv"),
                                Opis = reader.IsDBNull(reader.GetOrdinal("Opis")) ? "" : reader.GetString("Opis")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Možeš dodati logiranje ili prikaz poruke
                System.Windows.MessageBox.Show($"Greška pri učitavanju kategorija: {ex.Message}");
            }

            return kategorije;
        }
    }
}
