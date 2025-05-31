# Manga Downloader

Blazingly fast and easy-to-use manga downloader application using the [MangaDex](https://api.mangadex.org) API with a graphical user interface.

## Context and Motivation

Originally I made this application to entertain myself during boring classes, but I figured it might be useful for others as well, so I decided to make a polished version that even non-tech-savvy people can use. Currently, the biggest online platform I know for reading fan translations is MangaDex, but it has its issues. For one, reading manga straight from a website is not the best experience, especially on limited screen space devices like mobile phones. Plus, the site operates in a bit of a legal gray area regarding copyright. The core idea is to create locally available PDF files that you can read with your preferred choice of application on pretty much any device without even needing an internet connection. You can also future-preserve the content as an added bonus in case a DMCA takedown happens.

> This is the 4th (and possibly final) iteration of the application. It was originally a Java console application. Later it was ported to C#. Then I made a GUI in Flutter. Now both the backend and the frontend uses F#.

## Building from Source

The application uses AOT compilation so you need to install a few prerequisites, namely: `clang` and `zlib-devel`. Then if you have `.NET8` on your system, you can run the following command:
```
dotnet publish -c Release
```  
This will compile the native executable for your platform and place it inside a folder called `MangaDownloader` in the root of the cloned the repository.
