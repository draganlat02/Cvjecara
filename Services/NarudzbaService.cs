using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PrviProjekat.Database;
using PrviProjekat.Models;

namespace PrviProjekat.Services
{
    public static class NarudzbaService
    {
        // ==============================
        // 1️⃣ Dohvatanje svih narudžbi
        // ==============================
        public static List<Narudzba> GetAll()
        {
            var narudzbe = new List<Narudzba>();

            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
                    SELECT n.idNARUDZBA, n.DatumNarudzbe, n.Napomena,
                           s.Opis AS Status,
                           n.ImeKupca AS Kupac,
                           CONCAT(z.Ime, ' ', z.Prezime) AS Zaposleni
                    FROM narudzba n
                    LEFT JOIN status_narudzbe s ON n.StatusNarudzbe_idSTATUS = s.idSTATUS
                    LEFT JOIN zaposleni z ON n.Zaposleni_idZAPOSLENI = z.idZAPOSLENI
                    ORDER BY n.DatumNarudzbe DESC";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        narudzbe.Add(new Narudzba
                        {
                            Id = reader.GetInt32("idNARUDZBA"),
                            Datum = reader.GetDateTime("DatumNarudzbe"),
                            Napomena = reader["Napomena"] as string,
                            Status = reader["Status"] as string,
                            ImeKupca = reader["Kupac"] as string,
                            Zaposleni = reader["Zaposleni"] as string
                        });
                    }
                }
            }

            // Za svaku narudžbu — učitaj stavke
            foreach (var n in narudzbe)
            {
                n.Stavke = GetStavke(n.Id);
                n.Ukupno = 0;

                foreach (var s in n.Stavke)
                    n.Ukupno += s.Ukupno;
            }

            return narudzbe;
        }

        // ==============================
        // 2️⃣ Dohvatanje stavki narudžbe
        // ==============================
        private static List<NarudzbaStavka> GetStavke(int narudzbaId)
        {
            var stavke = new List<NarudzbaStavka>();

            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
                    SELECT p.idPROIZVOD, p.Naziv, nhp.Kolicina, nhp.Cijena
                    FROM narudzba_has_proizvod nhp
                    JOIN proizvod p ON nhp.PROIZVOD_idPROIZVOD = p.idPROIZVOD
                    WHERE nhp.NARUDZBA_idNARUDZBA = @id";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", narudzbaId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stavke.Add(new NarudzbaStavka
                        {
                            ProizvodId = reader.GetInt32("idPROIZVOD"),
                            NazivProizvoda = reader.GetString("Naziv"),
                            Kolicina = reader.GetInt32("Kolicina"),
                            Cijena = reader.GetDecimal("Cijena")
                        });
                    }
                }
            }

            return stavke;
        }

        // ==============================
        // 3️⃣ Promjena statusa narudžbe
        // ==============================
        public static void UpdateStatus(int narudzbaId, int noviStatusId)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = "UPDATE narudzba SET StatusNarudzbe_idSTATUS = @s WHERE idNARUDZBA = @id";
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@s", noviStatusId);
                cmd.Parameters.AddWithValue("@id", narudzbaId);
                cmd.ExecuteNonQuery();
            }
        }

        // ==============================
        // 4️⃣ Dodavanje nove narudžbe
        // ==============================
        public static int Add(Narudzba narudzba)
        {
            int narudzbaId = 0;

            using (var conn = MySqlDb.GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1️⃣ Ubaci glavnu narudžbu
                    // 1. Ubaci glavnu narudžbu
                    string query = @"
    INSERT INTO narudzba
        (ImeKupca, Zaposleni_idZAPOSLENI, DatumNarudzbe, Napomena, StatusNarudzbe_idSTATUS)
    VALUES
        (@ImeKupca, @Zaposleni, @Datum, @Napomena, @Status);
    SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ImeKupca", narudzba.ImeKupca);
                        cmd.Parameters.AddWithValue("@Zaposleni", narudzba.ZaposleniId);
                        cmd.Parameters.AddWithValue("@Datum", narudzba.Datum);
                        cmd.Parameters.AddWithValue("@Napomena", narudzba.Napomena ?? (object)DBNull.Value);

                        // OVDE JE PROMENA:
                        int statusId = narudzba.Status switch
                        {
                            "U obradi" => 1,
                            "Otkazana" => 4,
                            "Završena" => 7,
                            _ => 1
                        };
                        cmd.Parameters.AddWithValue("@Status", statusId);

                        narudzbaId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2️⃣ Ubaci stavke narudžbe
                    string stavkaQuery = @"
                        INSERT INTO narudzba_has_proizvod 
                            (NARUDZBA_idNARUDZBA, PROIZVOD_idPROIZVOD, Kolicina, Cijena)
                        VALUES 
                            (@narudzbaId, @proizvodId, @kolicina, @cijena);";

                    foreach (var s in narudzba.Stavke)
                    {
                        using (var cmd = new MySqlCommand(stavkaQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@narudzbaId", narudzbaId);
                            cmd.Parameters.AddWithValue("@proizvodId", s.ProizvodId);
                            cmd.Parameters.AddWithValue("@kolicina", s.Kolicina);
                            cmd.Parameters.AddWithValue("@cijena", s.Cijena);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Greška pri čuvanju narudžbe: " + ex.Message);
                }
            }

            return narudzbaId;
        }

        // ==================================
        // 5️⃣ Dohvati narudžbe po statusu i periodu
        // ==================================
        public static List<Narudzba> GetByStatusAndPeriod(string statusOpis, int? daysBack)
        {
            var narudzbe = new List<Narudzba>();
            using (var conn = MySqlDb.GetConnection())
            {
                string query = @"
            SELECT n.idNARUDZBA, n.DatumNarudzbe, n.Napomena,
                   s.Opis AS Status,
                   n.ImeKupca AS Kupac,
                   CONCAT(z.Ime, ' ', z.Prezime) AS Zaposleni
            FROM narudzba n
            LEFT JOIN status_narudzbe s ON n.StatusNarudzbe_idSTATUS = s.idSTATUS
            LEFT JOIN zaposleni z ON n.Zaposleni_idZAPOSLENI = z.idZAPOSLENI
            WHERE s.Opis = @statusOpis";

                if (daysBack.HasValue)
                {
                    query += " AND n.DatumNarudzbe >= DATE_SUB(CURDATE(), INTERVAL @daysBack DAY)";
                }

                query += " ORDER BY n.DatumNarudzbe DESC";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@statusOpis", statusOpis);
                    if (daysBack.HasValue)
                        cmd.Parameters.AddWithValue("@daysBack", daysBack.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            narudzbe.Add(new Narudzba
                            {
                                Id = reader.GetInt32("idNARUDZBA"),
                                Datum = reader.GetDateTime("DatumNarudzbe"),
                                Napomena = reader["Napomena"] as string,
                                Status = reader["Status"] as string,
                                ImeKupca = reader["Kupac"] as string,
                                Zaposleni = reader["Zaposleni"] as string
                            });
                        }
                    }
                }
            }

            foreach (var n in narudzbe)
            {
                n.Stavke = GetStavke(n.Id);
                n.Ukupno = n.Stavke.Sum(s => s.Ukupno);
            }

            return narudzbe;
        }



    }
}