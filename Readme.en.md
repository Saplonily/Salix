# Monosand

> Chinese version at [Readme.md](./Readme.md)  
> Github Repository (source): https://github.com/Saplonily/Monosand  
> Gitee Repository: https://gitee.com/Saplonily/Monosand

## Introduction
Monosand is a lightweight framework based on `.NET standard 2.0/2.1` and `.NET 6, 7, 8` focused on **2D**.  

Currently Monosand only implements the OpenGL rendering backend on Win32 platform, but this will expand as the project progresses.  

Note that the project is still in a **very early** development, so **breaking changes** can happen anytime.  

## Getting Started

The following contents only ensure the compatibility with commit `08256f6`. But in most cases you can trust them, because I will usually update this section when making a breaking change.  

### Environment Setup

Well, this is pretty easy. You just need to open the project, create a new console project targeting `.NET 8`, reference the `Monosand` project and you are good to go.  

### Run the Game

This is easy, just add the following in your `Main` method:

```cs
Game game = new();
game.Run();
```

Then launch it. You should see a black window titled 'Monosand' but nothing else. But this means you have successfully started the project!

### Overriding Update Logic & Rendering Textures

To do this, you need inherit `Game` first:

```cs 
public class MyGame : Game
{
}
```

Then modify the `Main` method we just wrote:

```cs
MyGame game = new(); 
game.Run();
```

Now let's flesh out our `MyGame`:

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

First make sure you have prepared a texture to render. If you haven't, you can use the 665x680 texture for testing from the `Test.Win32` project: [Test.Win32/TestAssets/665x680.png](./Test.Win32/TestAssets/665x680.png).  

Now let's see what the above code is doing:

- We loaded a texture using the `ResourceLoader` in the ctor of `MyGame`.
- Then we created a `SpriteBatch` with our game. This should looks familiar if you have written XNA code before!
- Finally in the render loop(by overriding `Render`) we draw the texture at the top left corner of the window using the `SpriteBatch`, and call `Flush` at the end. Note that we **MUST** remember to call this or drawing commands may be deferred causing unexpected behavior.

(Currently I'm trying to remove this limitation, actually now you needn't call `Flush` when viewport changed or `Render` ended.)

### End

Well, here comes the end. Why is it so short? Because I'm lazy XD.  

In fact there's also text rendering and basic keyboard input support, which you can find examples in `Test.Win32`.