# Saladim.Salix

> English version at [Readme.en.md](./Readme.en.md)  
> Github Repository (source): https://github.com/Saplonily/Salix  
> Gitee Repository: https://gitee.com/Saplonily/Salix

> 警告: 个人学习项目, 可能包含大量 TODO 和 Bug.

## 简介

Salix 是一个聚焦于 **2d** 游戏的基于 `.NET standard 2.0/2.1` 及 `.NET 6,7,8` 简易框架.  
Salix 目前只实现了 `Win32` 平台上的 `OpenGL` 渲染后端, 不过这会随着项目的推进而增加.  
注意的是, 目前项目依然处在**极早期**的开发之中, 随时都会产生**破坏性**更改.  


## 简单使用

以下内容只确保完全兼容 Commit `08256f6` 时的仓库, 不过较多情况下你可以相信它们, 因为在做出一个破坏性更改的同时我通常会更改这部分的内容.

### 环境配置

嗯, 这部分东西属于是老生常谈了, 不过这里我不会赘述, 你应该只需要打开项目, 新建一个控制台项目, 目标框架我会推荐 `.NET 8`, 然后引用 `Salix` 项目就行了.

### 启动游戏

这一步很简单, 在你的 `Main` 方法中加入如下代码:

```cs
Game game = new();
game.Run();
```

然后启动, 你应该会看到一个黑色的窗口, 标题为 'Salix', 但是什么也没有. 不过这代表你成功的启动了项目!

### 重写更新逻辑 & 渲染贴图

现在, 将你实例化的 `Game` 类换为一个继承于 `Game` 的类:

```cs
public class MyGame : Game
{
}
```

```cs
MyGame game = new();
game.Run();
```

然后来充实一下我们的 `MyMainWindow`:

```cs
public class MyGame : Game
{
    private Texture2D tex;
    private SpriteBatch spriteBatch;

    public MyGame()
    {
        tex = ResourceLoader.LoadTexture2D("TestAssets/665x680.png");
        spriteBatch = new SpriteBatch(this);
    }

    public override void Render()
    {
        RenderContext.Clear(Color.Known.CornflowerBlue);
        spriteBatch.DrawTexture(tex, DrawTransform.None);
        spriteBatch.Flush();
    }
}
```

首先, 确保你有一个用来渲染的贴图, 当然你不想找的话你可以用 `Test.Win32` 项目里测试使用的一张 665x680 大小的贴图: [Test.Win32/TestAssets/665x680.png](./Test.Win32/TestAssets/665x680.png).  
然后我们来看看上述的代码干了些什么:

- 首先我们在 `MyGame` 的构造器里通过 `ResourceLoader` 获取了一个资源加载器加载了一张贴图.
- 然后再使用当前游戏创建了一个 `SpriteBatch`, 我们将会用它来渲染我们的贴图. (如果你曾经写过 XNA 代码的话, 你可能会觉得眼熟, 这确实!)
- 最后, 在渲染循环中我们使用 `SpriteBatch` 绘制我们的贴图, 指定在窗口的左上角, 然后 `Flush` 它, 注意我们**必须记得**在渲染循环末尾调用这个方法, 否则我们的绘图指令可能会被推迟到下一帧造成不预想的结果.

(目前正在尝试解除这个限制, 现在实际上你就无需在视口更改和渲染结束时调用 `Flush` 了, 但是依然在渲染上下文的着色器更改时不会)

### 结尾

hm, 至此那么这个简单的使用说明就结束了, 可是为什么它这么短? 因为我懒 XD.  
目前其实它还支持文本渲染和简单的键盘输入, 这部分你可以在 `Test.Win32` 项目里找到测试用例.  