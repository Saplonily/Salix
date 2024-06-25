using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Salix.UnitTest;

[TestClass]
public class ColorTests
{
    [TestMethod]
    public void TestRGBAColor()
    {
        float r = 0.5f;
        float g = 0.25f;
        float b = 0.75f;
        float a = 1f;

        var color = new Color(r, g, b, a);

        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
        Assert.AreEqual(a, color.A);
    }

    [TestMethod]
    public void TestEquality()
    {
        var color1 = new Color(0.5f, 0.25f, 0.75f);
        var color2 = new Color(0.5f, 0.25f, 0.75f);

        Assert.IsTrue(color1 == color2);
    }

    [TestMethod]
    public void TestEquality2()
    {
        var color1 = new Color(0.5f, 0.25f, 0.75f);
        var color2 = new Color(0.5f, 0.25f, 0.75f, 0.2f);

        Assert.IsTrue(!(color1 == color2));
    }

    [TestMethod]
    public void TestInequality()
    {
        var color1 = new Color(0.5f, 0.25f, 0.75f, 1.0f);
        var color2 = new Color(0.2f, 0.25f, 0.75f, 1.0f);

        Assert.IsTrue(color1 != color2);
    }

    [TestMethod]
    public void TestInequality2()
    {
        var color1 = new Color(0.5f, 0.25f, 0.75f, 0.2f);
        var color2 = new Color(0.5f, 0.25f, 0.75f, 0.2f);

        Assert.IsTrue(!(color1 != color2));
    }

    [TestMethod]
    public void TestDarken()
    {
        var color = new Color(0.5f, 0.5f, 0.5f);
        float amount = 0.25f;

        var darkened = color.Darkened(amount);

        Assert.AreEqual(color.R * (1f - amount), darkened.R);
        Assert.AreEqual(color.G * (1f - amount), darkened.G);
        Assert.AreEqual(color.B * (1f - amount), darkened.B);
        Assert.AreEqual(color.A, darkened.A);
    }

    [TestMethod]
    public void TestLighten()
    {
        var color = new Color(0.25f, 0.25f, 0.25f);
        float amount = 0.5f;

        var lightened = color.Lightened(amount);

        Assert.AreEqual(color.R + (1f - color.R) * amount, lightened.R);
        Assert.AreEqual(color.G + (1f - color.G) * amount, lightened.G);
        Assert.AreEqual(color.B + (1f - color.B) * amount, lightened.B);
        Assert.AreEqual(color.A, lightened.A);
    }

    [TestMethod]
    public void TestToString()
    {
        var color = new Color(0.5f, 0.25f, 0.75f, 0.5f);
        var str = color.ToString();
        Assert.AreEqual("(0.50, 0.25, 0.75, 0.50)", str);
    }
}