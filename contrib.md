# Programmers manual

This document provides only a high level overview of the library.
For details, see reference of individual classes.

The library features can be split into following categories:

1. [Terminal buffering](#terminal-buffering)
2. [Color representation](#color-representation)
3. [Drawing shapes](#drawing-shapes)
4. [Escape codes](#escape-codes)

## Terminal buffering

The `TermColor.Terminal` class is the main API for interacting with this library.
It internally uses `TermColor.AnsiTermBuffer` and `TermColor.Win32TermBuffer` for
forwarding the colored text to the OS.

For a workflow example, checkout the [readme](index.html).

## Color representation

This library provides several structures to represent color, and algorithms to convert between them:
- `TermColor.ColorRGB`
- `TermColor.ColorHSV`
- `TermColor.Color4`
- `TermColor.Color8`
- `TermColor.Color24`

## Drawing shapes

Number of different shapes can be drawn on a `TermColor.ITerminalBuffer` via the `TermColor.Drawing.ITerminalBufferEx` class' extension methods.

## Escape codes

There is also a set of internal utilities useful for generating ANSI escape sequences:

- `TermColor.AnsiEscapeCodes.SGR`
- `TermColor.AnsiEscapeCodes.CSI`