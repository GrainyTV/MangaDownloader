# Building SkiaSharp

1. Clone and checkout the skia repository from the `version.toml` file.
2. Use the provided Python script to sync all dependencies.
```
python tools/git-sync-deps
```
3. For some reason there is a missing header in one of the source files. It needs to be patched or the compilation will fail midway through.
```
patch -p1 < ../0001.patch
````
4. Generate build files for the desired platform. You need to specify the OS, the architecture and the build directory. All other flags are specified in the `config.toml` file. The following examples will assume you are building for Linux-x64.
```
./bin/gn gen 'out/linux-x64' --args="target_os=\"linux\" target_cpu=\"x64\" $(cat ../config.toml)"
```
5. Now you can build the target libraries using Ninja. You need to specify the two targets and their previously selected build directory.
```
ninja 'SkiaSharp' -C 'out/linux-x64'
ninja 'HarfBuzzSharp' -C 'out/linux-x64'
``` 
