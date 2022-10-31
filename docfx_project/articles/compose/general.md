# Yleistä

---

## Toiminnon tarkoitus

Asiakirjan kokoamistoiminnolla on mahdollista koota asiakirjoja kansion sisältämistä erillisistä asiakirjoista määriteltyjen sääntöjen mukaan. Näin ollen on mahdollista tuottaa asiakirjoja muilla ohjelmilla ja nimetä tuotetut asiakirjat määritellyn kaavan mukaan ja sen jälkeen yhdistää Opuksella kyseiset yksittäiset asiakirjat valmiiksi kokonaisuudeksi.

**Esimerkki**

Kansio sisältää seuraavat, muilla ohjelmilla tuotetut tiedostot ja alikansiot (K = kansio, T = tiedosto):

```
K: ASIA 127763
    T: Kansilehti.pdf
    K: SELVITYKSET
        T: Selvitys, Matti Möttönen.docx
        T: Selvitys, Mikki Hiiri.docx
        T: Selvitys, Hessu Hopo.docx
    K: LIITTEET
        T: Liite 1, Kuitti.pdf
        T: Liite 2, Lasku.pdf
        T: Liite 3, Valokuvat.pdf
        T: Liite 4, Vaatimukset.pdf
```

Toiminnolla on mahdollista koota edeltävät tiedostot automaattisesti yhteen tiedostoon esimerkiksi siten, että kansilehti on omana yläkirjanmerkkinään, sen jälkeen kaikki selvitykset on koottu oman yläkirjanmerkkinsä alle ja vastaavasti liitteet oman yläkirjanmerkkinsä alle. Kokoaminen yhteen lopputiedostoon voisi siis kirjanmerkkien puolesta näyttää esimerkiksi seuraavalta:

```
Kansilehti
Selvitykset
    Selvitys, Matti Möttönen
    Selvitys, Mikki Hiiri
    Selvitys, Hessu Hopo
Liitteet
    Liite 1, Kuitti
    Liite 2, Lasku
    Liite 3, Valokuvat
    Liite 4, Vaatimukset
```

## Periaatteet pääpiirteissään

Asiakirjoja etsitään kansiosta (ja [asetuksista](settings.md#1-tiedostojen-etsiminen-alikansioista) riippuen myös sen alikansioista) annettujen [säännöllisten lausekkeiden (regular expression, regex)](https://fi.wikipedia.org/wiki/Säännöllinen_lauseke) perusteella. Säännöllisiä lausekkeita voi muotoilla ja testata esimerkiksi [täällä](https://regexr.com/).

Kun säännöllistä lauseketta vastaava tiedosto löytyy, sen sisältö liitetään osaksi muodostettavaa asiakirjaa ja sille annetaan kirjanmerkki, joka nimetään joko etsintäkriteereissä annetulla nimellä tai tiedoston nimen mukaan. Kirjanmerkki järjestetään kirjanmerkkipuuhun annetun rakenteen mukaisesti.

Mikäli pakolliseksi merkattua asiakirjaa ei löydetä tai kriteerien mukaisia asiakirjoja löydetään liikaa, käyttäjää pyydetään etsimään puuttuvat tiedostot tai valitsemaan, mikä löydetyistä tiedostoista on oikea.

Koska etsintä perustuu säännöllisiin lausekkeisiin, periaatteessa hyvinkin monimutkaiset etsintäkriteerit ovat mahdollisia.

## Profiilit ja osiot

Asiakirjojen haun kriteerit sisältyvät profiileihin, jotka on puolestaan jaettu osioihin. Jokainen osio vastaa yhtä tai useampaa kirjanmerkkiä valmiissa, kootussa tiedostossa.

Osiot voivat olla tiedosto- tai otsikko-tyyppisiä. Tiedosto-osioita vastaavasti haetaan annetusta kansiosta tiedostoja. Kyseiset osiot voivat olla vapaaehtoisia tai pakollisia riippuen siitä, millainen vaadittujen tiedostojen määrä niille on asetettu.

Otsikko-osioiden perusteella ei haeta tiedostoja, vaan niitä käytetään ainoastaan ylätason kirjanmerkkeinä valmiissa asiakirjassa. Otsikko-osiot lisätään asiakirjaan, mikäli ne sisältävät alakirjanmerkkejä (eli mikäli niiden alla on tiedosto-osioita, joiden perusteella on löydetty asiakirjoja).

## Hyväksytyt tiedostotyypit

- PDF (pdf)
- Word (doc, docx)

Muut hyväksytyt tiedostomuodot, kuin pdf, muunnetaan pdf-muotoon ennen niiden liittämistä osaksi muodostettavaa asiakirjaa.

## Esiasennetut koontiprofiilit

Koontiprofiileja voidaan toimittaa käyttäjille myös asennustiedoston mukana sisällyttämällä ne asennuksen (tai päivityksen) yhteydessä tai jälkikäteen muulla tavalla ohjelman asennuskansion alle kansioon "ProfileImport". Nämä profiilit tuodaan ohjelma avattaessa käyttäjän valittavaksi.

Näin tuotavat profiilit on mahdollista asettaa myös tilaan, joka estää niiden muokkaamisen tai poistamisen. Näin voidaan järjestelmänvalvojan toimesta asennuttaa käyttäjille sellaisia profiileja, jotka ovat organisaation yhteisen käytännön mukaisia ja joita ei siis käyttäjän toimesta voida muuttaa.

Profiilitiedostot ovat .opusprofile -päätteisiä ja JSON-muotoisia tiedostoja, joten niiden muokkaaminen on mahdollista myös tavallisilla tekstinkäsittelyohjelmilla.

---

> “We all make choices, but in the end... our choices make us.” - Andrew Ryan
