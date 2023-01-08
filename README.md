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
