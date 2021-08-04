#!/bin/bash

wc --bytes *.cs

SLOC=`cat *.cs | grep -vE '(^\s*//|^\s*#|^\s*$|^\s*..\s$)' | wc --bytes`
echo "$SLOC total bytes of SLOC"

