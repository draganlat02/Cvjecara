# Cvjećara - Sistem za upravljanje cvjećarom

Dobrodošli u Cvjećaru – aplikaciju namijenjenu za efikasno upravljanje radom cvjećare, sa posebnim fokusom na korisničko iskustvo, personalizaciju i podršku za dva korisnička profila: administratora i referenta.

## Uvod

Ovaj priručnik je namijenjen svim korisnicima sistema Cvjećare – administratorima i referentima – i objašnjava kako koristiti aplikaciju za efikasno upravljanje radom cvjećare. Aplikacija omogućava vođenje evidencije o zaposlenima, narudžbama i proizvodima. Referenti mogu praviti narudžbe, pregledati sve narudžbe, izvršavati ili otkazivati narudžbe, dodavati proizvode u korpu, kreirati nove narudžbe. Admini upravljaju narudžbama, imaju dodatna prava za pregled narudžbi - prebacivanje narudžbi iz jednog stanja u drugo, upravljanje šifrarnicima zaposlenih, dodavanje novog zaposlenog, upravljanje proizvodima, dodavanje novih proizvoda. Svi korisnici imaju pristup personalizaciji – mogu promijeniti temu interfejsa (svijetlu, rozu ili ocean) i jezik aplikacije (srpski ili engleski). Sve postavke se automatski čuvaju i vraćaju prilikom naredne prijave. Aplikacija podržava dva tipa korisnika: 

##

| Tip korisnika        | Šta može raditi                                                                                          |
|----------------------|-----------------------------------------------------------------------------------------------------------|
| **Referent**            | • Pregled kataloga<br>• Pretraga i filteri<br>• Dodavanje u korpu<br>• Naručivanje<br>• Pregled narudžbi |
| **Zaposleni / Admin**| • Dodavanje i uređivanje proizvoda<br>• Pregled i promjena statusa svih narudžbi<br>• Upravljanje zaposlenima |


Svi korisnici mogu:
- Promijeniti temu: **Svijetla – Pink – Ocean** (tri kruga gore desno)
- Promijeniti jezik: **Srpski – Engleski** (dugmad **BS / EN**)
- Sve postavke se **automatski pamte** za sljedeću prijavu


### 1. Prijava u aplikaciju

![Prijava](Screenshots/01_login.png)

- Pokrenite aplikaciju **Cvjećara**
- Unesite svoje korisničko ime i lozinku
- Kliknite na **"Prijava"**
  Otvara se glavni ekran prema vašoj ulozi

## 2. REFERENT – kompletno uputstvo

### 2.1 Glavni katalog proizvoda

### 3.1 Glavni ekran – Katalog proizvoda 

![Glavni ekran referenta](Screenshots/02_glavni_ekran.png)

- Svi proizvodi prikazani u prelijepim karticama sa cijenama i opisima
- Pretraga u realnom vremenu – filtrira se odmah prilikom kucanja
- Filter po kategoriji (padajući meni – „Sve kategorije“, Buketi, Sobne biljke, itd.)
- Sortiranje na 4 načina:
  - Po nazivu A → Z
  - Po nazivu Z → A
  - Po cijeni od najjeftinijeg
  - Po cijeni od najskupljeg
- Dugme **„Dodaj u korpu“** na svakoj kartici

### 3.2 Kreiranje narudžbe za kupca

![Korpa](Screenshots/03_korpa.png)

1. Dodaj proizvode u korpu klikom na „Dodaj u korpu“
2. Broj pored korpe pokazuje koliko artikala se nalazi u korpi
3. Klikni na ikonu korpe → otvori se prozor korpe
4. Može:
   - Povećati/smanjiti količinu (+ / –)
   - Ukloniti artikl
   - Dodati napomenu (npr. „Hitno“, „Dostava na adresu...“, „Potrebno je dodati..“)
5. Klikni **„Naruči“** → narudžba se automatski kreira i šalje u sistem

### 3.3 Pregled svih narudžbi 

![Pregled narudžbi](Screenshots/04_narudzbe.png)

- Prikazuju se **SVE narudžbe** iz cvjećare
- Filteri sa prelijepim obojenim dugmadima:
  - **U obradi** → narandžasto (default aktivno)
  - **Završeno** → zeleno
  - **Otkazano** → crveno
- Pretraga po imenu kupca ili napomeni (automatski filtrira prilikom kucanja)
- Filter po periodu: 7 dana, 14 dana, 30 dana, 6 mjeseci, godina, sve
- Dvostruki klik na narudžbu → otvara detalje

### 3.4 Detalji narudžbe i promjena statusa

![Detalji narudžbe](Screenshots/05_detalji_narudzbe.png)

- Prikaz:
  - Ime kupca
  - Datum i vrijeme narudžbe
  - Lista stavki (naziv × količina)
  - Ukupna cijena
  - Napomena
- Tri velika dugmeta za brzu promjenu statusa:
  - **U obradi**
  - **Završeno**
  - **Otkazano**
- Ili dugme **„Promijeni status“** → otvori poseban prozor za odabir

Sve promjene se odmah pamte u bazi!

---
## 3. ZAPOSLENI / ADMINISTRATOR – sve mogućnosti

### 3.1 Pregled svih narudžbi

![Zaposleni – narudžbe](Screenshots/05_zaposleni_narudzbe.png)

- Prikaz SVIH narudžbi iz sistema
- Isti filteri kao kod kupca + mogućnost promjene statusa
- Dvostruki klik → detalji narudžbe

### 3.2 Promjena statusa narudžbe

![Promjena statusa](Screenshots/06_promjena_statusa.png)

- Dugme **„Promijeni status“**
- Opcije:
  - U obradi → Završeno → Otkazano
- Promjena se odmah vidi kod kupca

### 3.3 Dodavanje novog proizvoda

![Dodaj proizvod](Screenshots/07_dodaj_proizvod.png)

- Forma sa svim poljima: naziv, cijena, kategorija, opis, slika
- Validacija obaveznih polja
- Nakon spremanja – odmah vidljiv u katalogu

### 3.4 Uređivanje i brisanje proizvoda

![Uredi proizvode](Screenshots/08_uredi_proizvode.png)

- Tabelarni pregled svih proizvoda
- Dugmad „Uredi“ i „Obriši“
- Brzo pretraživanje

### 3.5 Upravljanje zaposlenima

![Upravljanje zaposlenima](Screenshots/09_upravljanje_zaposlenima.png)

- Dodavanje novih referenata/administratora
- Promjena lozinke
- Brisanje korisnika

---

## 4. Personalizacija i odjava

### Promjena teme i jezika

![Teme i jezik](Screenshots/10_teme_jezik.png)

- Gore desno:
  - Tri kruga → **Svijetla | Pink | Ocean** tema
  - Dugmad **BS / EN** → srpki / engleski jezik

### Odjava
- Ikona na dnu lijevog menija 

---
docs: dodan kompletan korisnički priručnik
