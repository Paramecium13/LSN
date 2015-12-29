# LSN
A reified scripting language designed for use in RPGs and similar games.	
It is statically typed and variables are immutable by default. Its syntax is very similar to (and inspired by) that of rust.

LSN is a reified language; the reifier, LSNr, generates .NET objects from .lsn source files and saves them to the disk. 

LSN is meant to be used for higher level mechanics of a game, such as specific dialog, story interactions, quests, etc. It could also be used for parts of a combat system. It is not supposed to handle low level mechanics such as graphics, audio, user input, saving, etc. 


## Features
LSN is procedural and somewhat object oriented. All values, (strings, integers, doubles, etc.) are objects and will have methods.
Most values in LSN are passed by value, including numbers, and strings. I am still working on reference type variables.

### Collections
####Vector
An immutable collection of values of the same type that is passed by value.