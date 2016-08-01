# Delayware

I created a resiliency tool that helps applications tolerate random instance failures within Azure.

## Why?

An Azure Website has limited tools for testing resiliency, so I created a tool to allow any website to be taken offline with the click of a button. Simply put, this is my attempt at creating the [SimianArmy](https://github.com/Netflix/SimianArmy) for Azure.

## But what is it?
Delayware is a middleware that introduces a random delay into any request or take down a server for a limited time. It listens for a _X-POISON_ header in an request. This header will contain a JWT encoded message to either delay or take down a service.

## How do I use it?

### Middleware Setup

```csharp
public static class WebApiConfig {

  public static void Register(HttpConfiguration config) {

    string secret = "SHHH_SECRET";
    config.MessageHandlers.Add(new PoisonHandler(new DefaultPoisonStrategy(secret)));

  }
}
```

### Create JWT payload

> Create JWT @ https://jwt.io/

#### Poison pill payload

Example to delay the request into service randomly from 500ms to 1000 ms
```js
{
  "from" : 500,
  "to" : 1000,
  "action" : "single",
  "type" : "delay"
}
```

Example to delay any requests into service randomly from 500ms to 1000 ms for the next 30 secs
```js
{
  "from" : 500,
  "to" : 1000,
  "duration": 30,
  "action" : "timed",
  "type" : "delay"
}
```

Example to return 500 status code in the request
```js
{
  "code" : 500,
  "action" : "single",
  "type" : "status"
}
```

Example to return 500 for any requests into service randomly for the next 30 secs
```js
{
  "code" : 500,
  "duration": 30,
  "action" : "timed",
  "type" : "status"
}
```

Encoded JWT with secret _SHHH_SECRET_ `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmcm9tIjo1MDAsInRvIjoxMDAwLCJhY3Rpb24iOiJzaW5nbGUiLCJ0eXBlIjoiZGVsYXkifQ.qklMQ8H7kJhFfNxpAGTACoBs_7XdMytgg5DPYNpXEPE`

### Send request to service

`curl --header "X-POISON: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJmcm9tIjo1MDAsInRvIjoxMDAwLCJhY3Rpb24iOiJzaW5nbGUiLCJ0eXBlIjoiZGVsYXkifQ.qklMQ8H7kJhFfNxpAGTACoBs_7XdMytgg5DPYNpXEPE" www.myservice.com`

## Demo @ https://youtu.be/d82SSmGBsgo
