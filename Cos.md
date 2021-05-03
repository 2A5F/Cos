# CoS
全称 `Configuration Script`  
是嵌入式脚本语言  
目标是可以嵌入多个不同的宿主语言  
同时具有强大约束能力的类型系统，和极致简单的语法  

### TODO

- 详细规划模式
- 分号插补设计

## 变量

### 定义

`let` 是不可变绑定  

```ts
let a = 1;
let b: int;
```

- 详细语法  
  `let <定义模式> (:<类型>)? (= <表达式>)?`

### 修改

`let mut` 或 `mut` 是可变绑定  

使用 `alt` 语句来修改变量  
`alt` 意思是 `alter`  

```rust
let mut a; // 声明可变
mut a; // 或者直接 mut
alt a = 2;
alt a + 1; // 等于 alt a = a + 1
```

- 使用修改语句的好处
  - 语义明确，不会和表达式混起来
    - 方便搜索定位，`=` 在搜索中和定义区分很困难
  - 没有返回值，防止条件中使用 `=` 导致的低级错误
  - 自我修改语法糖
    ```ts
    alt a + 1; // 类似 a++;
    ```
  - 没有返回值，避免 `++a` `a++` 的困扰
  - 连续自我修改语法糖  
    `=` 是不能连续的  
    ```ts
    alt a + 1 * 2 / 3
    ```
    等于
    ```ts
    alt a = a + 1
    alt a = a * 2
    alt a = a / 3
    ```

- 详细语法  
  `alt <操作修改模式> = <表达式>`  
  `alt <操作修改模式> (<修改操作符> <表达式>)+`  
  `alt <操作修改模式> <一元修改操作符>`  
  `alt <修改模式>`

## 控制流

### If

```scala
if (a) b
if (a) b else c
if (a) b else if (c) d
if (a) { b }
if (a) { b } else { c }
if (a) { b } else if (c) { d }
// 如果条件无括号，则必须后面必须是块而不是表达式
if a { b }
if a { b } else { c }
if a { b } else if c { d }


if (a) else b;
if (a) else { b };
if a else b;
if a else { b }
```

### Case

平铺 case

```js
case a;
of b { c }
of (d) e; // case 的 条件括号也和 if 一样
else { f }
```

case 块

```haskell
case a {
  of b { c }
  of (d) e;
  else { f }
}
```

### 块

```js
do { }
let a = do { }
```

### With

with 可以在同级作用域下尾随语句或尾随块  

```js
do { } with { }
```

### 循环

```js
while true { } // 条件循环
while (true) body; // 再括号中的条件循环
for i in e { } // 迭代器循环
for (i in e) body; // 再括号中的迭代器循环
for (mut i = 0; i < len; alt i++) { } // 三元 for 循环，必须使用括号

while true { } with { } // for with 尾随的作用域是每次循环结束后

do { } while true;
do { } while true with { }
```

#### 使用 with 模拟三元 for

```js
do { mut i = 1 } with
while i < len { 

} with { alt i++ }
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
while@l true { 
  continue@l;
}
fun@l some() {
  return@l;
}
do@l {
  goto@l;
}
```

裸标签  

```kotlin
@l;
goto@l;
```

### Try Throw Catch Defer

含有 `throw` 的函数必须使用 `throws` 标注  
调用含有 `throw` 的函数必须使用 `try`  

```kotlin
fun some() throws { throw a }
try some();
```

在 `try` 同级块内任何位置使用 `catch`

```kotlin
try some();
catch e : Foo { }
catch (e : Bar) body;
catch e { }
catch { }
```

在任意块内使用 `defer`， `defer` 块将按倒序在主体块结束后顺序执行  
`defer` 类似别的语言的 `finally`  

```swift
defer { }
```

```swift
defer { print(1) }
defer { print(2) }
defer { print(3) }
// 输出 3、2、1
```

#### 正常写法

```swift
fun foo() {
  try some();
  catch e { }
  catch { }
  defer { }
}
```

#### 使用 `with` 模拟其他语言的 `try catch finally`

```swift
do { try some() } 
with catch e { }
with catch { }
with defer { }
```

## 函数

### 定义

```kotlin
fun foo() {}
fun add(a: int, b: int) -> int { a + b }
```

### 调用

```js
foo();
add(1, 2);
```

### 类型

```ts
let f: (a: int, int) -> int;
let f: int -> int;
let f: (a: int, b: int) -> (c: int) -> int;
let f: int -> int -> int;
// 加 fun 关键字时必须使用括号
let f: fun (a: int, int) -> int;
```

### 表达式

```ts
// 函数表达式
let f = fun (a: int, b: int) -> int { a + b };
let f = fun (a, b) { a + b };

let f = fun (a: int, b: int) -> int => a + b;
let f = fun (a, b) => a + b;

// 块函数表达式
let f = .{ (a: int, b: int) -> int => a + b };
let f = .{ (a, b) => a + b };

// 具名函数表达式
let f = fun fib (n: int, a: int = 0, b: int = 1) { 
    if (n > 0) fib(n - 1, b, a + b) else a 
  };
let f = .fun fib { (n: int, a: int = 0, b: int = 1) => 
    if (n > 0) fib(n - 1, b, a + b) else a 
  };
```

### 尾块函数

跟在表达式后面的块是尾块函数，要求表达式的类型是输入一个函数的函数  

```js
let a = foo { };
```

在诸如 `if` 的条件表达式等地方，要使用尾块函数必须包在 `()` 内  
或者使用显式尾块语法  

```js
let a = foo.{ };
```

### 函数标注

函数标注在大括号或者 => 以及返回类型前面，没有顺序要求

```rust
fun foo() inline {} // 内联语义
fun bar() co {} // 延续语义
fun baz() inline co {}
fun ret() co -> int { 1 }

// 类型上使用时必须加 fun
let t: fun () inline co -> int;
```

### 内联函数

内联是语义的而不仅仅是优化  
内联函数不保证与不加内联一样执行  

内联函数的参数传入的函数默认也是内联的  
可以使用 `noinline` 标注来表示传入函数不内联  

可以使用 `refctx` 标注来表示传入函数是共享上下文的  
具体如何共享还得看内联函数的内部实现  

```js
fun fori[R](
  init: fun () refctx, 
  cond: fun () refctx, 
  acc: fun () refctx -> bool, 
  body: fun () refctx -> R,
) inline {
  do { init() } with
  while cond() { 
    body()
  } with { acc() }
}

fori(.{ mut i = 1 }, .{ i < 10 }, .{ alt i + 1 }) {
  print(i);
}
```

## 基础类型

### 逻辑

```js
let b: bool = true;
let b = false;
```

### 数字

`half` 是 16 位浮点数  
`float` 是 32 位浮点数  
`num` | `double` 是 64 位浮点数  
`decimal` 是 128 位浮点数  
`bignum` 是任意精度数字  

`byte` 是 8 位有符号整数  
`tiny` 是 16 位有符号整数  
`short` 是 32 位有符号整数  
`int` 是 64 位有符号整数  
`long` 是 128 位有符号整数  
`bigint` 是任意范围有符号整数  

`ubyte` 是 8 位无符号整数  
`utiny` 是 16 位无符号整数  
`ushort` 是 32 位无符号整数  
`uint` 是 64 位无符号整数  
`ulong` 是 128 位无符号整数  

```js
let i: int = 1;
let n: num = 1.5;
```

整数字面量支持任意范围  
浮点字面量虽然也支持任意范围，  
但必须具体为某一类型，默认为 `num`  

所有整数其实都是整数字面量范围的类型别名  
所以低范围的整数可以自然而然的转成高范围整数  

高于 128 位的整数一般宿主都不会具有  
所以所有高于 128 位的整数都是 `bigint`  

### 字符串

`'` 和 `"` 等价

```js
let s: str = 'asd';
let s = "asd";
let ts = 'asd  ${s}'
let c = c'a'; // 前缀 c 是字符
```

字符串具体编码同宿主字符串编码  
`char` 是 32 位无符号整数  
`rawchar` 是宿主字符类型  

### 对象

```js
let o: { a: int, b: int } = {
  a = 1; // 对象里用 ; 和 , 都可以
  b = 2,
};
let o: obj = o;
```

### 数组

```js
let a: [int] = [1, 2, 3];
let a: [int; 3] = [1, 2, 3]; // 定长数组
```

### 元祖

```js
let t: (int, bool, str) = (1, true, 'asd');
```

### 单元类型

单元类型，类型和值都是自己

```js
let u: () = ();
```

### 顶类型

顶类型，可以放入任何值

```js
let a: any;
```

### 底类型

底类型，没有值

```js
let n: !;
```

### 范围

```ts
let a: int..int = 1..5;
type Range[T] = T..T;
```

### 字面量类型

逻辑，数字，字符串，字符 都能取部分成员作为字面量类型  

```ts
let a: 1 = 1;
```

### 约束范围

约束范围和范围的区别是前面有 `in`  
只有范围和元组可以作为 `in` 的目标  

```ts
let a: in 1..10 = 5;
type int = in -9223372036854775808..9223372036854775807;
let c: in c'a'..c'z' = c'f';
let t: in (1, 2, 3, 4, 5) = 3; // 等于 1 | 2 | 3 | 4 | 5
```

### 或类型

实际是 和（sum）类型

```ts
let a: 1 | 2 = 1;
```

### 与类型

实际是 积（product）类型

```ts
let a: { a: 1 } & { b: 2 } = { a = 1, b = 2 };
```

### 可空类型

```ts
let n: ?T = ();
let n: ?int = 1;
let n: ??int = 1; // 多层自动铺平

type ?[T] = T | (); // 伪代码
```

### 可选类型

```ts
let o: T? = none;
let o: int? = some(1);
let o: int? = 1; // 隐式转换
let o: int?? = some(1); // 不存在多层隐式转换

enum maybe[T] {
  none, some(T)
}
```

## 常量

常量是可以再编译时（即使cos是脚本也是有编译时的）确定的表达式  
常量可以作为泛型参数传入  
`const` 标注的函数可以再常量表达式中被调用  

```ts
const a: int = 1 + 1;
fun foo(a: int, b: int) const -> int => a * b;
const b: int = foo(3, 5);
```

## 定义

### 别名定义

```scala
type Foo = {
  a: int;
  b: num;
};

type Foo = Bar;
```

### 结构定义

```ocaml
class Foo {
  mut a: int;
  
  // Self 既是一个类型指向自己，也可以用于定义构造函数
  fun Self(a: int) { 
    alt self.a = a;
  }

  fun Self.foo() { // 具名构造函数
    alt a = 1;
  }

  fun Self.bar() {
    { a = 1 } // 对象构造语法
  }

  fun add(b: int) -> int {
    a + b
  }

  fun val() -> int { a }
}
let a: Foo = Foo(1);
a.add(2); // 3

a.val; // 1, 没有参数的函数可以省略括号
```

#### 静态和定义合并
 
类型的静态域实际上相当于隐私定义了一个同名的模块  
也可以手动声明同名模块  
同个模块内的同名的定义会按顺序合并  
 
```ts
class Foo {
  static let a: int = 1;
}
module Foo {
  fun get_a() -> int => a;
}
```

### 接口定义

使用结构化类型，无需显式标志实现接口

```rust
trait Foo {
  let a: int;
  fun add(b: int) -> int;
}

class Bar(a: int) : Foo {
  let a: int = a;
  fun add(b: int) -> int { a + b }
}
```

定义结构时会隐式定义同名的接口  
也可以手动定义同名接口  
手动定义同名接口后将不会隐式定义默认的同名接口  
手动定义后要求结构实现同名接口  

#### 关联类型

要求实现 Foo 的目标具有名为 Bar 的子定义成员  

```rust
trait Foo {
  type Bar;
}
```

### 枚举定义

```rust
enum Foo {
  A, B, C
}
enum Bar {
  A(int),
  B { a: int };
  
  fun some() {}
}
```

```rust
enum bool {
  true, false
}
type a = true; // 枚举的成员可以作为字面量类型独立存在  
```

## 泛型

```rust
fun id[T](v: T) -> T { v }

fun id(v: `T) -> `T { v } // 可以使用 ` 省略泛型参数

trait Functor[F: for[_]] {
  fun map[T, R](a: F[T], f: T -> R) -> F[R];
}

trait Functor[T] : for[T] {
  fun map[R](f: T -> R) -> Self[R];
}

enum Option[T] {
  Some(T),
  None,
}

// 外置约束
trait Foo[T] where T : bool {
}
```

## 定义

可以把类型定义和实现分开来  
同个模块内的定义和实现会自动合并  
相兼容的定义也会合并  
定义必须被实现  

```kotlin
def a: int -> int;
fun a(v) => v + 1;
```

## 模式匹配

### is 表达式

`<表达式> is <模式>`

`is` 表达式将返回模式是否匹配  

```csharp
if a is 1 { }
```

### if let 模式

`let` 和 `mut` 可以在 `if` 的条件中存在  
在 `if` 的条件中时将判断模式是否匹配  

```rust
if let 1 = a { }
if mut 1 = a { }
```

### mut 模式

`mut` 也可以认为是一种模式  

```rust
let mut a = 1;
```

### 通用模式

`case` 中的模式同 `is` 的  

- 绑定模式

  ```rust
  let a = 1; // let 中的绑定模式
  alt a = 1; // alt 中的绑定模式
  1 is let a; // is 中的绑定模式
  1 is mut a; // 可变绑定
  1 is let mut a; // 可变绑定
  if let a = 1 { } // if let 中的绑定模式
  ```

- 丢弃模式

  下划线表示丢弃  
  在 `case` 中也可以用来表示 `else` 

  ```rust
  let _ = 1;
  1 is _;
  if let _ = 1 { }
  ```

- 常量模式

  所有字面量都可以作为常量模式的目标  

  ```typescript
  let 1 = 1; // let 中的常量模式
  1 is 1; // is 中的常量模式
  if let 1 = 1 { } // if let 中的常量模式
  ```

- 类型模式

  ```typescript
  let a: int = 1; // let 中的类型模式
  1 is int; // is 中的直接类型模式
  1 is let a: int; // is 中的类型模式
  if let a: int = 1 { } // if let 中的类型模式

  let is int = 1; // 只匹配类型不进行绑定
  if let is int = 1 { }
  ```

- 解构模式

  - 元组

  ```typescript
  let (a, b) = (1, 2); // let 中的元组模式
  let (mut a, let b) = (1, 2); // 元组模式的内容也是模式
  let (mut a, b) = (1, 2);
  let (is int, b) = (1, 2); // let 中的元组模式
  (1, 2) is let (a, b); // is 中的元组模式
  (1, 2) is (mut a, let b); 
  (1, 2) is (int, let b);
  (1, 2) is let (is int, b);
  if let (a, b) = (1, 2); // if let 完全同 let
  ```

  - 数组
  ```typescript
  let [a, b] = [1, 2];
  let [mut a, b] = [1, 2];
  [1, 2] is let [a, b];
  if let [a, b] = [1, 2];
  ```

  - 对象
  ```typescript
  let { a, b } = { a = 1, b = 2 };
  let { mut a, b } = { a = 1, b = 2 };
  let { a is int } = { a = 1 };
  let { a = (a, b) } = { a = (1, 2) };
  { a = 1, b = 2 } is let { a, b };
  { a = 1, b = 2 } is { mut a, let b };
  { a = 1 } is { a: int };
  { a = (1, 2) } is { a = (1, 2) };
  { a = (1, 2) } is { a = let (a, b) };
  if let { a, b } = { a = 1, b = 2 }; // if let 完全同 let
  ```

  - 剩余模式

  三个点后跟一个模式  

  ```typescript
  let (a, ...b) = (1, 2, 3); // b 是 (2, 3)
  let [a, ...b] = [1, 2, 3]; // b 是 [2, 3]
  let { a, ...b } = { a = 1, b = 2, c = 3 }; // b 是 { b = 2, c = 3 }
  ```

  - 剩余丢弃模式

  两个点

  ```typescript
  let (a, ..) = (1, 2, 3);
  let [a, ..] = [1, 2, 3];
  let { a, .. } = { a = 1, b = 2, c = 3 };
  ```

- 枚举模式

  无参数枚举同字面量或类型模式  
  枚举的括号同元组或对象模式  

  ```typescript
  let some(a) = some(1);
  some(1) is let some(a);
  some(1) is some(let a);
  ```

- as 模式

  `as` 模式将模式匹配绑定到一个别名  

  ```typescript
  let (1, 2) as t = (1, 2);
  let (1, 2) as mut t = (1, 2);
  (1, 2) is (1, 2) as t;
  (1, 2) is (1, 2) as mut t;
  ```

- 比较模式

  `==` `!=` `>` `<` `<=` `<=` `!>` `!<`  
  比较模式将和值比较  
  直接使用简直就是毫无卵用  

  ```typescript
  let == 1 = 1;
  1 is == 1;
  if let == 1 = 1 { }

  if let (_, > 0) = (1, 2) { }
  ```

- 或模式

  定义时或模式的子模式中不能有 `:` 类型模式  

  ```typescript
  let 1 | 2 = 1;
  1 is 1 | 2;
  if let 1 | 2 = 1 { }
  ```

- 与模式

  ```typescript
  let 1 & a = 1;
  1 is 1 & let a;
  1 is let 1 & a;
  if let 1 & a = 1 { }
  ```

- 括号模式

  描述模式的优先级

  ```typescript
  let (1 | 2) & a = 1;
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

导入的第一个名称为包名  

```js
import a.b.c; // 导入模块内所有内容
import a.b.c as foo; // 创建一个模块别名
import a.b.c of { Foo, bar as Bar }; // 从模块中导入部分内容，括号内 , ; 都可以使用
import _.a.b.c; // 开头为 _ 表示当前包的根模块
import !.a.b.c; // ! 表示当前模块
import !!.a.b.c; // !! 表示上级模块
```

### 导出

默认模块内所有函数和定义都会导出，使用 private 来隐藏

使用 export 重新导出某个模块

```js
export a.b.c; // 重新导出模块内所有内容
export a.b.c as foo; // 将目标模块以 foo 为名字的子模块导出
export a.b.c of { Foo, bar as Bar }; // 导出模块内部分内容，括号内 , ; 都可以使用
export _.a.b.c; // 开头为 _ 表示当前包的根模块
export !.a.b.c; // ! 表示当前模块
export !!.a.b.c; // !! 表示上级模块
```

使用 export 导出一个变量

```js
export let a = 1;
```

### 分布模块

可以把模块的实现分布再多个文件  

模块的主文件必须再文件头内写明子文件  
子文件必须写明所属哪个模块  
子文件的加载顺序由主文件决定  
主文件永远比子文件早加载  

`module of` 和 `module : SomeTrait` 必须是2条语句  
并且只有主模块内能声明模块实现接口  

```ts
// a.cos
module of { // 括号内 , ; 都可以使用
  b;
}
// b.cos 
module of a;
```

### 文件头

文件头可以包含 import export module 

