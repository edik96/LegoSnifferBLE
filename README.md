# LegoSnifferBLE
Simple C# Solution to sniff incoming BLE packets from Robot Inventor 51515

> The repository is no longer maintained and isn't properly written. Feel free to extract code-snippets for the source.

## Requirements
- Dot NET Framework 4.7.2
- Windows Runtime API
- InTheHand NuGet
- Paired BLE connection to the 51515 HUB

## Installation
1. Pull the solution
2. Adjust the DB connecton string in Program.cs  (if needed)
3. Adjust the MAC address to your paired 51515 Hub in Program.cs (if needed)
4. Add Windows reference to LegSnifferBLE.proj
  - References, Add Reference, Browse, "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.22621.0(version can vary)", All Files (.*), Windows.winmd, Add
5. Profit?

## Information
### LegoSnifferBLE Project Args[3]
{mac_address:string ex.:FF:FF:FF:FF:FF:FF} {use_namedpipes:bool ex.:1} {use_db:bool ex.:1} 
