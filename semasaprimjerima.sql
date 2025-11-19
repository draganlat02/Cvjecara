-- ==========================================
-- BAZA PODATAKA: Cvjecara
-- ==========================================
CREATE DATABASE cvjecara CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE cvjecara;

-- ==========================================
-- 1. Tabela: KATEGORIJA
-- ==========================================
CREATE TABLE KATEGORIJA (
    idKATEGORIJA INT AUTO_INCREMENT PRIMARY KEY,
    Naziv VARCHAR(45) NOT NULL,
    Opis VARCHAR(255)
);

-- ==========================================
-- 2. Tabela: PROIZVOD
-- ==========================================
CREATE TABLE PROIZVOD (
    idPROIZVOD INT AUTO_INCREMENT PRIMARY KEY,
    Naziv VARCHAR(100) NOT NULL,
    Sifra VARCHAR(20) UNIQUE NOT NULL,
    Kolicina INT NOT NULL,
    Cijena DECIMAL(10,2) NOT NULL,
    Kategorija_idKATEGORIJA INT,
    Opis TEXT,
    Dostupan BOOLEAN DEFAULT TRUE,
    CONSTRAINT fk_proizvod_kategorija FOREIGN KEY (Kategorija_idKATEGORIJA)
        REFERENCES KATEGORIJA(idKATEGORIJA)
        ON UPDATE CASCADE ON DELETE SET NULL
);

-- ==========================================
-- 3. Tabela: ZAPOSLENI
-- ==========================================
CREATE TABLE ZAPOSLENI (
    idZAPOSLENI INT AUTO_INCREMENT PRIMARY KEY,
    Ime VARCHAR(45) NOT NULL,
    Prezime VARCHAR(45) NOT NULL,
    Kontakt VARCHAR(45),
    Pozicija ENUM('Administrator', 'Referent') NOT NULL,
    Username VARCHAR(45) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL
);

-- ==========================================
-- 4. Tabela: STATUS_NARUDZBE
-- ==========================================
CREATE TABLE STATUS_NARUDZBE (
    idSTATUS INT AUTO_INCREMENT PRIMARY KEY,
    Opis VARCHAR(45) NOT NULL
);

-- ==========================================
-- 5. Tabela: STATUS_RACUNA
-- ==========================================
CREATE TABLE STATUS_RACUNA (
    idSTATUS_RACUNA INT AUTO_INCREMENT PRIMARY KEY,
    Opis VARCHAR(45) NOT NULL
);

-- ==========================================
-- 6. Tabela: TIP_PLACANJA
-- ==========================================
CREATE TABLE TIP_PLACANJA (
    idTIP_PLACANJA INT AUTO_INCREMENT PRIMARY KEY,
    Opis VARCHAR(45) NOT NULL
);

-- ==========================================
-- 7. Tabela: RACUN
-- ==========================================
CREATE TABLE RACUN (
    idRACUN INT AUTO_INCREMENT PRIMARY KEY,
    Iznos DECIMAL(10,2) NOT NULL,
    DatumPlacanja DATETIME,
    TipPlacanja_idTIP_PLACANJA INT,
    StatusRacuna_idSTATUS_RACUNA INT,
    CONSTRAINT fk_racun_tipplacanja FOREIGN KEY (TipPlacanja_idTIP_PLACANJA)
        REFERENCES TIP_PLACANJA(idTIP_PLACANJA)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_racun_status FOREIGN KEY (StatusRacuna_idSTATUS_RACUNA)
        REFERENCES STATUS_RACUNA(idSTATUS_RACUNA)
        ON UPDATE CASCADE ON DELETE SET NULL
);

-- ==========================================
-- 8. Tabela: NARUDZBA
-- ==========================================
CREATE TABLE NARUDZBA (
    idNARUDZBA INT AUTO_INCREMENT PRIMARY KEY,
    Zaposleni_idZAPOSLENI INT,
    ImeKupca TEXT,
    DatumNarudzbe DATETIME NOT NULL,
    Napomena TEXT,
    StatusNarudzbe_idSTATUS INT,
    Racun_idRACUN INT,
    CONSTRAINT fk_narudzba_zaposleni FOREIGN KEY (Zaposleni_idZAPOSLENI)
        REFERENCES ZAPOSLENI(idZAPOSLENI)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_narudzba_status FOREIGN KEY (StatusNarudzbe_idSTATUS)
        REFERENCES STATUS_NARUDZBE(idSTATUS)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_narudzba_racun FOREIGN KEY (Racun_idRACUN)
        REFERENCES RACUN(idRACUN)
        ON UPDATE CASCADE ON DELETE SET NULL
);

-- ==========================================
-- 9. Tabela: NARUDZBA_has_PROIZVOD
-- ==========================================
CREATE TABLE NARUDZBA_has_PROIZVOD (
    NARUDZBA_idNARUDZBA INT,
    PROIZVOD_idPROIZVOD INT,
    Kolicina INT NOT NULL,
    Cijena DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (NARUDZBA_idNARUDZBA, PROIZVOD_idPROIZVOD),
    CONSTRAINT fk_nhp_narudzba FOREIGN KEY (NARUDZBA_idNARUDZBA)
        REFERENCES NARUDZBA(idNARUDZBA)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_nhp_proizvod FOREIGN KEY (PROIZVOD_idPROIZVOD)
        REFERENCES PROIZVOD(idPROIZVOD)
        ON UPDATE CASCADE ON DELETE CASCADE
);

INSERT INTO KATEGORIJA (Naziv, Opis) VALUES
('Buketi', 'Razni cvjetni buketi'),
('Sobne biljke', 'Biljke za enterijer'),
('Dekoracije', 'Ukrasni dodaci i aranžmani');

INSERT INTO TIP_PLACANJA (Opis) VALUES ('Gotovina'), ('Kartica'), ('Online');
INSERT INTO STATUS_RACUNA (Opis) VALUES ('Plaćen'), ('Nepotpun'), ('Otkazan');


INSERT INTO ZAPOSLENI (Ime, Prezime, Kontakt, Pozicija, Username, Password)
VALUES
('Ana', 'Marković', '061111222', 'Administrator', 'ana.admin', '123'),
('Ivan', 'Petrović', '065333444', 'Referent', 'ivan.ref', '123');



INSERT INTO STATUS_NARUDZBE (Opis)
VALUES 
('U obradi'),
('Završeno'),
('Otkazano')
ON DUPLICATE KEY UPDATE Opis = VALUES(Opis);



INSERT INTO PROIZVOD (Naziv, Sifra, Kolicina, Cijena, Kategorija_idKATEGORIJA, Opis, Dostupan)
VALUES 
('Ruža crvena', 'RUZ001', 10, 15.50, 1, 'Svježa crvena ruža, idealna za poklon.', TRUE),
('Orhideja bijela', 'ORH002', 10, 35.00, 2, 'Elegantna bijela orhideja u saksiji.', TRUE),
('Buketa mješavina cvijeća', 'BUK003', 10, 25.75, 1, 'Šarena kombinacija sezonskog cvijeća.', TRUE),
('Sukulenti mix', 'SUK004', 10, 12.00, 2, 'Mini sukulenti za dekoraciju stola.', TRUE),
('Vaza staklena 20cm', 'VAZ005', 10, 18.90, 3, 'Prozirna staklena vaza visine 20cm.', TRUE),
('Dekorativni kamenčići', 'DEK006', 10, 8.50, 3, 'Pakovanje sitnih dekorativnih kamenčića.', TRUE),
('Tulipan žuti', 'TUL007', 10, 14.00, 1, 'Svježi žuti tulipani u pakovanju od 10 kom.', TRUE),
('Ciklama u saksiji', 'CIK008', 10, 22.50, 2, 'Sretna ciklama u modernoj saksiji.', TRUE);

ALTER TABLE NARUDZBA MODIFY ImeKupca VARCHAR(100);

ALTER TABLE NARUDZBA modify Napomena varchar(200);



