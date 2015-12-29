# LSN
A reified scripting language designed for use in RPGs and similar games. It is statically typed and variables are immutable by default. Its syntax is very similar to (and inspired by) that of rust. Many of its features are inspired by the event system of RPG Maker VX Ace, and I would like to make it translate to RGSS3. Early versions of it had that functionality, which is still visible in the #sub/#endsub directive, which would substitute their contents into the generated code.

LSN is a reified language; the reifier, LSNr, generates .NET objects from .lsn source files and saves them to the disk. These files can be loaded by a game and run using an implementation of *IInterpreter*.

LSN is meant to be used for higher level mechanics of a game, such as specific dialog, story interactions, quests, etc. It could also be used for parts of a combat system. It is not supposed to handle low level mechanics such as graphics, audio, user input, saving, etc. 

## Features
LSN is procedural and somewhat object oriented. All values, (strings, integers, doubles, etc.) are objects and will have methods. Users will be able to create custom value types, structs, which contain read only fields. I may make it possible to make a mutable struct type--note that making the variable containing the struct mutable will not make the struct itself mutable.
All values in LSN are passed by value. I am still working on reference type variables.

###Statements
LSN will have many different statements, though currently only a few are implemented.

####General statements
The interpretation of these statements is implemented by the abstract Interpreter class.

* Assignment: Assign a value to new variable, can mark it as mutable.
* Reassignment: Assign a new value to an existing variable, if the variable was not declared mutable, the reifier will throw/log an exception.
* Function/Method call: Calls a function or method without doing anything with the result.
####Game specific statements
The interpretation of these statements must be implemented by the game specific interpreter.

* Give item/weapon/armor/gold
* GOTO: Change the location of the player, or an other character on the current map or a different map.
###References
References are LSN values that contain a .NET reference to an instance of the abstract class *LSN_ReferenceValue*. I have not worked out how the methods and fields of the referenced object will be accessed.

### Collections
Currently, none of the collections are fully functional. The syntax for initializing them, accessing their values, and (if applicable  modifying them) has not been decided on and is not implemented by the reifier.

####Vector
An immutable collection of values of the same type that is passed by value. I suppose that making a vector of references should not be allowed.

####List
A mutable collection of variable length that is passed by reference. Like *vector*, all of its contents must be of the same type.


##Types of LSN Files
LSN source files end with the *.lsn* extension and object files end with the *.dat* extension. There are currently five (10 if source and object files are counted separately) different types of LSN files planned. Only two, *script* and *scriptlet*, will be able to be directly interpreted, the others will have to be included(#include) or imported(#import). Currently, the reifier can only create *script* object files.
