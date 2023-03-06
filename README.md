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

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | True        | True            | 43.172413793103445 | 33.39655172413793  | 79.24137931034483  | 104.93103448275862 |
| medium-electric-pole | True        | True            | 43.172413793103445 | 25.82758620689655  | 79.24137931034483  | 104.93103448275862 |
| substation           | True        | True            | 43.172413793103445 | 7.896551724137931  | 79.24137931034483  | 104.93103448275862 |
| big-electric-pole    | True        | True            | 47.36206896551724  | 34.05172413793103  | 75.24137931034483  | 100.01724137931035 |
| small-electric-pole  | True        | False           | 41.51724137931034  | 13.568965517241379 | 5.948275862068965  | 11.017241379310345 |
| medium-electric-pole | True        | False           | 41.51724137931034  | 11.224137931034482 | 5.948275862068965  | 11.017241379310345 |
| substation           | True        | False           | 41.51724137931034  | 4.241379310344827  | 5.948275862068965  | 11.017241379310345 |
| big-electric-pole    | True        | False           | 41.793103448275865 | 10.827586206896552 | 5.9655172413793105 | 11.017241379310345 |
| small-electric-pole  | False       | N/A             | 83.93103448275862  | 13.068965517241379 | 0                  | 0                  |
| medium-electric-pole | False       | N/A             | 83.93103448275862  | 10.293103448275861 | 0                  | 0                  |
| substation           | False       | N/A             | 83.93103448275862  | 3.896551724137931  | 0                  | 0                  |
| big-electric-pole    | False       | N/A             | 84.13793103448276  | 9.03448275862069   | 0                  | 0                  |

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
| MediumElectricPole_NoBeacon_NoUnderground | 18.78 ms | 0.154 ms | 0.144 ms |
| SmallElectricPole_Beacon_Underground      | 34.20 ms | 0.097 ms | 0.091 ms |
| MediumElectricPole_Beacon_Underground     | 34.72 ms | 0.135 ms | 0.126 ms |
| BigElectricPole_Beacon_Underground        | 77.87 ms | 0.166 ms | 0.155 ms |
| Substation_Beacon_Underground             | 38.72 ms | 0.167 ms | 0.157 ms |
