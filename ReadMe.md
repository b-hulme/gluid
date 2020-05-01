Gluid
=====

A simple C# class that encodes a regular integer (32-bit) or long (64-bit) value into a Guid (UUID).
The integer can be easily recovered from the Guid at any point in the future.
In order to ensure two identical integers encode to a unique Guid, a namespace string is accepted when the Guid is generated. This allows you to group your integers by a name to prevent clashes.
It's not necessary to use a namespace string however if you don't, the generated Guid will only be as unique as the integer or long that you pass to it.

The Gluid library ensures that the generated Guid conforms to RFC.4122 and ensures that for a given namespace and integer combination, the generated Guid will be unique.
It does this by using UUID Variant 0xe and Version 0xf.
These are both currently reserved values in the RFC specification so for the moment at least (and probably for a very extended period of time) the values won't clash with any system generated one's.

## Use case
I wrote the Gluid class rather quickly to deal with a simple Id mapping problem between an old application (using int's) and a new application using Guids.
While we moved clients from the old application, one function at a time it was necessary to have a Guid that could intrinsicly map back to the original Id (and vice-versa), but not clash with and newly generated regular Guids.
This could have been done using look up tables however the performance then takes a huge hit.
Gluid's are very fast. Based on the initial testing most round-trip conversion occur in <1ms.

# Installing
Download the class file and put it somewhere appropriate in your project.

# Usage

## Create a Gluid Guid
The Gluid has a helper method to generate Guid rapidly:

```
var guidA = Gluid.NewGuid(42, "My old program");
```

Will generate a new Guid with an internal integer of 42 against the "My old program" namespace.
If you now call

```
var guidB = Gluid.NewGuid(42, "My new program");
```

Guid A and B will be different as they are created against two different namespaces, however the internal integer will be 42 in both cases.

If you used the same namespace for GuidB as you did for GuidA then the two Guids will be the same, potentially causing an Id collision.
This is by design as the primary use case for a Gluid is to map an old integer based Id value to a Guid in a new system.

## Get the internal integer (or long) of a Gluid Guid
To extract the internal value of a Gluid Guid, you do the following:

```
var guid = Gluid.NewGuid("My namespace", 100);

var value = guid.ToInt32("My namespace");
// value should be 100
```

Or if you need to get a long value:

```
var guid = Gluid.NewGuid("My namespace", 10000000000);

var value = guid.ToInt64("My namespace");
// value should be 10000000000
```

In both of these cases, if the namespace you provide doesn't match the one that the Gluid was created with or the Guid is just s regular Guid, the methods will return a `Null`.

## Checking the Guid is a Gluid

```
var guid = Gluid.NewGuid("Some namespace", 1);

var isGluid = guid.IsGluid();
```

Or if you want to check the Guid is a Gluid for a specific namespace:

```
var guid = Gluid.NewGuid("Some namespace", 1);

var isAGluid = guid.IsLinked("Some namespace");
```
