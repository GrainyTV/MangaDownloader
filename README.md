# Manga Downloader
Blazingly fast and easy to use GUI manga downloader application for the [MangaDex](https://mangadex.org) website, using their provided [API](https://api.mangadex.org).

## Building from Source
The application is provided as a bulky single file dynamic executable. The available platforms are `Linux`, `Linux-musl`, `Windows` and `macOS`. The project contains a custom target to build all at once.
```
dotnet msbuild -target:PublishAll
```
This will compile all the executables and place them inside the `out/publish` folder.
Or you can also build only for a single platform which takes less time.
```
dotnet publish -c Release -r <RID>
```

## Context
Originally, I made this application to entertain myself during boring classes, but I figured it might be useful for others as well. The core idea is to create locally available PDF files that you can read with your preferred choice of application on pretty much any device without even needing an internet connection. This approach also has the added benefit of preserving the content in case it is removed from the website in a DMCA takedown.
