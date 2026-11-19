# BUGS

The starter contains behaviors that contradict `ASSIGNMENT.md`. Write here every discrepancy you found.
Fill it in yourself, in your own words. There is no required number: what counts is that they are real.

| Which rule from ASSIGNMENT.md is violated (R1–R8) | Where the problem was in the starter (file, approximate line) | How you discovered it (test, reading, AI) | What you changed |
|---|---|---|---|
| R7 intervalele care se ating erau luate ca suprapuse | BookingService, pe la linia 28 | AI ul, si l am confirmat live: 10 12 si 12 14 dadeau 409 | Overlaps din DateRange foloseste < strict |
| R7 R8 rezervarile anulate inca blocau intervalul | BookingService, pe la linia 26 | AI ul, confirmat live: dupa cancel nu puteai rezerva iar acelasi interval | Room.Book ia doar rezervarile Active |
| R8 cancel de doua ori dadea 204 |bookingsrvice, la Cancel | l am vazut eu citind codul, Cancel doar seta statusul fara nicio verificare | Booking.Cancel da 409 daca e deja anulata |
| R5 rezervare in trecut | bookingservice, la create | l am vazut eu citind codul(de aseara) nu era nicio comparatie cu data curenta | verificare cu nowUtc din TimeProvider, da 400 |
| R6 nu exista limita de 4 ore | BookingService, la Create | AI ul, confirmat live: 5 ore lua 201 | verificare pe Duration din DateRange, exact 4h e permis |
| erorile trebuiau sa fie ProblemDetails | Program.cs | AI ul, confirmat live: json invalid dadea stack trace | AddProblemDetails si toate erorile sunt problem+json |