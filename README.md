# Multiplayer Not Included

> **Note:** This mod is a work in progress. Features may be incomplete or subject to change.

A multiplayer mod for [Oxygen Not Included](https://store.steampowered.com/app/457140/Oxygen_Not_Included/). This mod aims to add multiplayer functionality, allowing players to collaborate in the same world.

Steam workshop: Not released yet

## Developing

### Prerequisites

* .NET Framework 4.7.2
* An IDE that supports C# and .NET Framework, such as Visual Studio or JetBrains Rider.
* A local installation of the game "Oxygen Not Included".

### Setup

1. Clone this repository.

2. Open the `Multiplayer-Not-Included.sln` file in your IDE.

3. The project references game files from a specific path. You will need to edit the `Multiplayer-Not-Included/Multiplayer-Not-Included.csproj` file. Find the line `<SteamLibraryPath></SteamLibraryPath>` and change the path to match your Steam library location where "Oxygen Not Included" is installed. For example: `C:\Program Files (x86)\Steam`.

4. Build the solution in your IDE. This should restore any packages and compile the mod.

## Contributions

Contributions are welcome! Please feel free to open an issue or submit a pull request.
