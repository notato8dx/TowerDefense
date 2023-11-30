# TowerDefense
TowerDefense is a tower defense game inspired by Plants vs. Zombies.

## Dependencies
- [MGLib](https://github.com/notato8dx/MGLib)

## Execution
```
dotnet run
```

## Building

### Windows
```
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
```

### Mac
```
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
```

### Linux
```
dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false --self-contained
```
