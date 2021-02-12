## 变量

### 定义

```js
var a = 1;
var b: int;
```

### 修改

```js
let a = 2;
let a + 1;
```

## 控制流

### If

```js
if a { b }
if a { b } else { c }
if a { b } else if c { d }
if a do b;
if a do b else c;

if a else { b }
if a else b;
```

### Case

```js
case a;
of b { c }
of d do e;
else { f }
```

### Do

do 块在如 if 的条件内使用时必须包在括号内

```js
do { }
var a = do { }
```

### With

with 可以在同级作用域下尾随语句

```js
do { } with do { }
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

```js
var f: fn (a: int, int) -> int;
```

### 尾块函数

跟在表达式后面的块是尾块函数

```js
val a = foo { };
```

## 基础类型

### 逻辑

```js
var b: bool = true;
var b = false;
```

### 数字

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
```

### 元祖

```js
var t: (int, bool, str) = (1, true, 'asd');
```

#### 单元类型

```js
var u: () = ();
```

### 顶类型

```js
var a: any;
```

### 底类型

```js
var n: !;
```

## 定义

### 别名定义

```js
def Foo = {
  a: int,
  b: num,
};

def Foo = Bar;
```

### 结构定义

```js
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

```js
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

```js
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

```js
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
