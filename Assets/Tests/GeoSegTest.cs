using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GeoSegTest {
    [Test]
    public void TestCalculateLocalSegmentIndexForB() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Geocoding.CalculateLocalSegmentIndexForB(0, 0));

        // n = 1
        Assert.AreEqual(0, Geocoding.CalculateLocalSegmentIndexForB(1, 0));

        // n = 2
        Assert.AreEqual(0, Geocoding.CalculateLocalSegmentIndexForB(2, 0));
        Assert.AreEqual(3, Geocoding.CalculateLocalSegmentIndexForB(2, 1));
    }

    [Test]
    public void TestSearchForB() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Geocoding.SearchForB(0, 0, 1, 0));

        // n = 1
        Assert.AreEqual(0, Geocoding.SearchForB(1, 0, 0, 0));

        // n = 2
        Assert.AreEqual(0, Geocoding.SearchForB(2, 0, 1, 0));
        Assert.AreEqual(0, Geocoding.SearchForB(2, 0, 1, 1));
        Assert.AreEqual(0, Geocoding.SearchForB(2, 0, 1, 2));
        Assert.AreEqual(1, Geocoding.SearchForB(2, 0, 1, 3));
        Assert.Throws<IndexOutOfRangeException>(() => Geocoding.SearchForB(2, 0, 2, 0));

        // n = 3
        Assert.AreEqual(0, Geocoding.SearchForB(3, 0, 2, 0));
        Assert.AreEqual(0, Geocoding.SearchForB(3, 0, 2, 1));
        Assert.AreEqual(0, Geocoding.SearchForB(3, 0, 2, 2));
        Assert.AreEqual(0, Geocoding.SearchForB(3, 0, 2, 3));
        Assert.AreEqual(0, Geocoding.SearchForB(3, 0, 2, 4));
        Assert.AreEqual(1, Geocoding.SearchForB(3, 0, 2, 5));
        Assert.AreEqual(1, Geocoding.SearchForB(3, 0, 2, 6));
        Assert.AreEqual(1, Geocoding.SearchForB(3, 0, 2, 7));
        Assert.AreEqual(2, Geocoding.SearchForB(3, 0, 2, 8));
        Assert.Throws<IndexOutOfRangeException>(() => Geocoding.SearchForB(3, 0, 3, 0));
    }

    [Test]
    public void TestConvertToAbtCoords() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Geocoding.SplitLocalSegmentIndexToAbt(0, 0));

        // n = 1
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(1, 0));

        // n = 2
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(2, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Geocoding.SplitLocalSegmentIndexToAbt(2, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(2, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Geocoding.SplitLocalSegmentIndexToAbt(2, 3));

        // n = 3
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Geocoding.SplitLocalSegmentIndexToAbt(3, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), true), Geocoding.SplitLocalSegmentIndexToAbt(3, 3));
        Assert.AreEqual(Tuple.Create(new Vector2Int(2, 0), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 4));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 5));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), true), Geocoding.SplitLocalSegmentIndexToAbt(3, 6));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 1), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 7));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 2), false), Geocoding.SplitLocalSegmentIndexToAbt(3, 8));
    }

    [Test]
    public void TestGetInsideNeighborsOfLocalSegmentIndex() {
        // n 범위 오류
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(0, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(1, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(2, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(3, 9));

        // localSegmentIndex 범위 오류
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(4, -1));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.GetInsideNeighborsOfLocalSegmentIndex(4, 4 * 4));

        Assert.AreEqual(new[] {
            1,
            2,
            3,
            4,
            5,
            7,
            8,
            10,
            11,
            12,
            13,
            14,
        }, Geocoding.GetInsideNeighborsOfLocalSegmentIndex(4, 9));

        Assert.AreEqual(new[] {
            3,
            4,
            5,
            14,
            15,
            17,
            18,
            24,
            25,
            26,
            27,
            28,
        }, Geocoding.GetInsideNeighborsOfLocalSegmentIndex(7, 16));
    }

    [Test]
    public void TestConvertAbtToNeighborAndAbt() {
        // n = 1
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 1, new(0, 0), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.O, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 1, new(0, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.A, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 1, new(-1, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.B, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 1, new(0, -1), true));

        // n = 2
        // Inside
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 0), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.Inside, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, 0), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.Inside, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 1), false));
        // O
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.O, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.O, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 1), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.O, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, 1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.O, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, 1), true));
        // A
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.A, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-1, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.A, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-1, 1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.A, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 1), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.A, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-1, 1), true));
        // B
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.B, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, -1), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.B, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, -1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.B, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, -1), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.B, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, -2), true));

        // OA
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OA, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, 1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OA, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OA, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(3, 0), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OA, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, 0), false));

        // OB
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OB, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, 2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OB, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 2), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OB, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.OB, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, 3), false));

        // AO
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AO, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 3), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AO, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 2), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AO, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-1, 2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AO, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 2), false));

        // AB
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AB, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-1, 0), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AB, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 0), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AB, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.AB, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(-2, 0), false));

        // BO
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BO, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(3, -2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BO, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, -2), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BO, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, -2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BO, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(2, -1), false));

        // BA
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BA, new Vector2Int(0, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, -1), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BA, new Vector2Int(0, 0), true),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, -2), true));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BA, new Vector2Int(1, 0), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(0, -2), false));
        Assert.AreEqual((Geocoding.SegmentGroupNeighbor.BA, new Vector2Int(0, 1), false),
            Geocoding.ConvertAbtToNeighborAbt(true, 2, new(1, -2), false));
    }

    [Test]
    public void CheckCheckBottomOrTopFromParallelogram() {
        // n = 0
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.CheckBottomOrTopFromParallelogram(0, new(0, 0), false));

        // n = 1
        Assert.AreEqual(Geocoding.ParallelogramGroup.Bottom, Geocoding.CheckBottomOrTopFromParallelogram(1, new(0, 0), false));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Top, Geocoding.CheckBottomOrTopFromParallelogram(1, new(0, 0), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Outside, Geocoding.CheckBottomOrTopFromParallelogram(1, new(1, 0), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Outside, Geocoding.CheckBottomOrTopFromParallelogram(1, new(0, 1), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Outside, Geocoding.CheckBottomOrTopFromParallelogram(1, new(1, 1), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Outside, Geocoding.CheckBottomOrTopFromParallelogram(1, new(-1, -1), true));

        // n = 2
        Assert.AreEqual(Geocoding.ParallelogramGroup.Bottom, Geocoding.CheckBottomOrTopFromParallelogram(2, new(0, 0), false));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Bottom, Geocoding.CheckBottomOrTopFromParallelogram(2, new(0, 0), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Bottom, Geocoding.CheckBottomOrTopFromParallelogram(2, new(1, 0), false));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Top, Geocoding.CheckBottomOrTopFromParallelogram(2, new(1, 0), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Bottom, Geocoding.CheckBottomOrTopFromParallelogram(2, new(0, 1), false));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Top, Geocoding.CheckBottomOrTopFromParallelogram(2, new(0, 1), true));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Top, Geocoding.CheckBottomOrTopFromParallelogram(2, new(1, 1), false));
        Assert.AreEqual(Geocoding.ParallelogramGroup.Top, Geocoding.CheckBottomOrTopFromParallelogram(2, new(1, 1), true));
    }

    [Test]
    public void TestGetNeighborsOfLocalSegmentIndex() {
        Assert.AreEqual(new (Geocoding.SegmentGroupNeighbor, int)[] {
            (Geocoding.SegmentGroupNeighbor.Inside, 16),
            (Geocoding.SegmentGroupNeighbor.Inside, 17),
            (Geocoding.SegmentGroupNeighbor.Inside, 18),
            (Geocoding.SegmentGroupNeighbor.Inside, 25),
            (Geocoding.SegmentGroupNeighbor.Inside, 26),
            (Geocoding.SegmentGroupNeighbor.Inside, 28),
            (Geocoding.SegmentGroupNeighbor.Inside, 29),
            (Geocoding.SegmentGroupNeighbor.Inside, 33),
            (Geocoding.SegmentGroupNeighbor.Inside, 34),
            (Geocoding.SegmentGroupNeighbor.Inside, 35),
            (Geocoding.SegmentGroupNeighbor.Inside, 36),
            (Geocoding.SegmentGroupNeighbor.Inside, 37),
        }, Geocoding.GetLocalSegmentIndexNeighbors(7, 27));
    }

    [Test]
    public void CheckCheckSegmentGroupNeighbor() {
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.CheckSegmentGroupNeighbor(0, new(0, 0), false));

        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Inside, Geocoding.CheckSegmentGroupNeighbor(1, new(0, 0), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.O, Geocoding.CheckSegmentGroupNeighbor(1, new(0, 0), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.OB, Geocoding.CheckSegmentGroupNeighbor(1, new(1, 0), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.AB, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, 0), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.A, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, 0), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.AO, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, 1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.OA, Geocoding.CheckSegmentGroupNeighbor(1, new(0, 1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.BA, Geocoding.CheckSegmentGroupNeighbor(1, new(0, -1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.B, Geocoding.CheckSegmentGroupNeighbor(1, new(0, -1), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.BO, Geocoding.CheckSegmentGroupNeighbor(1, new(1, -1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(1, 0), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(0, 1), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(1, 1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(1, -1), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, 1), true));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, -1), false));
        Assert.AreEqual(Geocoding.SegmentGroupNeighbor.Outside, Geocoding.CheckSegmentGroupNeighbor(1, new(-1, -1), true));
    }

    [Test]
    public void TestDetermineEdgeNeighborOrigin() {
        // Neighbor O
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.Op, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                30,
                1,
                2,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.Op, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                30,
                2,
                1,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.A, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                1,
                2,
                30,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.A, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                1,
                30,
                2,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.B, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                2,
                1,
                30,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.O, Geocoding.EdgeNeighborOrigin.B, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                2,
                30,
                1,
            }));

        // Neighbor A
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.O, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                0,
                20,
                2,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.O, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                0,
                2,
                20,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.Ap, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                20,
                2,
                0,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.Ap, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                20,
                0,
                2,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.B, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                2,
                20,
                0,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.A, Geocoding.EdgeNeighborOrigin.B, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                2,
                0,
                20,
            }));

        // Neighbor B
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.O, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                0,
                1,
                10,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.O, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                0,
                10,
                1,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.A, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                1,
                10,
                0,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.A, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                1,
                0,
                10,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.Bp, Geocoding.AxisOrientation.CCW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                10,
                1,
                0,
            }));
        Assert.AreEqual((Geocoding.EdgeNeighbor.B, Geocoding.EdgeNeighborOrigin.Bp, Geocoding.AxisOrientation.CW), Geocoding.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Geocoding.AxisOrientation.CCW, new[] {
                10,
                0,
                1,
            }));
    }

    [Test]
    public void TestConvertCoordinate() {
        Assert.AreEqual((new Vector2Int(-1, 0), true), Geocoding.ConvertCoordinate(Geocoding.AxisOrientation.CW, Geocoding.EdgeNeighbor.O,
            Geocoding.EdgeNeighborOrigin.A,
            Geocoding.AxisOrientation.CW, 1, new(0, 0), false));
        Assert.AreEqual((new Vector2Int(-1, 0), false), Geocoding.ConvertCoordinate(Geocoding.AxisOrientation.CW, Geocoding.EdgeNeighbor.B,
            Geocoding.EdgeNeighborOrigin.O,
            Geocoding.AxisOrientation.CW, 1, new(-1, 0), true));
        
        Assert.AreEqual((new Vector2Int(-8, 7), true), Geocoding.ConvertCoordinate(Geocoding.AxisOrientation.CW, Geocoding.EdgeNeighbor.O,
            Geocoding.EdgeNeighborOrigin.A,
            Geocoding.AxisOrientation.CW, 8, new(0, 0), false));
        Assert.AreEqual((new Vector2Int(-8, 0), false), Geocoding.ConvertCoordinate(Geocoding.AxisOrientation.CW, Geocoding.EdgeNeighbor.B,
            Geocoding.EdgeNeighborOrigin.O,
            Geocoding.AxisOrientation.CW, 8, new(-8, 7), true));
    }

    [Test]
    public void TestSplitSegIndexToSegGroupAndLocalSegmentIndex() {
        const int subdivisionCountLimit = 14654;
        
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(0, 0));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(-1, 0));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCountLimit + 1, 0));
        
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(1, -1));
        Assert.Catch<ArgumentOutOfRangeException>(() => Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(1, 20));

        for (var i = 0; i < Geocoding.GroupCount; i++) {
            Assert.AreEqual((i, 0), Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(1, i));
        }

        TestSplitSeg(1);
        TestSplitSeg(2);
        TestSplitSeg(10);
        
        TestSplitSegReverse(1);
        TestSplitSegReverse(2);
        TestSplitSegReverse(10);

        // ReSharper disable once ConvertToConstant.Local
        var unsignedLastSegmentIndex = (long)subdivisionCountLimit * subdivisionCountLimit * Geocoding.GroupCount - 1;
        // ReSharper disable once IntVariableOverflowInUncheckedContext
        Assert.AreEqual((Geocoding.GroupCount - 1, subdivisionCountLimit * subdivisionCountLimit - 1), Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCountLimit, (int)unsignedLastSegmentIndex));
        // ReSharper disable once IntVariableOverflowInUncheckedContext
        Assert.AreEqual((int)unsignedLastSegmentIndex, Geocoding.ConvertToSegmentIndex(subdivisionCountLimit, Geocoding.GroupCount - 1, subdivisionCountLimit * subdivisionCountLimit - 1));
    }
    
    static void TestSplitSeg(int subdivisionCount) {
        var segPerGroup = subdivisionCount * subdivisionCount;
        var segIndexCounter = 0;
        for (var i = 0; i < Geocoding.GroupCount; i++) {
            for (var j = 0; j < segPerGroup; j++) {
                Assert.AreEqual((i, j), Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCount, segIndexCounter));
                Assert.AreEqual(segIndexCounter, Geocoding.ConvertToSegmentIndex(subdivisionCount, i, j));
                segIndexCounter++;
            }
        }
    }
    
    static void TestSplitSegReverse(int subdivisionCount) {
        var segPerGroup = subdivisionCount * subdivisionCount;
        var segIndexCounter = (int)((long)subdivisionCount * subdivisionCount * Geocoding.GroupCount - 1);
        for (var i = Geocoding.GroupCount - 1; i >= 0; i--) {
            for (var j = segPerGroup - 1; j >= 0; j--) {
                Assert.AreEqual((i, j), Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCount, segIndexCounter));
                Assert.AreEqual(segIndexCounter, Geocoding.ConvertToSegmentIndex(subdivisionCount, i, j));
                segIndexCounter--;
            }
        }
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GeoSegTestWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}