# Sundry.Option

<img alt="GitHub" src="https://img.shields.io/github/license/sundryoss/Sundry.Option?style=for-the-badge">

<img alt="GitHub release (latest SemVer including pre-releases)" src="https://img.shields.io/github/v/release/sundryoss/Sundry.Option?include_prereleases&style=for-the-badge">

<img alt="GitHub release (latest SemVer including pre-releases)" src="https://img.shields.io/github/v/release/sundryoss/Sundry.Option?style=for-the-badge">


## What is `Option<T>` type?
`Option<T>` type is an implementation of `Maybe` monad. It has two possible states - default one representing absence of value (`None`) and the other one representing presence of value (`Some`). 

## When to use `Option<T>` type?

> The option type is used when an actual value might not exist for a named value or variable. An option has an underlying type and can hold a value of that type, or it might not have a value.[^MicrosoftDoc]

## What's wrong with `null`?
In C#, `null` has no type, but most variables can be null. so you can't really trust the type system.

A Option, on the other hand, always has a type, which means that Option is a better approach to the question of values that may or may not be present. In other words, Make nulls explicit with the Option type.

## What is the difference between `Nullable<T>` and `Option<T>`?

`Option<T>` wraps value that might not exist, in similar way as Nullable<T> wraps value types. While Nullable<T> only wraps structs (value types), Option<T> works with all kinds of types, making things more consistent. The value representing absence of data in Nullable<T> is the same as for reference types - both use `null` for that purpose. 
With `Option<T>` you don't really care what is the value representing absence of data, because it's the property of the type itself.

## What is the difference between `Nullable<T>` and `Option<T>`?


[^MicrosoftDoc]:
    [Microsoft Doc](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/options)

