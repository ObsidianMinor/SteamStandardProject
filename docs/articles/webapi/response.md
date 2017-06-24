# General response
A response JSON object consists of an object with one object called "response" with two properties:
 * "result"
 * "message"
result being a Result enum value (found in Steam.Common) and message being a string message.

All responses contain these properties, and if they don't the values default to 1 (Result.OK) and an empty string.
