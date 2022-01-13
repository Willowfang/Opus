---
layout: default
title: Komentokehote
nav_order: 5
---

# Komentokehote
{: .no_toc }

Ohjelmaa on mahdollista käyttää yksinkertaisiin toimintoihin komentokehotteen kautta. Tässä osiossa selostetaan relevantit komennot. Jos et tiedä, mikä on komentokehote, et todennäköisesti tule tarvitsemaan tässä osiossa selostettuja ohjeita.
{: .fs-6 .fw-300 }

## Sisällys
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Ohjelman avaaminen

Asennuskansion polku tallennettaan asennuksen yhteydessä käyttäjän PATH -ympäristömuuttujaan. Ohjelman saa siten käynnistettyä komentokehotteesta komennolla opus:

    C:\>opus
    
## Kaikkien kirjanmerkkien erotteleminen

Kaikkien kirjanmerkkien erottaminen tapahtuu komennolla `-split`, jolle annetaan argumenttina tiedoston sijainti (tarvittaessa lainausmerkeillä, jos polku sisältää välilyöntejä):

    C:\>opus -split "C:\Tiedosto.pdf"

Kirjanmerkit erotellaan omiksi tiedostoikseen tiedoston sijaintikansion alle luotavaan uuteen kansioon, joka nimetään annetun tiedoston mukaan niin, että nimeen lisätään oletusarvoinen tunnistemerkintä (esim. "_erotellut").

    C:\Tiedosto.pdf --> C:\Tiedosto_erotellut\Kirjanmerkki 1.pdf

## Valittujen kirjanmerkkien erotteleminen

Tietyllä merkkijonolla alkavien kirjanmerkkien erotteleminen tapahtuu antamalla edeltävälle komennolle toisena argumenttina haluttu merkkijono (esim. "Liite"):

    C:\>opus -split "C:\Tiedosto.pdf" "Liite"

Merkkijonolla alkavat kirjanmerkit tallennetaan omiksi tiedostoikseen edeltävässä kohdassa kuvattuun kansioon.

## Kaikkien kirjanmerkkien erotteleminen kansiosta

Komentokehotteen avulla on mahdollista myös erotella kaikki kirjanmerkit kansion ja sen alikansioiden sisältämistä tiedostoista rekursiivisesti:

    [Lähdekansio]
       |- Tiedosto1.pdf <- EROTELLAAN
       |-- [Kansio]
       |      |- Tiedosto2.pdf <- EROTELLAAN
       |      |-- [Kansio]
       |             |- Tiedosto3.pdf <- EROTELLAAN
       |-- [Kansio]
              |- Tiedosto4.pdf <- EROTELLAAN
       
    [Kohdekansio]
          |-- [Tiedosto1_erotellut]
          |             |- Kirjanmerkki 1.pdf
          |             |- Kirjanmerkki 2.pdf
          |-- [Tiedosto2_erotellut]
          |             |- Kirjanmerkki 1.pdf
          |-- [Tiedosto3_erotellut]
          |             |- Kirjanmerkki 1.pdf
          |             |- Kirjanmerkki 2.pdf
          |             |- Kirjanmerkki 3.pdf
          |-- [Tiedosto4_erotellut]
                        |- Kirjanmerkki 1.pdf

Kansiossa sijaitsevien tiedostojen kirjanmerkkien erotteleminen tapahtuu komennolla `-splitdir`, jolle annetaan argumenttina sen kansion sijainti, jonka alla olevat tiedostot halutaan ottaa käsittelyyn:

    C:\>opus -splitdir "C:\Tiedostot"

Kohdekansion valitsemiseksi aukeaa ikkuna. Jokaista tiedostoa varten luodaan kohdekansion alle oma kansionsa.

## Valittujen kirjanmerkkien erotteleminen kansiosta

Kansiosta on mahdollista erottaa rekursiivisesti tiedostoista myös vain tietyt kirjanmerkit, lisäämällä edeltävän kohdan komentoon vielä argumentin halutusta merkkijonosta:

    C:\>opus -splitdir "C:\Tiedostot\" "Liite"
    
Erotellut kirjanmerkit tallennetaan samanlaiseen rakenteeseen, kuin edeltävässä kohdassa on selostettu.