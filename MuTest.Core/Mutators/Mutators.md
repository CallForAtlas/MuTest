MuTest supports a variety of mutators, which are listed below.
###### Note: Method Call Mutator is considering new mutants if there is no any other mutant at specific line of code


<!-- TOC -->
- [Arithmetic Operators](#arithmetic-operators)
- [Equality Operators](#equality-operators)
- [Boolean Literals](#boolean-literals)
- [Assignment statements](#assignment-statements)
- [Unary Operators](#unary-operators)
- [Update Operators](#update-operators)
- [Linq Methods](#linq-methods)
- [String Literals](#string-literals)
- [Method Calls](#method-calls)
<!-- /TOC -->

Default Mutators:

<!-- TOC -->
- [Arithmetic Operators](#arithmetic-operators)
- [Equality Operators](#equality-operators)
- [Assignment statements](#assignment-statements)
- [String Literals](#string-literals)
- [Method Calls](#method-calls)
<!-- /TOC -->

## Arithmetic Operators
| Original | Mutated | 
| ------------- | ------------- | 
| `+` | `-` |
| `-` | `+` |
| `*` | `/` |
| `/` | `*` |
| `%` | `*` |

## Equality Operators
| Original | Mutated | 
| ------------- | ------------- |
| `>` | `<` |
| `>` | `>=` |
| `>=` | `<` |
| `>=` | `>` |
| `<` | `>` |
| `<` | `<=` |
| `<=` | `>` |
| `<=` | `<` |
| `==` | `!=` |
| `!=` | `==` |

## Logical Operators
| Original | Mutated | 
| ------------- | ------------- | 
| `&&` | `\|\|` | 
| `\|\|` | `&&` |

## Boolean Literals
| Original | Mutated | 
| ------------- | ------------- | 
| `true`	| `false` |
| `false`	| `true` |
| `!person.IsAdult()`		| `person.IsAdult()` |
| `if(person.IsAdult())` | `if(!person.IsAdult())` |
| `while(person.IsAdult())` | `while(!person.IsAdult())` |

## Assignment Statements
| Original | Mutated | 
| ------------- | ------------- | 
|`+= `	| `-= ` |
|`-= `	| `+= ` |
|`*= `	| `/= ` |
|`/= `	| `*= ` |
|`%= `	| `*= ` |
|`<<=`  | `>>=` |
|`>>=`  | `<<=` |
|`&= `	| `\|= ` |
|`\|= `	| `&= ` |

## Unary Operators
|    Original   |   Mutated  | 
| ------------- | ---------- | 
| `-variable`	| `+variable`|
| `+variable` 	| `-variable`|
| `~variable` 	| `variable` |

## Update Operators
|    Original   |   Mutated  | 
| ------------- | ---------- | 
| `variable++`	| `variable--` |
| `variable--`	| `variable++` |
| `++variable`	| `--variable` |
| `--variable`	| `++variable` |

## Linq Methods
|      Original         |       Mutated         |
| --------------------- | --------------------- |
| `SingleOrDefault()`   | `FirstOrDefault()`    |
| `FirstOrDefault()`    | `SingleOrDefault()`   |
| `First()`             | `Last()`              |
| `Last()`              | `First()`             |
| `All()`               | `Any()`               |
| `Any()`               | `All()`               |
| `Skip()`              | `Take()`              |
| `Take()`              | `Skip()`              |
| `SkipWhile()`         | `TakeWhile()`         |
| `TakeWhile()`         | `SkipWhile()`         |
| `Min()`               | `Max()`               |
| `Max()`               | `Min()`               |
| `Sum()`               | `Count()`             |
| `Count()`             | `Sum()`               |

## String Literals
| Original | Mutated |
| ------------- | ------------- | 
| `"foo"` | `""` |
|  `""` | `"MuTest"` |
| `$"foo {bar}"` | `$""` |
| `@"foo"` | `@""` |

## Method Calls
| Original | Mutated |
| ------------- | ------------- |
| `ActionOne();` | `;` |
| `OpenConnection();` | `;` |
