# Monosand

> Chinese version at [Readme.md](./Readme.md)
> Github Repository (source): https://github.com/Saplonily/Monosand
> Gitee Repository: https://gitee.com/Saplonily/Monosand

## Introduction
Monosand is a lightweight framework based on `.NET standard 2.0/2.1` and `.NET 6, 7, 8` focused on **2D**.  
And here it's.  

Currently Monosand only implements the OpenGL rendering backend on Win32 platform, but this will expand as the project progresses.  

Note that the project is still in **very early** development, so **breaking changes** can happen anytime.  

## Getting Started

The following contents only ensure the compatibility with commit `e88fd32`. But in most cases you can trust them, because I will usually update this section when making a breaking change.  

### Environment Setup

Well, this is pretty easy. You just need to open the project, create a new console project targeting .NET 7, reference the Monosand.Win32 project and you are good to go.  

### Run the Game

This is easy, just add the following in your `Main` method:

```cs
Game game = new(new Win32Platform());
game.Run();
```

Then launch it. You should see a light blue window titled 'Monosand' but nothing else. But this means you have successfully started the project!

### Overriding Update Logic & Rendering Textures

Currently there are no related APIs exposed on the `Game` class, so we have to do it through `Window`:

```cs 
public class MyMainWindow : Window
{
}
```

Then modify the `Main` method we just wrote:
```cs
Game game = new(new Win32Platform(), new MyMainWindow()); 
game.Run();
```

Now let's flesh out our `MyMainWindow`:
```cs
public class MyMainWindow : Window
{
    private Texture2D texture665x680 = null!;
    private SpriteBatch spriteBatch = null!;

    public override void OnCreated() 
    {
        texture665x680 = Game.ResourceLoader.LoadTexture2D("665x680.png");
        spriteBatch = new SpriteBatch(RenderContext);
    }

    public override void Render()
    {
        base.Render();
        spriteBatch.DrawTexture(texture665x680, Vector2.Zero); 
        spriteBatch.Flush();
    }
}
```

First make sure you have prepared a texture to render. If you haven't, you can use the 665x680 test texture from the `Test.Win32` project: [Test.Win32/665x680.png](./Test.Win32/665x680.png).  

Now let's see what the above code is doing:

- We override the `OnCreated` method of the `Window` which gets called after the window is created (before being shown). Here we load a texture using the `Game.ResourceLoader`.
- Then we create a `SpriteBatch` with the `RenderContext` of the current window. If you have written XNA code before this should looks familiar!
- Finally in the render loop we draw the texture at the top left corner of the window using the `SpriteBatch`, and call `Flush` at the end. Note that we **MUST** remember to call this or drawing commands may be deferred causing unexpected behavior.

### End

Well, here comes the end. Why is it so short? Because I'm lazy XD.

In fact there's also text rendering and basic keyboard input support, which you can find examples in [`Test.Win32`](https://github.com/Saplonily/Monosand/blob/e88fd32ed01ac309c1bf411624149f6530826561/Test.Win32/Program.cs#L18C37-L65).  

So, this ends? May be.