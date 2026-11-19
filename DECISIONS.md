# DECISIONS

Answer **yourself**, in your own words, in 3–5 sentences per question.
Refer to concrete files and classes from your own repo (for example `Features/Bookings/CreateBooking/...`).
An unanswered question gives 0 points.

## 1. A rule you moved
Pick one business rule (R1–R8). In which file did it live in the starter and in which file does it live now? Why there?

am ales R1, initial era in Services/RoomService.cs in Create, doua ifuri care aruncau exceptii daca numele era gol sau capacitatea nu era pozitiva. acum e in Domain/Room.cs in Room.Create, care intoarce Result<Room> cu o eroare in loc de exceptie. e acolo pt ca e o regula a camerei si nu poate exista o camera invalida

## 2. Result instead of an exception
Why is `Result` a better choice than an exception for "the room is occupied"? What happens in your code when an `Error` is returned?

am ales Result fiindca nu trebuie sa folosim exceptii pentru orice problema care apare (tocmai asta e Result Pattern). daca o camera e ocupata nu inseamna ca aplicatia are o problema. Room.Book intoarce un Error, handlerul nu mai salveaza nimic si ToProblem il face 409

## 3. Who knows about EF Core?
Which classes in your project know about EF Core and which do not? What do you gain and what do you pay with Repository + Unit of Work?

doar Infrastructure stie de EF Core, adica RoomHubDbContext si repository urile. comenzile vad doar interfetele din Domain, iar GetRoom si GetBooking citesc direct din DbContext. castig ca am trecut de la in memory la SQLite fara sa schimb handlerele, platesc ca am mai multe clase de scris

## 4. The flow of a request
Draw in text (ASCII arrows) the path of a `POST /api/rooms/{roomId}/bookings` request through your project, with the **real** names of files and classes, from the endpoint to the database.

POST /api/rooms/{roomId}/bookings
-> Features/Bookings/CreateBooking/CreateBooking.cs
-> CreateBookingHandler
-> EfRoomRepository (404 daca nu e camera)
-> EfBookingRepository
-> Room.Book
-> SaveChangesAsync (RoomHubDbContext) -> SQLite
-> 201 sau eroare cu ToProblem

## 5. What you rejected from what the AI gave you
Give a concrete example of code or an idea proposed by the AI that you corrected or rejected, and say why.

la primul prompt i am zis sa se opreasca dupa fiecare pas dar a inceput direct sa scrie cod. l am oprit si i am zis sa stearga tot, pt ca voiam intai sa stiu ce buguri sunt

## 6. If you had 30 more minutes
What would you improve and what risk did you leave in the project?

as fi facut versionarea si teste si as fi cautat singura mai multe buguri. ca risc, doua rezervari in acelasi timp pe aceeasi camera pot lua 409 chiar daca nu se suprapun, iar R9 nu e protejat daca aceeasi persoana rezerva in paralel pe camere diferite