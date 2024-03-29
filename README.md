## FIT VUT - IPK Projekt 1
Autor: Jakub Jeřábek (xjerab28)

Úkolem tohoto projektu bylo vytvořit chat klienta k serveru, 
který podporuje komunikaci skrze TCP i UDP protokol.

Program je vytvořen v jazyku C# za použití systémových knihoven.
Je přiložen Makefile, který po použití příkazu *make* vytvoří binární
soubor nazvaný *ipk24chat-client*.

## Struktura projektu
Projekt se skládá z několika tříd a částí, které jsou nezbytné pro fungování
programu:
- **Program.cs** - Zobrazuje nápovědu, nastavuje debug mód, vytváří a
spouští instanci třídy *ChatClient*
- **ArgParser.cs** a **CommandLineSettings**.cs - Zajišťují zpracování argumentů
příkazové řádky a uložení těchto informací do struktury pro jednotný přístup.
- **ChatClient.cs** - Vytváří TCP nebo UDP klienta dle zvoleného argumentu, 
spouští příjem zpráv od serveru a zároveň ze standardního vstupu.
- **Client.cs** - Abstraktní třída, která definuje atributy a rozhraní společné pro TCP i UDP
klienta a zároveň implementuje metody, které mají oba tito klienti společné -
zejména zpracovávání zpráv od serveru.
- **Commands** - Obsahuje třídy pro všechny podporované příkazy. Každá z nich
implementuje stejné rozhraní ICommand, což zajišťuje jednotné provádění těchto
příkazů.
- **Exceptions** - Výjimky pro správnou činnost programu.
- **Factory - CommandFactory.cs** - Továrna na tvorbu instancí třídy *ICommand*
- **Messages** - Obsahuje třídu pro každý typ zprávy, každá z nich implementující
rozhraní *IMessage* pro jednotný převod do TCP stringu (_ToTcpString_) nebo UDP bytového pole (_ToUdpBytes_).
- **Logger.cs** - Pro výpis debugovacích zpráv, aktivován argumentem --debug.

## Nejdůležitější části projektu
Mezi páteř programu patří třídy *ClientTcp* a *ClientUdp*, které zprostředkovávají
komunikaci se serverem - odesílání a příjem zpráv.

Aby program fungoval dle automatu a nebylo klientovi umožněno posílat zprávy, dokud se
neautorizuje, je ve tříde _Client_ definována proměnná _IsAuthenticated_, která v případě obdržení první REPLY OK
zprávy se nastaví na _true_ a tím přechází do "stavu" _open_. 

Dále aby při čekání na REPLY odpověď nedocházelo k odesílání dalších zpráv, je definován
_TaskCompletionSource_ typu bool s názvem _ReplyReceivedTcs_ a semaforem _ReplySemaphore_. 
To zajišťuje synchronizaci mezi asynchronními metodami a čekáním tak, aby nedocházelo
k data racingu a nekonzistentnímu chování.

Obě třídy, _ClientTcp_ i _ClientUdp_ využívají systémové knihovny _System.Net.Sockets_, která
obsahuje _TcpClient_ a _UdpClient_. Tyto třídy jsou využívány pro navázaní spojení, odesílaní a 
příjem zpráv a také následné odpojení. 

Každá třída instance _IMessage_ obsahuje atribut _IsAwaittingReply_, který programu říká,
zda má po odeslání zprávy zabránit odeslání dalších zpráv, pokud se čeká na zprávu _REPLY_ ze serveru.

Zpracovávání příchozích zpráv zajišťuje metoda _HandleServerMessage_, která přijímá zprávu v poli bytů a tu
zpracovává dle specifikace, podle toho zda se jedná o UDP nebo TCP variantu. Chování klienta po
vyparsování jednotlivých zpráv je implementováno v abstraktní nadtřídě ve tvaru _HandleXMessage_, kde X je
typ zprávy, který může ze serveru přijít.

### UDP Klient

Varianta UDP klienta byla avšak na implementaci mnohem náročnější. Na rozdíl
od TCP klienta obsahuje další atributy:
- **MessageId** - Pro postupnou inkrementaci ID zpráv a uchovávání aktuálního ID.
- **Timeout** - Doba po které má dojít ke znovu-odeslání zprávy na server.
- **MaxRetries** - Počet pokusů znovu-odeslání zprávy.
- **IsAck** - Bool proměnná značící, zda byla obdržena _CONFIRM_ zpráva na právě odesílanou zprávu.
- **AckSemaphore, AckReceivedTcs** - Pomocné proměnné po korektní synchronizaci
mezi asynchronními metodami (zabránění data race, zachování konzistence).
- **ReceivedMessageIds** - Seznam ID již obdržených zpráv ze serveru, aby bylo zabráněno zpracovávání
duplicitních zpráv (duplicitních packetů)

Každé odeslání zprávy nastavuje _TaskCompletionSource_ pro přijmutí _CONFIRM_ zprávy. Poté se
v cyklu odešle zpráva, počká se zadaný _Timeout_ a zkontroluje se, zda je _IsAck_ nastaven na _true_.
Pokud ne, odesílá se další zpráva a inkrementuje čítač _retryCount_. V případě, že cyklus skončí a IsAck
je na _false_, nebo úloha _AckReceivedTcs_ nebyla dokončena, je vypsána chybová hláška o nedoručení zprávy.



## Teorie potřebná k porozumění
### TCP Varianta
TCP je spolehlivý, orientovaný na spojení protokol, který se používá u většiny síťových aplikací. Zajišťuje spolehlivý přenos skrze potvrzovací mechanismy, které také umožňují obnovení ztracených či duplicitních packetů. [1] Vhodný u aplikací, kde je vyžadováná spolehlivost a integrita dat - webové aplikace, e-mailové služby, webový prohlížeč atd. [3]

Implementace této varianty chat klienta nepřináší žádné větší režijní náklady jako u UDP varianty. TCP je mnohem vhodnějším protokolem pro chatovacího klienta, protože se nemůže žádná zprava ztratit, či zničit.
### UDP Varianta
UDP je jednoduchý protokol, umožňující přenost dat mezi hostiteli bez zajištění doručení či ochraně proti duplicitám [2]. Je vhodnější pro aplikace, které vyžadují rychlost na úkor spolehlivosti, tedy například u streamovacích služeb, online her či hlasové/video komunikace. [4]

Kvůli tomu, že není možné se dozvědět, zda zpráva byla doručena je důležité správně implementovat CONFIRM zprávy IPK24-CHAT protokolu, které tohle potvrzení zajišťují. To přináší poměrné drahé režijní náklady, protože klient musí sledovat, zda přišla odpověď a případně znovu zprávu odeslat. UDP tedy není pro chatovací aplikaci příliš vhodné. 
## Průběh testování
Pro testování celkové funkčnosti klienta byl využíván software _Wireshark_ s pluginem zobrazujícím
_IPK-24-CHAT_ protokol. 


### Automatické testy
Automatické testy byly implementovány pomocí knihovny _xUnit_ a testovaly pouze jednotlivé
funkce TCP a UDP klienta - _Unit testy_. Bylo zejména otestováno provádění jednotlivých příkazů,
správné odesílání zpráv, správné chování po přijmutí zprávy a reakce programu na neočekávaný vstup - 
neočekávaný počet parametrů u příkazu, příliš dlouhé parametry příkazu, neočekávané znaky apod. 

U TCP varianty byla využita instance třídy _TcpListener_ z knihovny _System.Net.Sockets_ pro
jednoduchý TCP server, získávání odeslaných zpráv a odesílání zpráv na klienta.

U UDP varianty byl v rámci testů vytvořen jednoduchý UDP server pro účely testování zmíněné výše.

Všechny tyto automatické testy v době psaní této dokumentace jsou označeny jako _Success_, splňují
tedy očekávané výstupy a chování.

### Manuální testy
Manuální testování probíhalo výhradně s otevřeným Wiresharkem a zapnutým _debug_ módem pro
výpis všech důležitých informací. Testováno bylo hlavně chování klienta při čekání na _REPLY_, či _CONFIRM_
zprávu, chování při odesílaní zpráv z neautorizovaného klienta (tj. neumožnění odeslání takové zprávy a vypsaní 
_ERR_ hlášky do terminálu) a chování při neočekávaných vstupech (tj. vypsání _ERR_ hlášky).

Pro TCP variantu byl využit _netcat_ server, manuální kontrola správných zpráv a kontrola čekání
na _REPLY_ zprávu. Z _netcat_ serveru byly odesílany zprávy zpět na klienta, sledování správnosti
ve Wiresharku a chování klienta. Tedy že klient správně vypisuje zprávy na výstup, v očekávaném a 
specifikovaném formátu.

V rámci UDP varianty byl vytvořen jednoduchý _Python_ server, který automaticky
odesílá CONFIRM zprávy na klienta a implementuje jednoduché příkazy pro odesílání
_MSG_, _BYE_, _ERR_, _REPLY OK_ a _REPLY NOK_ zpráv. Manuálně bylo testováno například
znovu-odesílání zpráv v případě neobdržení _CONFIRM_ zprávy (zakomentováním kódu v serveru) a chování
klienta při obdržení jednotlivých zpráv.

Byl kladen důraz na správný formát zpráv v terminálu (a správně zvolený _stdout_ nebo _stderr_), protože 
je dle mého názoru rozhodujícím při opravování - jakákoliv mezera, či znak navíc značí chybu.

### Akceptační testy
Akceptační testy měly simulovat průběh reálného testování projektu opravujícím, tj. bylo spuštěno
referenční prostředí, zadán příkaz _make_, spuštěn sestavený _ipk24chat-client_ s parametry
a přiloženým vstupem (pomocí "_< soubor.txt_"). 

Takhle spuštěný program byl znovu otestován na _netcat_ serveru (pro TCP), Python serveru
(pro UDP), na serveru zveřejněným kolegou (pro TCP) a na závěr na 
_anton5.fit.vutbr.cz_ serveru. Ve všech případech bylo sledováno chování programu
v terminálu (formát zpráv, správná reakce, pořadí těchto zpráv), i výpis ve Wiresharku.



## Zdroje
[1] RFC 793 - Transmission control protocol https://www.ietf.org/rfc/rfc0793.txt

[2] RFC 768 - User Datagram Protocol - https://www.ietf.org/rfc/rfc768.txt

[3] Wikipedia - Transmission control protocol - https://cs.wikipedia.org/wiki/Transmission_Control_Protocol

[4] Wikipedia - User Datagram Protocol - https://cs.wikipedia.org/wiki/User_Datagram_Protocol