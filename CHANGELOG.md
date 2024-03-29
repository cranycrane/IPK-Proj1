# Changelog

Všechny významné změny v tomto projektu budou dokumentovány v tomto souboru.

## 2024-02-27
### Přidáno
- Základní rozhraní projektu, rozhraní _Messages_, _Client_, _Command_

## 2024-03-08
### Přidáno
- Zpracovávání příkazů z standardního vstupu
- Implementace pro UDP variantu klienta - odeslání AUTH zprávy

## 2024-03-15
### Přidáno
- Implementován debug logger pro efektivní zapínání a vypínaní debugovacích výpisů

## 2024-03-23
### Přidáno
- Implementace TCP verze klienta spolu s _REPLY_ semaforem, který zajišťuje správné
čekání na _REPLY_ zprávu ze serveru a zablokování odesílání dalších zpráv

### Opraveno
- Synchronizace mezi asynchronními funkcemi

## 2024-03-26
### Přidáno
- Dokumentace k projektu a komentáře, makefile, příprava k odevzdání
### Opraveno
- Oprava chyb při obdržení ERR zprávy ze serveru - nebyla odeslána BYE zpráva

## 2024-03-29
### Opraveno
- Opravena chyba dvojitého ukočení při stistku CTRL+C - přidání stavové bool proměnné



### Známá omezení
- Aplikace může mít rozdílné chování na Linuxu a Windows (je efektivnější právě na Windows)
- Někdy příliš dlouhá prodleva, než klient odešle CONFIRM zprávu na server, možno způsobeno
firewallem či nastavením ve virtuálním stroji
- Klient neposílá BYE zprávu z AUTH stavu (pokud není autorizovaný), možno vyřešit 
stavovou bool proměnnou.