## 变量

### 定义

```ts
var a = 1;
var b: int;
```

### 修改

```ts
let a = 2;
let a + 1;
```

## 控制流

### If

```scala
if a do { b }
if a do { b } else { c }
if a do { b } else if c do { d }
if a do b;
if a do b else c;

if a else { b }
if a else b;
```

### Case

平铺 case

```haskell
case a;
of b do { c }
of d do e;
else { f }
```

case 块

```haskell
case a {
  of b do { c }
  of d do e;
  else { f }
}
```

### Do

do 块在如 if 等 的条件内使用时必须包在括号内

```scala
do { }
var a = do { }
```

### With

with 可以在同级作用域下尾随语句

```scala
do { } with do { }
```

### For

```scala
for do { }
for true do { }
for e of i do { }

for do { } with { } // for with 尾随的作用域是每次循环结束后
```

#### 使用 with 模拟三元 for

```scala
do { var a = 1 } with
for a < len do { 

} with { let a + 1 }
```

### Break Continue Return Goto

```js
break;
break a;
continue;
return;
return a;
```

带标签情况

```kotlin
do@l {
  break@l;
}
for@l true do { 
  continue@l;
}
fn@l some() {
  return@l;
}
do@l {
  goto@l;
}
```

#### 裸三元 for 实现

```scala
do@block { 
  var a = 1;
  do@cond {
    if a < len else break@block;
  }
  do@body {
    if some do goto@inc; // continue
    if some do break@block; // break
    do@inc {
      let a + 1;
    }
  }
  goto@cond;
}
```

##### C 语言版

```c#
  int a = 1;
cond:
  if (!(a < len)) goto end;
body:
  if (some) goto inc; // continue
  if (some) goto end; // break
inc:
  a++;
  goto cond;
end:;
```

### Try Throw Catch Finally

调用含有 throw 的函数必须使用 try

```scala
fn some() { throw a }
try some();
```

在 try 同级块内任何位置使用 catch

```scala
try some();
catch e : Foo do { }
catch e : Bar do { }
catch e do { }
catch { }
```

在任意块内使用 finally， finally 块将按倒序在主体块结束后顺序执行  

```c#
finally { }
```

#### 使用 with 模拟其他语言的 try catch finally

```scala
do { try some() } 
with catch e do { }
with catch { }
with finally { }
```

## 函数

### 定义

```rust
fn foo() {}
fn add(a: int, b: int) -> int { a + b }
```

### 调用

```js
foo();
add(1, 2);
```

### 类型

```ts
var f: fn (a: int, int) -> int;
```

### 表达式

```ts
// 函数表达式
var f = fn (a: int, b: int) -> int { a + b };
var f = fn (a, b) { a + b };
// 块函数表达式
var f = fn { (a: int, b: int) -> int => a + b };
var f = fn { (a, b) => a + b };

// 具名函数表达式
var f = fn fib (n: int, a: int = 0, b: int = 1) { if n > 0 do fib(n - 1, b, a + b) else a };
var f = fn { fib (n: int, a: int = 0, b: int = 1) => if n > 0 do fib(n - 1, b, a + b) else a };

// 单表达式函数
var f = fn (a, b) => a + b;
```

### 尾块函数

跟在表达式后面的块是尾块函数，要求表达式的类型是输入一个函数的函数

```js
var a = foo { };
```

## 基础类型

### 逻辑

```js
var b: bool = true;
var b = false;

def bool = true | false; // 伪代码，表示 true 和 false 可以作为字面量独立存在
```

### 数字

`half` 是 16 位浮点数  
`float` 是 32 位浮点数  
`num` 是 64 位浮点数  

`byte` 是 8 位有符号整数  
`narrow` 是 16 位有符号整数  
`short` 是 32 位有符号整数  
`int` 是 64 位有符号整数  

`ubyte` 是 8 位无符号整数  
`unarrow` 是 16 位无符号整数  
`ushort` 是 32 位无符号整数  
`uint` 是 64 位无符号整数  

```js
var i: int = 1;
var n: num = 1.5;
```

### 字符串

`'` 和 `"` 等价

```js
var s: str = 'asd';
var s = "asd";
var ts = 'asd  ${s}'
var c = c'a'; // 前缀 c 是字符
```

### 对象

```js
var o: { a: int, b: int } = {
  a = 1,
  b = 2,
};
var o: obj = o;
```

### 数组

```js
var a: [int] = [1, 2, 3];
var a: [int; 3] = [1, 2, 3]; // 定长数组
```

### 元祖

```js
var t: (int, bool, str) = (1, true, 'asd');
```

### 单元类型

单元类型，类型和值都是自己

```js
var u: () = ();
```

### 顶类型

顶类型，可以放入任何值

```js
var a: any;
```

### 底类型

底类型，没有值

```js
var n: !;
```

### 范围

```ts
var a: int..int = 1..5;
def Range[T] = T..T;
```

### 字面量类型

逻辑，数字，字符串，字符 都能取部分成员作为字面量类型  

```ts
var a: 1 = 1;
```

### 约束范围

约束范围和范围的区别是前面有 `in`  
只有范围和元组可以作为 `in` 的目标  

```ts
var a: in 1..10 = 5;
def int = in -9223372036854775808..9223372036854775807;
var c: in c'a'..c'z' = c'f';
var t: in (1, 2, 3, 4, 5) = 3; // 等于 1 | 2 | 3 | 4 | 5
```

### 或类型

实际是 和（sum）类型

```ts
var a: 1 | 2 = 1;
```

### 与类型

实际是 积（product）类型

```ts
var a: { a: 1 } & { b: 2 } = { a = 1, b = 2 };
```

### 可空类型

```js
var n: ?T = ();
var n: ?int = 1;
var n: ??int = 1; // 多层自动铺平

def ?[T] = T | (); // 伪代码
```

### 可选类型

```js
var o: T? = none;
var o: int? = some(1);
var o: int? = 1; // 隐式转换
var o: int?? = some(1); // 不存在多层隐式转换

def optional[T] enum {
  none, some(T)
}
```

## 定义

### 别名定义

```scala
def Foo = {
  a: int,
  b: num,
};

def Foo = Bar;
```

### 结构定义

```scala
def Foo data {
  a: int;
  
  fn Me(a: int) {
    let me.a = a;
  }

  fn add(b: int) -> int {
    a + b
  }

  fn val() -> int { a }
}
var a: Foo = Foo(1);
a.add(2); // 3

a.val; // 1, 没有参数的函数可以省略括号
```

### 接口定义

使用结构类型，无需显式标志实现接口

```scala
def Foo kind {
  a: int;
  fn add(b: int) -> int;
}

def Bar data(a: int) : Foo {
  a: int = a;
  fn add(b: int) -> int { a + b }
}
```

### 枚举定义

```scala
def Foo enum {
  A, B, C
}
def Bar enum {
  A(int),
  B { a: int },
  C data { a: int },
  D enum { A };
  
  fn some() {}
}
```

## 泛型

```scala
fn id[T](v: T) -> T { v }

def Functor[F: for[_]] kind {
  fn map[T, R](a: F[T], f: fn (T) -> R) -> F[R];
}

def Functor[T] kind : for[T] {
  fn map[R](f: fn (T) -> R) -> Me[R];
}

def Option[T] enum {
  Some(T),
  None,
}
```

## 模块

一个文件就是一个模块

```ocaml
module foo { 
  module bar { }
}
```

模块可以标注实现接口

在文件头使用不带名字的 module 来指示文件实现的接口，文件模块标注应该在所有其他东西之前

```ocaml
module : bar;
module foo: bar { }
```

### 导入

```js
import a.b.c; // 导入模块内所有内容
import a.b.c as foo; // 创建一个模块别名
import a.b.c of { Foo, bar as Bar }; // 从模块中导入部分内容
import _.a.b.c; // 开头为 _ 表示当前根目录
```

### 导出

默认模块内所有函数和定义都会导出，使用 private 来隐藏

使用 export 重新导出某个模块

```js
export a.b.c; // 重新导出模块内所有内容
export a.b.c as foo; // 将目标模块以 foo 为名字的子模块导出
export a.b.c of { Foo, bar as Bar }; // 导出模块内部分内容
export _.a.b.c; // 开头为 _ 表示当前根目录
```

使用 export 导出一个变量

```js
export var a = 1;
```
