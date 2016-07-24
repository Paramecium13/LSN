# LSN
LSN is a reified scripting language that is designed for use in RPGs and similar games. It is statically typed and variables are immutable by default. Its syntax is very similar to (and inspired by) that of rust. Many of its features are inspired by the event system of RPG Maker VX Ace, and I would like to make it translate to RGSS3. Very early versions of it had that functionality, which is still visible in the #sub/#endsub directive, which would substitute their contents into the generated code.

LSN is a reified language; the reifier, LSNr, generates .NET objects from .lsn source files and saves them to the disk. These files can be loaded by a game and run using an implementation of *IInterpreter*.

LSN is meant to be used for higher level mechanics of a game, such as specific dialog, story interactions, quests, etc. It could also be used for parts of a combat system. It is not supposed to handle low level mechanics such as graphics, audio, user input, saving, etc. 

## Features
LSN is procedural and somewhat object oriented. All values, (strings, integers, doubles, etc.) are objects and will have methods. Users will be able to create custom types, structs, which contain read only fields, and records, which contain mutable fields and are passed by value. All values in LSN are passed by value (structs are technically kind of passed by reference because it doesn't make a difference). I am still working on reference type variables.

###Types
LSN is a strongly typed language and comes with several built in types and allows the creation of new types. All types will have methods (I plan to add a *ToString* method for all of them) and some have operators. LSN currently does not support creating new methods or operators for user defined types, though I may add this. I have not put much thought into polymorphism, inheritance, and type conversions but I would like LSN to support them in some manner.
####Built in Types
*bool*: A boolean value, is either true or false. It currently does not have any unique operators (I should add equals and not equals--and do something to optimise away the terrible *if(x == true)* code smell). The typical boolean operators, *or* (||) and (&&), and not (!), are not exclusive to *bool*s in LSN. Their use should be described elsewhere in this document.

*int*: A 32-bit signed integer value, wraps the .NET Int32 type. It currently has one method, *Abs*, which returns the absolute value. It has arithmetic operators for add (+), subtract (-), multiply (\*), divide (/), mod (%), and power (^) that take another *int* as the second operand and return an *int*. The *int* type also has the same arithmatic operators that take a *double* as the second operand and return a *double*. It also has comparison operators for greater than (>), less than (<), equal to (==), greater than or equal to (>=), less than or equal to (<=), and not equal to (!=) that take another *int* as the second operand and return a *bool*, and the same operators that take a *double* as the second operand and also return a *bool*. It also has a multiply (\*) operator that takes a *string* as the second operand and returns a new *string*, that is the other *string* repeated the value of the *int* times (I am unsure what negative values will do, they may cause an exeption to be thrown in the .NET code).

*double*: A double-precision floating-point value, wraps around the .NET *Double* type. It has two sets of arithmetic operators, one that takes an *int* as the second operand, and one that takes another *double*, both of which return a *double*. It also has two sets of comparison operators, one for *int*s, and one for *double*s, both of which return a *bool*. This apparent repetition of operators (e.g. *int* + *double* -> *double* and *double* + *int* -> double) is necessary due to the way operators are stored and accessed as well as the fact that not all of them are commutative.

*string*: Represents text in the Unicode format, wraps the .NET string type. *string* values in LSN are immutable. This allows them to be effectively passed by reference (passing a(n) LSN string value creates a new LSN string value that shares the same .NET string as the original), while it appears that they are passed by value. The *string* type has a multiplication (\*) operator that takes an *int* as the second operand and returns a *string* that is the *string* operand repeated the value of the *int* times. It also has equals (==) and not equals (!=) operators that take another *string* as the second operand and return a *bool*. I intend to add the other comparison operators to allow sorting.

#####Collection Types
Currently, none of the collections are fully functional. The syntax for initializing them has not been decided on and is not implemented by the reifier.

***Vector<T>***: An immutable collection of values of the same type that appears to be passed by value but is actually passed by reference. I suppose that making a vector of references should not be allowed. It currently has the methods *Length()* and *ToList()*. *Vector<int>* and *Vector<double>* also have the methods *Sum()* and *Mean()*.

***List<T>***: A mutable collection of variable length that is passed by reference. Like *Vector<T>*, all of its contents must be of the same type. It currently has the methods *Length* and *Add(value : T)*. *List<int>* and *List<double>* also have the methods *Sum()* and *Mean()*.

####Structs
Structs are user defined types that consist of named and typed members. They are immutable and effectively passed by reference, though they appear to be passed by value. Currently, there is no syntax for initializing values of a struct type.
####Records
Records are user defined types that, like structs, consist of named and typed members. They differ from structs in that they are mutable and passed by value. This means that when a record is passed to a function or method as an argument, the function recieves a new record with the same values. The function can make changes to that record but those changes are not made on the record that was passed to the function.
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

###Control Structures
Control the execution of their contents, which can be statements and/or other control structures.

If-elsif-else control, choice control, match control, loops...

### Expressions
Calculate a value.

###Quests
...

###References
References are LSN values that contain a .NET reference to an instance of the abstract class *LSN_ReferenceValue*. I have not worked out how the methods and fields of the referenced object will be accessed. 

I am thinking of just making them like regular values with the only difference being that they are passed by reference.

##Types of LSN Files
LSN source files end with the *.lsn* extension and object files end with the *.dat* extension. There are currently five (ten if source and object files are counted separately) different types of LSN files planned. Only three, *script*, *scene* and *scriptlet*, will be able to be directly interpreted, the others will have to be included(#include) or imported(#import). Currently, the reifier can only create *script* and *resource* object files.

All object files can contain a collection of included types and functions that were not defined in it.
This is done using the #include directive. They can also tell the interpreter to load resource and quest files at runtime, with the #using directive. The reifier will also load these resources to check type and function usage.

###Scripts
Contains a sequence of statements and control structures that are executed by the interpreter.

###Scene
Similar to scripts but is passed arguments by the interpreter. For example, the scene for a chest could be passed the character who opened it and the chest itself so it can give the character the chest's items and make the chest empty.

###Resources
Contains structs definitions, functions, and exported inline substitutions.

###Quest Files
Contains definitions of quests. The reason they are not put in resource files is that quests should not be #include 'd.

### Scriptlets
Other than #import 's and #using 's, they only contain a single expression, the result of which is returned by the interpreter.
