This directory contains source (either original or transpiled) of the following open source projects.

The transpiled source is transpiled using the `Invoke-LuaBuild.ps1` script in this directory. Internally it uses my fork
of CSharp.lua to compile 

The `FactorioTools` and `FluteSharp` directories are transpiled Lua sources generated from C# in the
`../src/FactorioTools` directory and my [FluteSharp](https://github.com/joelverhagen/FluteSharp) project respectively. 

## CSharp.lua

- Original repository: https://github.com/yanghuan/CSharp.lua
- Author: [yanghuan](https://github.com/yanghuan) (YANG Huan)
- My fork: https://github.com/joelverhagen/CSharp.lua
- Changes in my fork:
  - [diff](https://github.com/yanghuan/CSharp.lua/compare/master...joelverhagen:CSharp.lua:lua-compat)

### Original license
```
Copyright 2017 YANG Huan (sy.yanghuan@gmail.com).

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

## DelaunatorSharp

- Original repository: https://github.com/nol1fe/delaunator-sharp
- Author: [nol1fe](https://github.com/nol1fe) (Patryk Grech)
- My fork: https://github.com/joelverhagen/delaunator-sharp
- Changes in my fork:
  - [diff](https://github.com/nol1fe/delaunator-sharp/compare/master...joelverhagen:delaunator-sharp:lua-compat)
  - Refactor decrement and assign to separate statements. Workaround for [yanghuan/CSharp.lua#473](https://github.com/yanghuan/CSharp.lua/issues/473).
  - Refactor LINQ usage to simple array implementation. Workaround for Factorio not supporting Lua coroutines.

### Original license
```
MIT License

Copyright (c) 2019 Patryk Grech

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```