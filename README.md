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

| Electric pole        | Add beacons | Pipe count        | Pole count         | Beacon count      |
| -------------------- | ----------- | ----------------- | ------------------ | ----------------- |
| small-electric-pole  | True        | 43.51724137931034 | 33.03448275862069  | 79.72413793103448 |
| medium-electric-pole | True        | 43.51724137931034 | 25.67241379310345  | 79.72413793103448 |
| substation           | True        | 43.51724137931034 | 7.844827586206897  | 79.72413793103448 |
| big-electric-pole    | True        | 46.58620689655172 | 34.36206896551724  | 75.75862068965517 |
| small-electric-pole  | False       | 83.93103448275862 | 13.068965517241379 | 0                 |
| medium-electric-pole | False       | 83.93103448275862 | 10.293103448275861 | 0                 |
| substation           | False       | 83.93103448275862 | 3.896551724137931  | 0                 |
| big-electric-pole    | False       | 84.13793103448276 | 9.03448275862069   | 0                 |

### Runtime performance

``` ini
BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1265/22H2/2022Update/SunValley2)
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```

|                                    Method |     Mean |    Error |   StdDev |
|------------------------------------------ |---------:|---------:|---------:|
| MediumElectricPole_NoBeacon_NoUnderground | 17.92 ms | 0.142 ms | 0.132 ms |
|      SmallElectricPole_Beacon_Underground | 33.70 ms | 0.247 ms | 0.231 ms |
|     MediumElectricPole_Beacon_Underground | 33.50 ms | 0.151 ms | 0.141 ms |
|        BigElectricPole_Beacon_Underground | 78.69 ms | 0.391 ms | 0.346 ms |
|             Substation_Beacon_Underground | 37.96 ms | 0.295 ms | 0.276 ms |
