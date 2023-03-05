# FactorioTools

Custom tools to augment the play of Factorio by Joel Verhagen.

## Pumpjack Pipe Optimizer

Given a blueprint containing pumpjacks, this tool return output a new blueprint conecting the pumpjacks with pipes and
electric poles. It also tried to find the best direction for the pumpjack to minimize extra pipes.

**Credits:** the pipe routing algorithm is by [teoxoy](https://github.com/teoxoy) and was innovated in their
[Factorio Blueprint Editor](https://github.com/teoxoy/factorio-blueprint-editor) project. It performed way better than
my algorithm based on Dijkstra's.

![Oil field input in Factorio](docs/img/oil-field-input-in-factorio.png)

![Oil field tool output](docs/img/oil-field-tool-ouptut.png)

![Oil field output in Factorio](docs/img/oil-field-output-in-factorio.png)

### Planner quality

| Electric pole        | Add beacons | Overlap beacons | Pipe count        | Pole count         | Beacon count      |
| -------------------- | ----------- | --------------- | ----------------- | ------------------ | ----------------- |
| small-electric-pole  | True        | True            | 43.46551724137931 | 33.206896551724135 | 79.74137931034483 |
| medium-electric-pole | True        | True            | 43.46551724137931 | 25.93103448275862  | 79.74137931034483 |
| substation           | True        | True            | 43.46551724137931 | 7.896551724137931  | 79.74137931034483 |
| big-electric-pole    | True        | True            | 46.51724137931034 | 34.258620689655174 | 75.63793103448276 |
| small-electric-pole  | True        | False           | 42.1551724137931  | 14.189655172413794 | 6.396551724137931 |
| medium-electric-pole | True        | False           | 42.1551724137931  | 11.60344827586207  | 6.396551724137931 |
| substation           | True        | False           | 42.1551724137931  | 4.431034482758621  | 6.396551724137931 |
| big-electric-pole    | True        | False           | 42.36206896551724 | 11.03448275862069  | 6.413793103448276 |
| small-electric-pole  | False       | N/A             | 83.93103448275862 | 13.068965517241379 | 0                 |
| medium-electric-pole | False       | N/A             | 83.93103448275862 | 10.293103448275861 | 0                 |
| substation           | False       | N/A             | 83.93103448275862 | 3.896551724137931  | 0                 |
| big-electric-pole    | False       | N/A             | 84.13793103448276 | 9.03448275862069   | 0                 |

### Runtime performance

``` ini
BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1265/22H2/2022Update/SunValley2)
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=7.0.201
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
```

| Method                                    |     Mean |    Error |   StdDev |
| ----------------------------------------- | -------: | -------: | -------: |
| MediumElectricPole_NoBeacon_NoUnderground | 18.39 ms | 0.100 ms | 0.089 ms |
| SmallElectricPole_Beacon_Underground      | 34.27 ms | 0.296 ms | 0.277 ms |
| MediumElectricPole_Beacon_Underground     | 34.57 ms | 0.162 ms | 0.151 ms |
| BigElectricPole_Beacon_Underground        | 79.04 ms | 0.296 ms | 0.277 ms |
| Substation_Beacon_Underground             | 39.13 ms | 0.201 ms | 0.178 ms |
