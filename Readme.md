Studentnaam: Mahmoud Fares

Studentnummer: 599485

# Algemene beschrijving van Chat Applicatie

Als opdracht, worden de studenten van de WIN-vak  gevraagd om een Multi clients chat applicatie te bouwen. Deze applicatie moet gebruik maken van het TCP-protocol als netwerk verkeer systeem.
Deze applicatie moet aan de volgende eisen voldoen:
- De gebruiker moet zich aanmelden en afmelden kunnen van de berichtenstroom.
- De gebruiker kan berichten versturen en ontvangen van meerdere gebruikers.
- Het systeem moet minimaal met 20 gebruikers kunnen omgaan. 
- De gebruiker kan de buffer grootte aanpassen. 
 
Voor de duidelijkheid heb ik mijn applicatie in twee aparte kanten verdeeld, namelijk als Server side en Client side applicatie. De server side is verantwoordelijk voor het starten en stoppen van de server. De server side applicatie is ook verantwoordelijk voor het ontvangen van de berichten van de gebruiker en doorsturen naar de andere deelnemers. Als een deelnemer zich afmeldt, wordt de deelnemer uit de deelnemers lijst weghaalt en aangegeven aan de andere deelnemers dat de deelnemer niet meer in berichtstroom zit. 
De Client side applicatie beidt de mogelijkheid aan om de gebruiker zich aan en af te melden van de berichtstroom. Verder kan de gebruiker met de Client side applicatie berichten ontvangen en versturen naar de server.


## Generics

Is een manier of techniek in de object oriented programmeertalen om klassen of methoden te kunnen maken die meerder types kunnen accepteren of meewerken. Generics geeft de mogelijkheid om de code met een brede scope te laten werken (Geen duplicate code voor elke specifice type) zonder dat het type saftey voordeel van de object oriented programmeertalen verliezen. 


Een goede en sterke voorbeeld van Genrics is de List klasse van c#. List klasse kan alle beschikbare typen in de C# accepteren en meewerken. 
Tijdens het initialisatie van de List klasse, moet de type die de List klasse zou verwachten bepaald worden. 

```sh
Voorbeeld van Generics list.
List<TestClass> testList = new List<TestClass>(); 
```

De bovenstaande code regel toont hoe de List klasse kan weten welke type moet hij later verwachten en behandelen. Bijvoorbeeld als we later de methode Add van de list klasse willen aanroepen en andere type van de gedefinieerde type meegeven willen, gaan de code klagen en een error geven (Type saftey). 

```sh
Goede code
tesList.Add(new TestClass());

Slechte code 
testList.Add(new TestClass2()); 
```

In de chat applicatie heb ik een paar keer gebruik gemaakt van de Generiks, Hieronder is een voorbeeld te zien. 

```sh
Eigen code voorbeeld 1 
public ObservableCollection<ClientOnServerSide> Clients { get; set; }
```

In de bovenstaande code, is er gebruik gemaakt van Observablecollection die gebruik maakt van generics. Is ook te zien dat de type die de ObservableCollection klasse moet verwachten is bepaald en meegegeven binnen de verglijkinstekens. 

* C# - Generics - Tutorialspoint. (n.d.). Retrieved February 20, 2020, from https://www.tutorialspoint.com/csharp/csharp_generics.htm

* Rai, A. (n.d.). C# | Generics - Introduction. Retrieved February 20, 2020, from https://www.geeksforgeeks.org/c-sharp-generics-introduction/

* Generics in C#. (n.d.). Retrieved February 20, 2020, from https://www.tutorialsteacher.com/csharp/csharp-generics

---

## Boxing & Unboxing

Integer, float, long en bool zijn de typen die in de stack scope op de geheugen leven. Deze typen zijn **Value Type** genoemd. De value typen hebben de value direct in de stack scope. Daarentegen hebben de **Reference Type** (Class, object, arrary, string etc) geen direct value in de stack scope maar wel een verwijzing naar een plaats in de heap scope. 
Het proces van het omzetten van een **Value Type** naar een **Reference Type**, wordt Boxing genoemd. 

```sh
Voorbeeld van Boxing

int valueType = 10;
string refrenceType = valueType.ToString(); 
```

In de bovenstaande voorbeeld is er een nieuw string **Reference Type** gemaakt met de waarde van de al gemaakte int **Value Type** Dit proces wordt Boxing genoemd.

```sh
Voorbeeld van Unboxing
Int valueType2 = Int32.Parse(referenceType); 
```

In de bovenstaande voorbeeld is er een nieuwe int **Value type** gemaakt met de waarde van een al gemaakte string **Reference Type** variable. Dit proces is De Unboxing. 


Boxing en Unboxing zijn heel erg duur. Waneer je bijvoorbeeld een nieuwe waarde boxen wilt, wordt een nieuwe plaats in de geheugen gereserveerd (extra ruimte). Theritisch kan het Boxing en Unboxing proces tot 20 keer langer duren dan een referentie aanpassen. Om het Boxing en Unboxing proces te kunnen vermijden, wordt er aangeraden om gebruik te maken van genric list collection. 

* Boxing and Unboxing - C# Programming Guide. (2015, July 20). Retrieved February 20, 2020, from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing

---
### Delegate & Invoke

Stel ervoor dat je een werkgever bent, en je hebt een werknemer die niets doet behalve van wat aan hem gevraagd is. Als je deze werknemer een functie wilt laten uitvoeren, dan moet je letterlijk uitleggen wat de werknemer moet doen, wat hij van je kan verwachten en wat hij moet inleveren als hij klaar is. Delegate is niets meer dan deze werknemer. Delegate is een refrence type die een functie krijgt en later uitvoert en de resultaat daarvan terug stuurt als het nodig. 

```sh
Voorbeel van Delegate 

Public delegate void DelMethodExample(int x, int y);
 
Public void AddMethod(int x, int y){
	console.write(x + y);
}

Public void DeleteMethode(int x, int y){
	console.log(x - y);
}

DelMethodExample addDelMethodExample = new DelMethodExample(DeleteMethode);
DelMethodExample deleteDelMethodExample = new DelMethodExample(DeleteMethode);


addDelMethodExample.invoke(4, 6);
deleteDelMethodExample .invoke(14, 4);

//Output 
//10
//10
```

In de bovenstaande voorbeeld wordt er een nieuwe instantie van de Delegate klasse gemaakt. Deze delegate verwacht een functie met twee parameters van de type int en geeft niets terug. Later zijn er twee instaties gemaakt van de DelMethodeExample type. Aan het einde zijn de invoke functie aangeroepen met de benodige parameters.
Om de delegate objecten aan te geven dat hij mag beginnen met het uitvoeren, is de functie invoek aangeroepen met de twee verwachtende parametes. 

* Delegates - C# Programming Guide. (2015, July 20). Retrieved February 20, 2020, from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/

---
Threading is een manier om de processor twee of meerdere functies tegelijkertijd te laten uitvoeren. Het gebruiken van van threading is heel handig, wanneer er bijvoorbeeld twee functies zijn die niet afhankelijk van elkaar zijn en moeten tegelijkertijd uitgevoerd worden.

Bijvoorbeel in mijn chat applicatie, had ik een functie nodig die een connectie maakt met een andere applicatie. Deze functie wordt geblokkeerd totdat een stabiele connectie maakt met de andere applicatie. Zonder het gebruik van multie threading zou de user interface in mijn applicatie geblokkeerd worden, totdat deze functie klaar is. Daarom heb ik gebruik gemaakt van de multi threading. 

```sh
voorbeeld van het gebruik van thread.

beginAcceptClientThread = new Thread(BeginAcceptClients);
beginAcceptClientThread.Start();
```
Op deze manier kon de user interface niet laten blokkeren en teglijkertijd kan de andere functie blijven aan het uitvoeren. 

Async/await  is een implementatie van C# om meerdere functies parallel te laten uitvoeren. In mijn code heb ik ook gebruik gemaakt van async/await. Hieronder is een voorbeeld. 

```sh
Async/await voorbeeld. 

await network.ReadAsync(streamParts, 0, Server.BufferSize);
```

* I Am Dev. (2016, September 20). What is threading in programming. Retrieved February 20, 2020, from https://www.youtube.com/watch?v=6mChjwcTZRU

* dotNET. (2020b, January 6). Introduction To Async, Await, And Tasks | C# Advanced  [5 of 8]. Retrieved February 20, 2020, from https://www.youtube.com/watch?v=X9N5r6kMOxw&t=3s

