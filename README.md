# IceDog.Jint.Test

the jint lib test demo project

## IceDog.Jint.Shell

use jint implate a simple shell by video:
https://docs.microsoft.com/shows/code-conversations/sebastien-ros-on-jint-javascript-interpreter-net

this video tutorial is old , I use Jint 3.1.1 can't run ,so I write a new Version¡£

the run result:

```bash
Welcome to use Jint Shell,Jint Verssion:3.1.1,this is implement by video:
https://docs.microsoft.com/shows/code-conversations/sebastien-ros-on-jint-javascript-interpreter-net
input exit() to exit
[Jint Shell]>let hw=()=>"hello world";hw();
hello world
[Jint Shell]>
```

## IceDog.Jint.UseModule

I find Jint can Import js file,so I want to import the `lodash.js`,
base the Jint readme file module part,it hard to understand how to use,
so I write this demo test.

we can import lodash.js as global variable(_).

if we import a js file write by us,the variable,method .etc is not can use in global,
not like in browser , will auto mount on `window` obj,we need handle mount method,variable
to `globalThis`¡£

example:js/old/tool.js

```js
const goodNum = 42;

const greet = (name) => `hello,${name},nice to meet you`;

globalThis.goodNum = goodNum;

globalThis.greet = greet;
```

we can use `engine.Execute(code)` or `engine.Evaluate(code)` to execute js code.

the esm module is write simple,just write like run in browser

example:js/esm/tool.js

```js
const goodNum = 42;

const greet = (name) => `hello,${name},nice to meet you`;

export { goodNum, greet }

```

but the code not run by `engine.Execute(code)` or `engine.Evaluate(code)`,
we need run code like this:

```c#
// we can write code to main.js directly
var main2 = engine.Modules.Import("./esm/main.js");

// the module should run by Get Or  As* Method£¬use like engine.Execute will error
// the module main only execute once
var moduleMain2 = main2.AsObject();
double returnVal = main2.Get("returnVal").AsNumber();
```
we need to get the module instalnce,we get the module result ,the code in module
will execute.

the test run result:

```bash
Test_GlobalVariable---------------
[12:00:01][info] name:tom,age:20,likeNum:42
[12:00:01][info] hello,jerry,nice to meet you
[12:00:01][info] [1,4,9]
[12:00:01][info] lodash version:4.17.15
Test_ESMModule---------------
main1------------
[12:00:01][info] name:tom,age:20,likeNum:42
[12:00:01][info] hello,jerry,nice to meet you
[12:00:01][info] [1,4,9,16]
[12:00:01][info] lodash version:4.17.15
main2------------
[12:00:01][info] name:tom,age:20,likeNum:42
[12:00:01][info] hello,jerry,nice to meet you
[12:00:01][info] [1,4,9,16]
[12:00:01][info] lodash version:4.17.15
```