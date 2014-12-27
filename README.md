Umbraco JWT AuthTokens
======================

This is a repository for providing a secure based API to perform backoffice actions using JWT Auth tokens

## What are JWTs?
They are an auth token that allows you to send a piece of JSON encoded as a token and are the more modern approach to deal with auth in applications 
especially as we build applications across different devices. The videos below will do a lot better trying to explain it than I can do.

## Why do this?
I needed to create this POC for an upcoming pet project I am currently hacking & building. I needed a way to authenticate to any Umbraco backoffice using 
the same credentials as the Umbraco backoffice user & ensure they have access to specific section/s on that user. The user will only need to login once and we 
then store the auth token in local storage or cookies and use that token from storage or cookie for any future secured/protected API calls.

## More Resources on JWTs
**NG Europe talk from 0Auth.com guys**<br/>
https://www.youtube.com/watch?v=lDb_GANDR8U&list=UUEGUP3TJJfMsEM_1y8iviSQ

**Another good talk on JWT**<br/>
https://www.youtube.com/watch?v=vIGZxeQUUFU#t=83

**Debugger tool**<br/>
http://jwt.io

**Single Class File Library & Nuget Package I use for JWT dedcoding<br/>
Authored by FireBase, Twilio & others**<br/>
http://www.nuget.org/packages/JWT/<br/>
https://github.com/johnsheehan/jwt/blob/master/JWT/JWT.cs

## Explaining how my implementation works
1. A user will do a HTTP post of their backoffice Umbraco username & password to a normal API Controller
  1. Controller verifies credentials
  2. If a token already exists for the user (matches against user id) in custom PetaPoco DB table
  3. If no token exists create a new token for the user & store in the DB table
  4. Return existing or newly created token in the response

2. A user can then store that token say in LocalStorage or Cookie

3. User calls secured API sends bearer auth token in HTTP header for request (From LocalStorage or cookie)
  1. Server finds token in request
  2. Tries to decode the token with secret
  3. If token not encoded with same secret Send 401
  4. If token can be decoded correctly find user id in JSON
  5. Check if user has the same token stored in the DB
  6. If so process method on API controller

4. User changes password in Umbraco backoffice
  1. If no token exists in DB - nothing happens
  2. If token exists in DB - generate new token with new datetime stamp to make it unique from last time

## Why do you not just store the username & password in the JWT?
The payload of the JSON object in the JWT can be decoded easily, paste in a token into jwt.io and you can see it easily. 
However the part to do with JWT is that we verify that the AuthToken is using the signed secret/string to ensure our server created the JWT & it's validity.

So in my implementation I only store the username, user ID, user role and Created Date of the token. The date ensures that the token is different every time 
a new one is generated for easy revoking.

## Do the tokens expire?
My implementation allows the tokens to work indefinitely until the user in the Umbraco backoffice changes their password.
Which creates a new token and thus revoking access to the API for any clients or services using it.

However it is easily possible to store an expiry date in the JSON payload of the Auth Token and when decoding it verifying the expiry date on it.
