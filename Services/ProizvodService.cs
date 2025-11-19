using MySql.Data.MySqlClient;
using PrviProjekat.Database;
using PrviProjekat.Models;
using System.Collections.Generic;

namespace PrviProjekat.Services
{
    public static class ProizvodService
    {
        public static void DodajProizvod(Proizvod proizvod)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
                    INSERT INTO proizvod 
                    (Naziv, Sifra, Kategorija_idKATEGORIJA, Cijena, Opis, Dostupan)
                    VALUES 
                    (@Naziv, @Sifra, @Kategorija_idKATEGORIJA, @Cijena, @Opis, @Dostupan)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Naziv", proizvod.Naziv);
                    cmd.Parameters.AddWithValue("@Sifra", proizvod.Sifra);
                    cmd.Parameters.AddWithValue("@Kategorija_idKATEGORIJA", proizvod.Kategorija_idKATEGORIJA);
                    cmd.Parameters.AddWithValue("@Cijena", proizvod.Cijena);
                    cmd.Parameters.AddWithValue("@Opis", proizvod.Opis ?? "");
                    cmd.Parameters.AddWithValue("@Dostupan", proizvod.Dostupan);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateProizvod(Proizvod proizvod)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
                    UPDATE proizvod
                    SET Naziv = @Naziv,
                        Sifra = @Sifra,
                        Kategorija_idKATEGORIJA = @Kategorija_idKATEGORIJA,
                        Cijena = @Cijena,
                        Opis = @Opis,
                        Dostupan = @Dostupan
                    WHERE idPROIZVOD = @Id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Naziv", proizvod.Naziv);
                    cmd.Parameters.AddWithValue("@Sifra", proizvod.Sifra);
                    cmd.Parameters.AddWithValue("@Kategorija_idKATEGORIJA", proizvod.Kategorija_idKATEGORIJA);
                    cmd.Parameters.AddWithValue("@Cijena", proizvod.Cijena);
                    cmd.Parameters.AddWithValue("@Opis", proizvod.Opis ?? "");
                    cmd.Parameters.AddWithValue("@Dostupan", proizvod.Dostupan);
                    cmd.Parameters.AddWithValue("@Id", proizvod.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ObrisiProizvod(int proizvodId)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"DELETE FROM proizvod WHERE idPROIZVOD = @Id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", proizvodId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Proizvod> GetAll()
        {
            var proizvodi = new List<Proizvod>();

            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
                    SELECT p.idPROIZVOD, p.Naziv, p.Sifra, p.Cijena,
                           p.Kategorija_idKATEGORIJA, p.Opis, p.Dostupan
                    FROM proizvod p
                    WHERE p.Dostupan = 1;";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        proizvodi.Add(new Proizvod
                        {
                            Id = reader.GetInt32("idPROIZVOD"),
                            Naziv = reader.IsDBNull(reader.GetOrdinal("Naziv")) ? "" : reader.GetString("Naziv"),
                            Sifra = reader.IsDBNull(reader.GetOrdinal("Sifra")) ? "" : reader.GetString("Sifra"),
                            Cijena = reader.IsDBNull(reader.GetOrdinal("Cijena")) ? 0 : reader.GetDecimal("Cijena"),
                            Kategorija_idKATEGORIJA = reader.IsDBNull(reader.GetOrdinal("Kategorija_idKATEGORIJA")) ? 0 : reader.GetInt32("Kategorija_idKATEGORIJA"),
                            Opis = reader.IsDBNull(reader.GetOrdinal("Opis")) ? "" : reader.GetString("Opis"),
                            Dostupan = reader.IsDBNull(reader.GetOrdinal("Dostupan")) ? false : reader.GetBoolean("Dostupan")
                        });
                    }
                }
            }

            return proizvodi;
        }

        public static void DeleteProizvod(int id)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = "DELETE FROM proizvod WHERE idPROIZVOD = @Id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }



    }
}
