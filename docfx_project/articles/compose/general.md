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

## Hyväksytyt tiedostotyypit

- PDF (pdf)
- Word (doc, docx)

Muut hyväksytyt tiedostomuodot, kuin pdf, muunnetaan pdf-muotoon ennen niiden liittämistä osaksi muodostettavaa asiakirjaa.

---

> “We all make choices, but in the end... our choices make us.” - Andrew Ryan
