#!/bin/bash
dotnet publish -c Release -r osx-x64 --self-contained true --output "../out/osx"

dotnet publish -c Release -r win-x64 --self-contained true --output "../out/win"