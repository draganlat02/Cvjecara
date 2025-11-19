using MySql.Data.MySqlClient;
using PrviProjekat.Database;
using PrviProjekat.Models;
using System.Collections.Generic;
using System.Linq;

namespace PrviProjekat.Services
{
    public static class KorisnikService
    {
        private static string connStr = "server=localhost;database=cvjecara;uid=root;pwd=dragan;";

        // Login
        public static Korisnik Login(string username, string password)
        {
            username = username.Trim().ToLower();
            password = password.Trim();

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = @"
                SELECT *
                FROM ZAPOSLENI
                WHERE LOWER(TRIM(Username)) = @username 
                  AND TRIM(Password) = @password";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Korisnik
                {
                    Id = reader.GetInt32("idZAPOSLENI"),
                    Ime = reader.GetString("Ime"),
                    Prezime = reader.GetString("Prezime"),
                    Kontakt = reader.IsDBNull(reader.GetOrdinal("Kontakt")) ? "" : reader.GetString("Kontakt"),
                    Tip = reader.GetString("Pozicija") == "Administrator" ? TipKorisnika.Administrator : TipKorisnika.Referent,
                    Username = reader.GetString("Username"),
                    Password = reader.GetString("Password")
                };
            }

            return null;
        }

        // Dohvati sve korisnike
        public static List<Korisnik> GetAll()
        {
            var list = new List<Korisnik>();
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT * FROM ZAPOSLENI";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Korisnik
                {
                    Id = reader.GetInt32("idZAPOSLENI"),
                    Ime = reader.GetString("Ime"),
                    Prezime = reader.GetString("Prezime"),
                    Kontakt = reader.IsDBNull(reader.GetOrdinal("Kontakt")) ? "" : reader.GetString("Kontakt"),
                    Tip = reader.GetString("Pozicija") == "Administrator" ? TipKorisnika.Administrator : TipKorisnika.Referent,
                    Username = reader.GetString("Username"),
                    Password = reader.GetString("Password")
                });
            }

            return list;
        }

        // Dodaj korisnika
        public static void AddKorisnik(Korisnik k)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = @"
                INSERT INTO ZAPOSLENI (Ime, Prezime, Kontakt, Pozicija, Username, Password)
                VALUES (@ime, @prezime, @kontakt, @pozicija, @username, @password)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ime", k.Ime);
            cmd.Parameters.AddWithValue("@prezime", k.Prezime);
            cmd.Parameters.AddWithValue("@kontakt", k.Kontakt);
            cmd.Parameters.AddWithValue("@pozicija", k.Tip == TipKorisnika.Administrator ? "Administrator" : "Referent");
            cmd.Parameters.AddWithValue("@username", k.Username);
            cmd.Parameters.AddWithValue("@password", k.Password);

            cmd.ExecuteNonQuery();
        }

        // Update korisnika
        public static void UpdateKorisnik(Korisnik k)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = @"
                UPDATE ZAPOSLENI
                SET Ime=@ime, Prezime=@prezime, Kontakt=@kontakt, Pozicija=@pozicija, Username=@username, Password=@password
                WHERE idZAPOSLENI=@id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", k.Id);
            cmd.Parameters.AddWithValue("@ime", k.Ime);
            cmd.Parameters.AddWithValue("@prezime", k.Prezime);
            cmd.Parameters.AddWithValue("@kontakt", k.Kontakt);
            cmd.Parameters.AddWithValue("@pozicija", k.Tip == TipKorisnika.Administrator ? "Administrator" : "Referent");
            cmd.Parameters.AddWithValue("@username", k.Username);
            cmd.Parameters.AddWithValue("@password", k.Password);

            cmd.ExecuteNonQuery();
        }

        // Obriši korisnika
        public static void DeleteKorisnik(int id)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "DELETE FROM ZAPOSLENI WHERE idZAPOSLENI=@id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Provjera UNIQUE username
        public static bool UsernameExists(string username, int excludeId = 0)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT COUNT(*) FROM ZAPOSLENI WHERE Username=@username AND idZAPOSLENI<>@id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@id", excludeId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static void UpdateTema(int zaposleniId, string tema)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = "UPDATE ZAPOSLENI SET Tema = @tema WHERE idZAPOSLENI = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@tema", tema);
                    cmd.Parameters.AddWithValue("@id", zaposleniId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string GetTema(int zaposleniId)
        {
            using (var conn = MySqlDb.GetConnection())
            {
                string query = "SELECT Tema FROM ZAPOSLENI WHERE idZAPOSLENI = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", zaposleniId);
                    return cmd.ExecuteScalar()?.ToString() ?? "Light";
                }
            }
        }

        public static void UpdateJezik(int zaposleniId, string jezik)
        {
            using (var conn = MySqlDb.GetConnection())
            { 

                string query = "UPDATE ZAPOSLENI SET Jezik = @jezik WHERE idZAPOSLENI = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", zaposleniId);
                    cmd.Parameters.AddWithValue("@jezik", jezik);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // DODAJ OVO ZA DEBUG (privremeno)
                    System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows for user {zaposleniId} -> {jezik}");
                }
            }
        }

        public static string GetJezik(int id)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            string query = "SELECT Jezik FROM ZAPOSLENI WHERE idZAPOSLENI = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteScalar()?.ToString() ?? "bs-BA";
        }
    }
}
