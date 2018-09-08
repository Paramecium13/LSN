# LSN
LSN is a reified scripting language that is designed for use in RPGs and similar games. It is statically typed and variables are immutable by default. Its syntax is very similar to (and inspired by) that of rust. 

LSN is a reified language; the reifier, LSNr, generates .NET objects from .lsn source files and saves them to the disk in a custom binary format. These files can be loaded by a game, using the provided method(s) to parse the binary format, and run using an implementation of *IInterpreter*.

LSN is meant to be used for higher level mechanics of a game, such as specific dialog, story interactions, quests, etc. It could also be used for parts of a combat system. It is not supposed to handle lower level mechanics such as graphics, audio, user input, saving, etc. 

## Features
LSN is procedural and somewhat object oriented. All values, (strings, integers, doubles, etc.) are objects and will have methods. Users are able to create custom types, records, which contain read only fields, and structs, which contain mutable fields and are passed by value. All values in LSN are passed by value (records are technically kind of passed by reference because it doesn't make a difference). 

### Types
LSN is a strongly typed language and comes with several built in types and allows the creation of new types. All types will have methods (I plan to add a *ToString* method for all of them) and some have operators. LSN currently does not support creating new methods or operators for user defined types, though I may add this. I have not put much thought into polymorphism, inheritance, and type conversions but I would like LSN to support them in some manner.
#### Built in Types
*bool*: A boolean value, is either true or false. It currently does not have any unique operators (I should add equals and not equals--and do something to optimise away the terrible *if(x == true)* code smell). The typical boolean operators, *or* (||) and (&&), and not (!), are not exclusive to *bool*s in LSN. Their use should be described elsewhere in this document.

*int*: A 32-bit signed integer value, wraps the .NET Int32 type. It currently has one method, *Abs*, which returns the absolute value. It has arithmetic operators for add (+), subtract (-), multiply (\*), divide (/), mod (%), and power (^) that take another *int* as the second operand and return an *int*. The *int* type also has the same arithmatic operators that take a *double* as the second operand and return a *double*. It also has comparison operators for greater than (>), less than (<), equal to (==), greater than or equal to (>=), less than or equal to (<=), and not equal to (!=) that take another *int* as the second operand and return a *bool*, and the same operators that take a *double* as the second operand and also return a *bool*. It also has a multiply (\*) operator that takes a *string* as the second operand and returns a new *string*, that is the other *string* repeated the value of the *int* times (I am unsure what negative values will do, they may cause an exeption to be thrown in the .NET code).

*double*: A double-precision floating-point value, wraps around the .NET *Double* type. It has two sets of arithmetic operators, one that takes an *int* as the second operand, and one that takes another *double*, both of which return a *double*. It also has two sets of comparison operators, one for *int*s, and one for *double*s, both of which return a *bool*. This apparent repetition of operators (e.g. *int* + *double* -> *double* and *double* + *int* -> double) is necessary due to the way operators are stored and accessed as well as the fact that not all of them are commutative.

*string*: Represents text in the Unicode format, wraps the .NET string type. *string* values in LSN are immutable. This allows them to be effectively passed by reference (passing a(n) LSN string value creates a new LSN string value that shares the same .NET string as the original), while it appears that they are passed by value. The *string* type has a multiplication (\*) operator that takes an *int* as the second operand and returns a *string* that is the *string* operand repeated the value of the *int* times. It also has equals (==) and not equals (!=) operators that take another *string* as the second operand and return a *bool*. I intend to add the other comparison operators to allow sorting.

##### Collection Types
I'm not sure how functional the collections currently are. It should be possible to access elements of vectors and lists and modify the elements of lists. Empty lists can be constructed with a constructor expression. The syntax for initializing non-empty collections has not been decided on and thus is not implemented by the reifier.

***Vector<T>*** : An immutable collection of values of the same type that appears to be passed by value but is actually passed by reference. I suppose that making a vector of references should not be allowed. It currently has the methods *Length()* and *ToList()*. *Vector<int>* and *Vector<double>* also have the methods *Sum()* and *Mean()*. Currently, vectors can only be obtained by calling the *ToVector* method on a list or by being passed through an event or function by the game engine or as the return value of a host interface method.

***List<T>*** : A mutable collection of variable length that is passed by reference. Like *Vector<T>*, all of its contents must be of the same type. It currently has the methods *Length*, *Add(value : T)*, and *ToVector*. *List<int>* and *List<double>* also have the methods *Sum()* and *Mean()*.

#### Records
Records are user defined types that consist of named and typed members. They are immutable and effectively passed by reference, though it doesn't matter as they are immutable.
#### Structs
Structs are user defined types that, like records, consist of named and typed members. They differ from records in that they are mutable and passed by value. This means that when a struct is passed to a function or method as an argument, the function recieves a new struct with the same values. The function can make changes to that struct but those changes are not made on the struct that was passed to the function.
### Statements
A statement tells the interpreter to do something, such as store a value as a variable or display a message to the player. LSN will have many different statements, though currently not all of them are implemented.

#### General statements
The interpretation of these statements is implemented by the abstract Interpreter class.

* Assignment: Assign a value to new variable, can mark it as mutable.
* Reassignment: Assign a new value to an existing variable; if the variable was not declared as mutable, the reifier will report an error.
* Function/Method call: Calls a function or method without doing anything with the result.

See the wiki for more information on these and other statements.
#### Game specific statements
The interpretation of these statements must be implemented by the game specific interpreter.

* Give item/gold
* GOTO: Change the location of the player, or an other character on the current map or a different map.
* Say: Display a message on the screen.

### Control Structures
Control structures are used to control the flow of script execution. They control the execution of their contents, which can be statements and/or other control structures. LSN currently supports some of the common control structures, namely If-else controls, while loops, and for loops. Additionaly, LSN offers the *choice control*, which displays a list of choices to the player and, when they make their choice, executes the code corresponding to that choice.

If-elsif-else control, choice control, match control, loops...

### Expressions
Expressions calculate a value.

## LSN Files
LSN source files end with the *.lsn* extension and object files end with the *.obj* extension, though this last extension is configurable in the main file.

LSN files also tell the interpreter to load other LSN files at runtime, with the #using directive. The reifier will also load these other files to check type and function usage. However, circular dependencies are not allowed. For example, if file 'a' depends on file 'b' (i.e. has a #using directive for it), then neither file 'b' nor any file that file 'b' depends on may depend on file 'a' or any file that depends on file 'a'. In other words, the graph of file dependencies must be a [directed acyclic graph](https://en.wikipedia.org/wiki/Directed_acyclic_graph).

LSN files contain struct and record definitions, functions, Host Interfaces, and Script Classes.

## The Main File
The main file is a .json file that is passed as a command line argument to the reifier. It serves as a marker for an *LSN project* and contains varous settings for that project. Currently, the only setting that is implemented and used is what extension the output object files will have. An *LSN project* consists of a main file and, in the same directory, a subdirectory named 'src' that contains all the LSN source files.
