using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GeoSegTest
{
    [Test]
    public void TestCalculateSegmentSubIndexForB() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.CalculateSegmentSubIndexForB(0, 0));
        
        // n = 1
        Assert.AreEqual(0, Sphere.CalculateSegmentSubIndexForB(1, 0));
        
        // n = 2
        Assert.AreEqual(0, Sphere.CalculateSegmentSubIndexForB(2, 0));
        Assert.AreEqual(3, Sphere.CalculateSegmentSubIndexForB(2, 1));
    }

    [Test]
    public void TestSearchForB()
    {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.SearchForB(0, 0, 1, 0));

        // n = 1
        Assert.AreEqual(0, Sphere.SearchForB(1, 0, 0, 0));
        
        // n = 2
        Assert.AreEqual(0, Sphere.SearchForB(2, 0, 1, 0));
        Assert.AreEqual(0, Sphere.SearchForB(2, 0, 1, 1));
        Assert.AreEqual(0, Sphere.SearchForB(2, 0, 1, 2));
        Assert.AreEqual(1, Sphere.SearchForB(2, 0, 1, 3));
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.SearchForB(2, 0, 2, 0));
        
        // n = 3
        Assert.AreEqual(0, Sphere.SearchForB(3, 0, 2, 0));
        Assert.AreEqual(0, Sphere.SearchForB(3, 0, 2, 1));
        Assert.AreEqual(0, Sphere.SearchForB(3, 0, 2, 2));
        Assert.AreEqual(0, Sphere.SearchForB(3, 0, 2, 3));
        Assert.AreEqual(0, Sphere.SearchForB(3, 0, 2, 4));
        Assert.AreEqual(1, Sphere.SearchForB(3, 0, 2, 5));
        Assert.AreEqual(1, Sphere.SearchForB(3, 0, 2, 6));
        Assert.AreEqual(1, Sphere.SearchForB(3, 0, 2, 7));
        Assert.AreEqual(2, Sphere.SearchForB(3, 0, 2, 8));
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.SearchForB(3, 0, 3, 0));
    }
    
    [Test]
    public void TestConvertToAbtCoords()
    {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.ConvertSubSegIndexToAbt(0, 0));
        
        // n = 1
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertSubSegIndexToAbt(1, 0));
        
        // n = 2
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertSubSegIndexToAbt(2, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Sphere.ConvertSubSegIndexToAbt(2, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Sphere.ConvertSubSegIndexToAbt(2, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Sphere.ConvertSubSegIndexToAbt(2, 3));
        
        // n = 3
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertSubSegIndexToAbt(3, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Sphere.ConvertSubSegIndexToAbt(3, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Sphere.ConvertSubSegIndexToAbt(3, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), true), Sphere.ConvertSubSegIndexToAbt(3, 3));
        Assert.AreEqual(Tuple.Create(new Vector2Int(2, 0), false), Sphere.ConvertSubSegIndexToAbt(3, 4));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Sphere.ConvertSubSegIndexToAbt(3, 5));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), true), Sphere.ConvertSubSegIndexToAbt(3, 6));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 1), false), Sphere.ConvertSubSegIndexToAbt(3, 7));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 2), false), Sphere.ConvertSubSegIndexToAbt(3, 8));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GeoSegTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
