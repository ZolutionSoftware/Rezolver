# Rezolver Tests Projects

This folder contains tests for the Rezolver framework:

## Rezolver.Tests

Main tests project - covers the Targets, Container and TargetContainer APIs.

## Rezolver.Tests.Compilation.Specification

A suite of specification tests for an implementation of `Rezolver.Compilation.ITargetCompiler`

## Rezolver.Tests.Compilation.Expressions

Applies the compilation specification tests to the built-in `Rezolver.Compilation.Expressions.ExpressionCompiler`

## Rezolver.Microsoft.DependencyInjection.Tests

Applies the specification tests from the Microsoft.Extensions.DependencyInjection.Specification.Tests package to the Rezolver implementation
of Microsoft's .Net Core DI Container

## Rezolver.Tests.Examples

Complete scenario tests which are also used as code examples for [the developer documentation at rezolver.co.uk](http://rezolver.co.uk/developers/)

## Rezolver.Tests.Configuration.Json

Tests for the Json configuration implementation

*This is unstable, hence why the nuget packages have been delisted for the moment*

## Rezolver.Tests.Shared

A shared project for many of the other tests projects - defines types etc that are common to all tests.