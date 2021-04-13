# CoS
全称 `Configuration Script`  
是嵌入式脚本语言  
目标是可以嵌入多个不同的宿主语言  
同时具有强大约束能力的类型系统  

## 变量

### 定义

```ts
var a = 1;
var b: int;
```

### 修改

```ts
var mut a; // 声明可变
mut a; // 或者直接 mut
let a = 2;
let a + 1;
```

## 控制流

### If

单表达式情况可以使用 `do` 标注  

```scala
if a { b }
if a { b } else { c }
if a { b } else if c { d }
if a do b;
if a do b else c;

if a else { b }
if a else b;

if a else do { b = 1 }; // else 也可以使用 do 标注为表达式
```

### Case

平铺 case

```haskell
case a;
of b { c }
of d do e;
else { f }
```

case 块

```haskell
case a {
  of b { c }
  of d do e;
  else { f }
}
```

### 块

```scala
@{ }
var a = @{ }
```

### With

with 可以在同级作用域下尾随语句或尾随块  

```scala
@{ } with { }
```

### 循环

```scala
while true { } // 条件循环
for i in e { } // 迭代器循环

while true { } with { } // for with 尾随的作用域是每次循环结束后

while do true { } // 等于 c 系的 do while
```

#### 使用 with 模拟三元 for

```scala
@{ mut a = 1 } with
while a < len { 

} with { let a + 1 }
```

### Break Continue Return Goto

```js
break;
break a;
continue;
return;
return a;
=> a; // 等价于 return，后面必须跟表达式
```

带标签情况

```kotlin
l@{
  break@l;
}
l@while true { 
  continue@l;
}
l@fn some() {
  return@l;
}
l@{
  goto@l;
}
```

裸标签  

```kotlin
l@;
goto@l;
```

#### 裸三元 for 实现

```kotlin
block@{ 
  mut a = 1;
  cond@{
    if a < len else break@block;
  }
  body@{
    goto@inc; // continue
    break@block; // break
    inc@{
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
  goto inc; // continue
  goto end; // break
inc:
  a++;
  goto cond;
end:;
```

### Try Throw Catch Finally

含有 `throw` 的函数必须使用 `throws` 标注  
调用含有 `throw` 的函数必须使用 `try`  

```scala
fn some() throws { throw a }
try some();
```

在 try 同级块内任何位置使用 catch

```scala
try some();
catch e : Foo { }
catch e : Bar { }
catch e { }
catch { }
```

在任意块内使用 finally， finally 块将按倒序在主体块结束后顺序执行  

```c#
finally { }
```

#### 使用 with 模拟其他语言的 try catch finally

```scala
@{ try some() } 
with catch e { }
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
var f = fn { (a: int, b: int) -> int do a + b };
var f = fn { (a, b) do a + b };

// 具名函数表达式
var f = fn fib (n: int, a: int = 0, b: int = 1) { if n > 0 do fib(n - 1, b, a + b) else a };
var f = fn { fib (n: int, a: int = 0, b: int = 1) do if n > 0 do fib(n - 1, b, a + b) else a };

// 单表达式函数
var f = fn (a, b) do a + b;
```

### 尾块函数

跟在表达式后面的块是尾块函数，要求表达式的类型是输入一个函数的函数  

```js
var a = foo { };
```

在诸如 `if` 的条件表达式等地方，要使用尾块函数必须包在 `()` 内  
或者使用显式尾块语法  

```js
var a = foo.{ };
```

### 函数标注

函数标注在大括号或者 do 前面，没有顺序要求

```rust
fn foo() inline {} // 内联语义
fn bar() co {} // 延续语义
fn baz() inline co {}
```

## 基础类型

### 逻辑

```js
var b: bool = true;
var b = false;
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

字符串具体编码同宿主字符串编码  
`char` 是 32 位无符号整数  
`rawchar` 是宿主字符类型  

### 对象

对象字面量不能直接作为返回值，需要包在 () 内 或者使用 return  

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
  mut a: int;
  
  // Me 既是一个类型指向自己，也可以用于定义构造函数
  fn Me(a: int) { 
    let me.a = a;
  }

  fn Me.foo() { // 具名构造函数
    let me.a = 1;
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
  var a: int;
  fn add(b: int) -> int;
}

def Bar data(a: int) : Foo {
  var a: int = a;
  fn add(b: int) -> int { a + b }
}
```

#### 关联类型

要求实现 Foo 的目标具有名为 Bar 的子成员  

```scala
def Foo kind {
  def Bar need;
}
```

### 枚举定义

```scala
def Foo enum {
  A, B, C
}
def Bar enum {
  A(int),
  B { a: int };
  
  fn some() {}
}
```

```scala
def bool enum {
  true, false
}
type a = true; // 枚举的成员可以作为字面量类型独立存在  
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

在文件头使用不带名字的 module 来指示文件实现的接口，文件模块标注应该在所有 import 之前，在所有 export 和 非文件头之前  

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

### 文件头

文件头可以包含 import export module 

