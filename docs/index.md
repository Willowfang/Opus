# Sisällysluettelo

### Yleistä

1. #### [Opuksesta](#opuksesta-yleisesti)
   1.1. [Lähdekoodi](#lähdekoodi)
   
   1.2. [Lisenssi](#lisenssi)
   
   1.3. [Avoimen lähdekoodin riippuvuudet](#avoimen-lähdekoodin-riippuvuudet)

### Kirjanmerkkien erotteleminen

 2. #### [Käyttöliittymä](#kirjanmerkkien-erottaminen-käyttöliittymässä)
    2.1. [Avaa Opus](#avaa-opus)
    
    2.2. [Avaa tiedosto](#avaa-tiedosto)
    
    2.3. [Valitse kirjanmerkkejä](#valitse-kirjanmerkkejä)

<br/>

---

## Opuksesta yleisesti

_**[Sisällysluettelo](#sisällysluettelo)**_

Opus on ohjelma, joka helpottaa toistuvien työnkulkuun liittyvien tehtävien tekemistä. Sen avulla voit muun muassa poistaa .pdf -tiedostoista allekirjoituksia sekä erotella niistä liitteitä tai koota niitä yhteen. Ohjelma sisältää graafisen käyttöliittymän tarkempaan työskentelyyn, mutta yksinkertaisimmat toiminnot on mahdollista toteuttaa myös komentokehotteen kautta. Lisäksi valmis asennus tukee mainittuja toimintoja Windowsin Context Menun kautta eli klikkaamalla oikealla hiiren näppäimellä tiedostoja tai kansioita.

### Lähdekoodi

Opus on avoimen lähdekoodin ohjelma. Kyseinen lähdekoodi on saatavilla GitHub -palvelusta klikkaamalla sivun vasemmassa laidassa olevaa linkkiä. Opus on riippuvainen muutamasta hyvin tunnetusta avoimen lähdekoodin kirjastosta.

### Lisenssi

Opus on lisensoitu GNU Affero General Public License 3.0 -lisenssillä. Ohjelma ja sen lähdekoodi ovat vapaasti käytettävissä, jaettavissa ja muokattavissa mainitun lisenssin ehtojen mukaisesti.

* [GNU Affero Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.html)

### Avoimen lähdekoodin riippuvuudet

#### [MaterialDesignThemes](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [MIT License](https://licenses.nuget.org/MIT)

#### [Prism](https://github.com/PrismLibrary/Prism)
- [MIT License](https://www.nuget.org/packages/Prism.Unity/8.0.0.1909/license)

#### [iText7](https://itextpdf.com/)
- [AGPL 3.0](https://www.gnu.org/licenses/agpl-3.0.html)

#### [LiteDB](https://github.com/mbdavid/LiteDB)
- [MIT License](https://github.com/mbdavid/LiteDB/blob/master/LICENSE)

<br/>

---

## Kirjanmerkkien erottaminen käyttöliittymässä

_**[Sisällysluettelo](#sisällysluettelo)**_

Opuksen avulla kirjanmerkkejä voi erottaa graafisessa käyttöliittymässä, klikkaamalla hiiren oikealla painikkeella kohdetta resurssienhallinnassa tai käyttämällä komentoriviä.
Tässä osiossa kerrotaan, kuinka kirjanmerkkien erottelu tapahtuu graafisen käyttöliittymän avulla.

### Avaa Opus

Kirjoita Windowsin hakukenttään "opus" ja oikean tuloksen kohdalla paina Enter tai klikkaa kuvaketta.

**TAI**

Klikkaa aloitusvalikkoa (ikkunan kuva) työpöydän vasemmassa alalaidassa ja etsi selaamalla valikosta O-kirjaimen kohdalta Opus. Klikkaa kuvaketta.

### Avaa tiedosto

1. Klikkaa ruudun ylälaidassa keskellä olevaa painiketta "Avaa tiedosto". Tiedostonvalintaikkuna aukeaa.
2. Etsi tiedosto, jonka haluat avata ja tuplaklikkaa sitä tai klikkaa sitä kerran ja valitse sitten "Avaa".
3. Tiedosto aukeaa ohjelmaan ja sen sisältämät kirjanmerkit luetellaan alempana kirjanmerkkiruudussa.

<img src="https://codex-fi.github.io/Opus/ui/gif/open_file.gif" width="400"><br/><br/>

### Valitse kirjanmerkkejä

1. Klikkaa kirjanmerkkiruudusta niitä kirjanmerkkejä, jotka haluat erottaa. Kyseiset kirjanmerkit tulevat maalatuiksi. 
2. Jos haluat poistaa tekemäsi valinnan, klikkaa merkkiä uudestaan. 
3. Jos valitset ylätason merkin, myös alatason merkit valitaan.
4. Alaosan painikkeista voit valita kerralla kaikki kirjanmerkit tai poistaa kaikki valinnat.

<img src="https://codex-fi.github.io/Opus/ui/gif/select_bookmarks.gif" width="400"><br/><br/>

[Sisällysluetteloon](#sisällysluettelo)
