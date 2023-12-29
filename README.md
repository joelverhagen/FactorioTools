# FactorioTools

Custom tools to augment the play of Factorio by Joel Verhagen. Currently, there is only one tool: an **oil field (outpost) planner**.

## Oil field planner


Given a blueprint containing pumpjacks, this tool return output a new blueprint conecting the pumpjacks with pipes and
electric poles. It also tried to find the best direction for the pumpjack to minimize extra pipes.

If you've used [FBE's](https://fbe.teoxoy.com/) or [Autotorio's](https://www.autotorio.com/oil) oil outpost generator or read [DeeFox's post](https://www.reddit.com/r/factorio/comments/6all0k/after_those_blueprintwizardryposts_i_decided_to/), it's a lot like that.

Why did I build my own? Well, I didn't know of these tools before I started and by the time I was committed, I still hadn't learned about the sunk cost fallacy. But actually I was interested if I could make the State-of-the-Art planner that generates the very best plans! Well, I haven't 😂 (at least not all the time). But I have built a tool that is very configurable and tries several different planning strategies and returns the best results.

### Check it out

The tool is available here: https://factorio-tools.vercel.app/oil-field. There is an "Add sample" button (which drops one of my test blueprints in) and "View in FBE" buttons to view the input and output blueprints. Give it a try and let me know what you think!

I've also attached a video of the thing in action.

https://user-images.githubusercontent.com/94054/224560733-35ca401f-ae51-46a9-951b-78ecc511227e.mp4

### Features

- Given a blueprint containing ≥ 1 pumpjack, it makes a blueprint with pipes, beacons, and poles.
- It prefers the blueprint with the _most beacon effects_ (highest oil field output), then _fewest beacons_ (minimizing power consumption), then _fewest pipes_ (maximizing pipe throughput).
- It supports non-vanilla beacon settings and can avoid beacon overload (for Space Exploration).
- It supports non-vanilla electric pole settings.
- It's meant to have a fast user experience, so it plans quickly but also makes it easy to paste in BPs.
- It's a web app so it's easy to Alt + Tab from Factorio to your web browser with the tool open.
- Your preferred settings are saved so if you come back to the tool later, it remembers.
- You can save a blueprint and all settings into a shareable URL.

### You might want to use this tool if

- You want an optimal (or near optimal) layout for your oil outposts.
- You have non-vanilla beacon or electric pole settings.

### Methodology

I captured 57 test blueprints from my own saves and used them for a scoring data set. If the beacon or pipe planning algorithms I tried produced better results on this data set, I considered it a better algorithm and moved forward with the idea. This iterative process allowed me to come up with several algorithm variants that are all used.

I re-implemented FBE's pipe planner and beacon planner (with some tweaks) and wrote my own algorithms for pipe placement, pipe straitening, beacon placement, and electric pole placement. I did not try FBE's electric pole algorithm since it's not that important to have the fewest electric poles.

My pipe planning algorithm is called "connected centers". It first tries to find pumpjacks that should be directly connected with lines of pipes ("trunks") and then connects these groups incrementally. My best performing "connected centers" variant uses Dr. Chris C. N. Chu's FLUTE algorithm, which is used for rectilinear Steiner minimal tree generation (RSMT) problem. RSMT is an NP-hard problem so getting a truly optimal solution isn't very feasible especially when considering multiple parameters like pipes, beacons, and poles.

My beacon planning algorithm is called "snug" and it tries to place beacons as close to each other as possible after preferring starting locations that cover the most pumpjacks.

The tool tries up to 16 different planning routines and returns the best one to the user. Even for larger oil fields (20+ pumps) it generally completes in less than 100ms. The tool shows the quality of the other plans so you can see how each algorithm did.

![image](https://user-images.githubusercontent.com/94054/224569867-936af0b4-28e1-4c44-9ad5-af33e89f8236.png)

### Ranking

For all of my test blueprints, I counted which series of algorithms (a "plan") yielded the best blueprint. If the plan was tied for first place, all ties share the win.

![image](https://user-images.githubusercontent.com/94054/224569786-4ce67ec9-ba83-461e-9d1e-f1343909c79c.png)

This shows that on my data set my "Connected Centers" pipe planner is the best the most often but FBE's beacon planner is dominant. The other algorithms are still valuable to keep in the app because they still yield the best plans sometimes (just not most often).

### Learning

This was a very interesting problem domain for me, and I learned a lot about front-end web development (Vue.js + Vite is great), .NET performance optimizations, algorithms (e.g. Delaunay triangulation, A* variants, Bresenham's line, etc), and hosting/deployment technologies. I also talked to the Factorio team over email about fair use of Factorio assets (which was ambiguous in their terms of service).

If you want to get your hands dirty with software development, I highly recommend finding a Factorio problem and writing a tool to help you.

### Credits

I want to thank [teoxoy](https://github.com/teoxoy), author of the awesome Factorio Blueprint Editor (FBE), for making their tool open source and talking to me on their Discord channel. I've tried to give credit to their algorithms that were re-implemented by me in both my tool and my source code. Their idea of using Delaunay triangulation for this space was AWESOME and really helped get my head in the right space.

### Screenshots

![Oil field input in Factorio](docs/img/oil-field-input-in-factorio.png)

![Oil field tool output](docs/img/oil-field-tool-ouptut.png)

![Oil field output in Factorio](docs/img/oil-field-output-in-factorio.png)

### Planner quality

| Electric pole        | Add beacons | Overlap beacons | Pipe count        | Pole count         | Beacon count      | Effect count       |
| -------------------- | ----------- | --------------- | ----------------- | ------------------ | ----------------- | ------------------ |
| small-electric-pole  | True        | True            | 46.39344262295082 | 34.14754098360656  | 80.81967213114754 | 110.14754098360656 |
| medium-electric-pole | True        | True            | 46.39344262295082 | 26.311475409836067 | 80.81967213114754 | 110.14754098360656 |
| substation           | True        | True            | 46.39344262295082 | 8.049180327868852  | 80.81967213114754 | 110.14754098360656 |
| big-electric-pole    | True        | True            | 49.75409836065574 | 35.26229508196721  | 77.04918032786885 | 106.24590163934427 |
| small-electric-pole  | True        | False           | 44.68852459016394 | 14.229508196721312 | 6.19672131147541  | 11.80327868852459  |
| medium-electric-pole | True        | False           | 44.68852459016394 | 11.60655737704918  | 6.19672131147541  | 11.80327868852459  |
| substation           | True        | False           | 44.68852459016394 | 4.377049180327869  | 6.19672131147541  | 11.80327868852459  |
| big-electric-pole    | True        | False           | 44.90163934426229 | 11.360655737704919 | 6.19672131147541  | 11.80327868852459  |
| small-electric-pole  | False       | N/A             | 87.65573770491804 | 13.557377049180328 | 0                 | 0                  |
| medium-electric-pole | False       | N/A             | 87.65573770491804 | 10.59016393442623  | 0                 | 0                  |
| substation           | False       | N/A             | 87.65573770491804 | 4.016393442622951  | 0                 | 0                  |
| big-electric-pole    | False       | N/A             | 87.88524590163935 | 9.639344262295081  | 0                 | 0                  |

#### FBE beacon planner

##### `Fbe`

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | yes         | yes             | 68.45564516129032  | 45.645161290322584 | 108.50537634408602 | 152.69892473118279 |
| medium-electric-pole | yes         | yes             | 68.45698924731182  | 34.6505376344086   | 108.51209677419355 | 152.71370967741936 |
| substation           | yes         | yes             | 68.46102150537635  | 10.751344086021506 | 108.51209677419355 | 152.71370967741936 |
| big-electric-pole    | yes         | yes             | 72.33106267029973  | 47.63896457765667  | 102.06539509536785 | 146.69073569482288 |
| small-electric-pole  | yes         | no              | 66.0241935483871   | 20.349462365591396 | 7.086021505376344  | 15.913978494623656 |
| medium-electric-pole | yes         | no              | 66.0241935483871   | 16.536290322580644 | 7.086021505376344  | 15.913978494623656 |
| substation           | yes         | no              | 66.0241935483871   | 6.423387096774194  | 7.086021505376344  | 15.913978494623656 |
| big-electric-pole    | yes         | no              | 66.01907356948229  | 15.129427792915532 | 7.106267029972752  | 15.8283378746594   |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                  | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                  | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                  | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                  | 0                  |

##### `FbeOriginal` - one, sort to match `Fbe`

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | yes         | yes             | 68.46908602150538  | 45.625             | 108.50268817204301 | 152.7002688172043  |
| medium-electric-pole | yes         | yes             | 68.47043010752688  | 34.67204301075269  | 108.50940860215054 | 152.71505376344086 |
| substation           | yes         | yes             | 68.47446236559139  | 10.762096774193548 | 108.50940860215054 | 152.71505376344086 |
| big-electric-pole    | yes         | yes             | 72.37465940054496  | 47.65122615803815  | 102.0858310626703  | 146.7057220708447  |
| small-electric-pole  | yes         | no              | 66.0241935483871   | 20.352150537634408 | 7.086021505376344  | 15.913978494623656 |
| medium-electric-pole | yes         | no              | 66.0241935483871   | 16.53494623655914  | 7.086021505376344  | 15.913978494623656 |
| substation           | yes         | no              | 66.0241935483871   | 6.419354838709677  | 7.086021505376344  | 15.913978494623656 |
| big-electric-pole    | yes         | no              | 66.01907356948229  | 15.128065395095367 | 7.106267029972752  | 15.8283378746594   |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                  | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                  | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                  | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                  | 0                  |

##### `FbeOriginal` - one, modified sort

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | yes         | yes             | 68.71236559139786  | 44.810483870967744 | 107.03360215053763 | 151.3508064516129  |
| medium-electric-pole | yes         | yes             | 68.64247311827957  | 34.206989247311824 | 107.06451612903226 | 151.42069892473117 |
| substation           | yes         | yes             | 68.64650537634408  | 10.826612903225806 | 107.06317204301075 | 151.41935483870967 |
| big-electric-pole    | yes         | yes             | 72.34059945504087  | 47.94822888283379  | 100.89100817438693 | 145.29972752043597 |
| small-electric-pole  | yes         | no              | 66.14112903225806  | 19.883064516129032 | 7.091397849462366  | 15.918010752688172 |
| medium-electric-pole | yes         | no              | 66.14112903225806  | 16.059139784946236 | 7.091397849462366  | 15.918010752688172 |
| substation           | yes         | no              | 66.14112903225806  | 6.096774193548387  | 7.091397849462366  | 15.918010752688172 |
| big-electric-pole    | yes         | no              | 66.1267029972752   | 15.217983651226158 | 7.114441416893733  | 15.831062670299728 |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                  | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                  | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                  | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                  | 0                  |

##### `FbeOriginal` - multiple, modified sort

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | yes         | yes             | 68.75268817204301  | 44.88172043010753  | 106.9489247311828  | 151.32930107526883 |
| medium-electric-pole | yes         | yes             | 68.72849462365592  | 34.251344086021504 | 106.95295698924731 | 151.3508064516129  |
| substation           | yes         | yes             | 68.72311827956989  | 10.858870967741936 | 106.95161290322581 | 151.3494623655914  |
| big-electric-pole    | yes         | yes             | 72.42643051771117  | 47.94550408719346  | 100.81062670299727 | 145.19073569482288 |
| small-electric-pole  | yes         | no              | 66.11290322580645  | 19.885752688172044 | 7.091397849462366  | 15.918010752688172 |
| medium-electric-pole | yes         | no              | 66.11290322580645  | 16.087365591397848 | 7.091397849462366  | 15.918010752688172 |
| substation           | yes         | no              | 66.11290322580645  | 6.134408602150538  | 7.091397849462366  | 15.918010752688172 |
| big-electric-pole    | yes         | no              | 66.1158038147139   | 15.207084468664851 | 7.114441416893733  | 15.831062670299728 |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                  | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                  | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                  | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                  | 0                  |

##### `FbeOriginal` - one, original sort

| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count      | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ----------------- | ------------------ |
| small-electric-pole  | yes         | yes             | 68.85349462365592  | 42.94758064516129  | 95                | 137.7486559139785  |
| medium-electric-pole | yes         | yes             | 68.85349462365592  | 32.12231182795699  | 95                | 137.7486559139785  |
| substation           | yes         | yes             | 68.86021505376344  | 10.297043010752688 | 95.0013440860215  | 137.75             |
| big-electric-pole    | yes         | yes             | 73.23809523809524  | 49.440816326530616 | 90.46666666666667 | 132.5469387755102  |
| small-electric-pole  | yes         | no              | 66.02016129032258  | 19.762096774193548 | 7.020161290322581 | 15.935483870967742 |
| medium-electric-pole | yes         | no              | 66.02016129032258  | 15.771505376344086 | 7.020161290322581 | 15.935483870967742 |
| substation           | yes         | no              | 66.02016129032258  | 6.012096774193548  | 7.020161290322581 | 15.935483870967742 |
| big-electric-pole    | yes         | no              | 66.02452316076294  | 14.866485013623977 | 7.03133514986376  | 15.8433242506812   |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                 | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                 | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                 | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                 | 0                  |

##### `FbeOriginal` - multiple, original sort
F
| Electric pole        | Add beacons | Overlap beacons | Pipe count         | Pole count         | Beacon count       | Effect count       |
| -------------------- | ----------- | --------------- | ------------------ | ------------------ | ------------------ | ------------------ |
| small-electric-pole  | yes         | yes             | 68.32661290322581  | 45.41129032258065  | 104.90456989247312 | 147.53091397849462 |
| medium-electric-pole | yes         | yes             | 68.32661290322581  | 34.446236559139784 | 104.90456989247312 | 147.53091397849462 |
| substation           | yes         | yes             | 68.32661290322581  | 10.364247311827956 | 104.90456989247312 | 147.53091397849462 |
| big-electric-pole    | yes         | yes             | 72.66757493188011  | 47.444141689373296 | 99.87602179836512  | 142.11852861035422 |
| small-electric-pole  | yes         | no              | 65.93951612903226  | 19.916666666666668 | 7.009408602150538  | 15.938172043010752 |
| medium-electric-pole | yes         | no              | 65.93951612903226  | 16.047043010752688 | 7.009408602150538  | 15.938172043010752 |
| substation           | yes         | no              | 65.93951612903226  | 6.178763440860215  | 7.009408602150538  | 15.938172043010752 |
| big-electric-pole    | yes         | no              | 66.00272479564033  | 14.935967302452315 | 7.013623978201635  | 15.8433242506812   |
| small-electric-pole  | no          | N/A             | 123.44354838709677 | 18.68548387096774  | 0                  | 0                  |
| medium-electric-pole | no          | N/A             | 123.44354838709677 | 14.528225806451612 | 0                  | 0                  |
| substation           | no          | N/A             | 123.44354838709677 | 5.643817204301075  | 0                  | 0                  |
| big-electric-pole    | no          | N/A             | 124.02993197278911 | 13.644897959183673 | 0                  | 0                  |


### Runtime performance

``` ini
BenchmarkDotNet v0.13.11, Windows 11 (10.0.22621.2861/22H2/2022Update/SunValley2)
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
```

| Method                                    |     Mean |    Error |   StdDev |
| ----------------------------------------- | -------: | -------: | -------: |
| MediumElectricPole_NoBeacon_NoUnderground | 16.12 ms | 0.051 ms | 0.048 ms |
| SmallElectricPole_Beacon_Underground      | 30.55 ms | 0.151 ms | 0.134 ms |
| MediumElectricPole_Beacon_Underground     | 31.05 ms | 0.081 ms | 0.067 ms |
| BigElectricPole_Beacon_Underground        | 69.71 ms | 0.291 ms | 0.273 ms |
| Substation_Beacon_Underground             | 34.98 ms | 0.215 ms | 0.191 ms |
