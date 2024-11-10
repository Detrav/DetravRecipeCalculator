# Expressions

The original component: <https://github.com/matheval/expression-evaluator-c-sharp/blob/main/README.md>

## Supported Operators

| Operator | Description                                                                 |
| -------- | --------------------------------------------------------------------------- |
| `+`      | Add, unary plus, concatenate strings, datetime addition                     |
| `&`      | Concatenate strings                                                         |
| `–`      | Subtract, unary minus, datetime subtraction                                 |
| `*`      | Multiply (can be omitted before an open bracket)                            |
| `/`      | Divide                                                                      |
| `%`      | Modulo                                                                      |
| `^`      | Power                                                                       |

## Supported Conditional Statements

| Statement                                                      | Description                                                     |
| -------------------------------------------------------------- | --------------------------------------------------------------- |
| `IF(logical_condition, value_if_true, value_if_false)`         | Example: `IF(2>1,"Pass","Fail")`                                |
| `SWITCH(expression, val1, result1, [val2, result2], …, [default])` | Example: `SWITCH(3+2,5,"Apple",7,"Mango",3,"Good","N/A")`     |

## Supported Logical and Math Functions

| Function                          | Description                                                                |
| --------------------------------- | -------------------------------------------------------------------------- |
| `AND(logical1, [logical2], …)`    | Returns TRUE if all conditions are TRUE                                    |
| `OR(logical1, [logical2], …)`     | Returns TRUE if any condition is TRUE                                      |
| `NOT(logical)`                    | Returns TRUE if the condition is FALSE                                     |
| `XOR(logical1, [logical2], …)`    | Returns TRUE if an odd number of arguments are TRUE                        |
| `SUM(number1, [number2],…)`       | Adds the numbers provided                                                  |
| `AVERAGE(number1, [number2],…)`   | Returns the average of numbers provided                                    |
| `MIN(number1, [number2],…)`       | Returns the smallest value                                                 |
| `MAX(number1, [number2],…)`       | Returns the largest value                                                  |
| `MOD(number, divisor)`            | Returns the remainder after division                                       |
| `ROUND(number, num_digits)`       | Rounds the number to the specified number of digits                        |
| `FLOOR(number, significance)`     | Rounds down towards zero to the nearest specified multiple                 |
| `CEILING(number, significance)`   | Rounds up, away from zero, to the nearest specified multiple               |
| `POWER(number, power)`            | Raises a number to a specified power                                       |
| `RAND()`                          | Generates a random number between 0 and 1                                  |
| `SIN(number)`                     | Sine of an angle in radians                                                |
| `SINH(number)`                    | Hyperbolic sine                                                            |
| `ASIN(number)`                    | Arc sine in radians                                                        |
| `COS(number)`                     | Cosine of an angle in radians                                              |
| `COSH(number)`                    | Hyperbolic cosine                                                          |
| `ACOS(number)`                    | Arc cosine in radians                                                      |
| `TAN(number)`                     | Tangent of an angle in radians                                             |
| `TANH(number)`                    | Hyperbolic tangent                                                         |
| `ATAN(number)`                    | Arc tangent                                                                |
| `ATAN2(x_number, y_number)`       | Arc tangent of y/x                                                         |
| `COT(number)`                     | Cotangent of an angle in radians                                           |
| `COTH(number)`                    | Hyperbolic cotangent                                                       |
| `SQRT(number)`                    | Square root                                                                |
| `LN(number)`                      | Natural logarithm (base `e`)                                               |
| `LOG10(number)`                   | Logarithm (base 10)                                                        |
| `EXP(number)`                     | Exponential function                                                       |
| `ABS(number)`                     | Absolute value                                                             |
| `FACT(number)`                    | Factorial                                                                  |
| `SEC(number)`                     | Secant of an angle in radians                                              |
| `CSC(number)`                     | Cosecant of an angle in radians                                            |
| `PI()`                            | Value of Pi                                                                |
| `RADIANS(degrees)`                | Converts degrees to radians                                                |
| `DEGREES(radians)`                | Converts radians to degrees                                                |
| `INT(number)`                     | Returns integer part of a number                                           |

## Supported Constants

| Constant | Description         |
| -------- | ------------------- |
| `e`      | Euler's number      |
| `PI`     | Pi                  |
| `TRUE`   | Boolean true        |
| `FALSE`  | Boolean false       |
| `NULL`   | Null value          |

You can also use all variables declared in the *Variables* section inside a recipe.

```
Variables: Tier=1
```

Now, the variable `Tier` can be referenced in expressions.

## Supported Text Functions

| Function                                           | Description                                                                                      |
| -------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `LEFT(text, num_chars)`                            | Extracts the specified number of characters from the left                                        |
| `RIGHT(text, num_chars)`                           | Extracts the specified number of characters from the right                                       |
| `MID(text, start_num, num_chars)`                  | Extracts a specified number of characters from a text string                                     |
| `REVERSE(text)`                                    | Reverses the string                                                                              |
| `ISNUMBER(text)`                                   | Checks if the value is a number                                                                  |
| `LOWER(text)`                                      | Converts text to lowercase                                                                      |
| `UPPER(text)`                                      | Converts text to uppercase                                                                      |
| `PROPER(text)`                                     | Capitalizes each word                                                                           |
| `TRIM(text)`                                       | Removes extra spaces                                                                            |
| `LEN(text)`                                        | Returns the length of the text                                                                  |
| `TEXT(value, [format_text])`                       | Converts a value to text with optional formatting                                               |
| `REPLACE(old_text, start_num, num_chars, new_text)`| Replaces part of text based on position and length                                              |
| `SUBSTITUTE(text, old_text, new_text)`             | Substitutes all occurrences of one string with another                                          |
| `FIND(find_text, within_text, [start_num])`        | Finds the location of one string within another (case-sensitive)                                |
| `SEARCH(find_text, within_text, [start_num])`      | Finds the location of one string within another (case-insensitive)                              |
| `CONCAT(text1, text2, …)`                          | Concatenates multiple text strings                                                              |
| `ISBLANK(text)`                                    | Returns TRUE if the text is null or empty                                                       |
| `REPT(text, repeat_times)`                         | Repeats the text a specified number of times                                                    |
| `CHAR(char_code)`                                  | Returns character from ASCII code                                                               |
| `CODE(char)`                                       | Returns ASCII code of a character                                                               |
| `VALUE(text)`                                      | Converts text to a number                                                                      |

## Text Examples

```
TEXT(123) -> 123

TEXT(DATEVALUE("2021-01-23"),"dd-MM-yyyy") -> 23-01-2021

TEXT(2.61,"hh:mm") -> 14:38

TEXT(2.61,"[hh]") -> 62

TEXT(2.61,"hh-mm-ss") -> 14-38-24

TEXT(DATEVALUE("2021-01-03")-DATEVALUE("2021-01-01"),"[h]") -> 48

TEXT(TIME(12,00,00)-TIME(10,30,10),"hh hours and mm minutes and ss seconds") -> "01 hours and 29 minutes and 50 seconds"
```