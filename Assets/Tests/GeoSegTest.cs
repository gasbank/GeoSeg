using System;
using System.Collections;
using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;

public class GeoSegTest {
    [Test]
    public void TestCalculateLocalSegmentIndexForB() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.CalculateLocalSegmentIndexForB(0, 0));

        // n = 1
        Assert.AreEqual(0, Sphere.CalculateLocalSegmentIndexForB(1, 0));

        // n = 2
        Assert.AreEqual(0, Sphere.CalculateLocalSegmentIndexForB(2, 0));
        Assert.AreEqual(3, Sphere.CalculateLocalSegmentIndexForB(2, 1));
    }

    [Test]
    public void TestSearchForB() {
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
    public void TestConvertToAbtCoords() {
        // n = 0
        Assert.Throws<IndexOutOfRangeException>(() => Sphere.ConvertLocalSegmentIndexToAbt(0, 0));

        // n = 1
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(1, 0));

        // n = 2
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(2, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Sphere.ConvertLocalSegmentIndexToAbt(2, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(2, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Sphere.ConvertLocalSegmentIndexToAbt(2, 3));

        // n = 3
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 0));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 0), true), Sphere.ConvertLocalSegmentIndexToAbt(3, 1));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 2));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 0), true), Sphere.ConvertLocalSegmentIndexToAbt(3, 3));
        Assert.AreEqual(Tuple.Create(new Vector2Int(2, 0), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 4));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 5));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 1), true), Sphere.ConvertLocalSegmentIndexToAbt(3, 6));
        Assert.AreEqual(Tuple.Create(new Vector2Int(1, 1), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 7));
        Assert.AreEqual(Tuple.Create(new Vector2Int(0, 2), false), Sphere.ConvertLocalSegmentIndexToAbt(3, 8));
    }

    [Test]
    public void TestGetInsideNeighborsOfLocalSegmentIndex() {
        // n 범위 오류
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(0, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(1, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(2, 9));
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(3, 9));

        // localSegmentIndex 범위 오류
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(4, -1));
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.GetInsideNeighborsOfLocalSegmentIndex(4, 4 * 4));

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
        }, Sphere.GetInsideNeighborsOfLocalSegmentIndex(4, 9));

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
        }, Sphere.GetInsideNeighborsOfLocalSegmentIndex(7, 16));
    }

    [Test]
    public void TestConvertAbtToNeighborAndAbt() {
        // n = 1
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(1, new(0, 0), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.O, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(1, new(0, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.A, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(1, new(-1, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.B, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(1, new(0, -1), true));

        // n = 2
        // Inside
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 0), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.Inside, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.Inside, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, 0), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.Inside, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 1), false));
        // O
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.O, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.O, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 1), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.O, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, 1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.O, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, 1), true));
        // A
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.A, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-1, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.A, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-1, 1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.A, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 1), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.A, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-1, 1), true));
        // B
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.B, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, -1), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.B, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, -1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.B, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, -1), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.B, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, -2), true));

        // OA
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OA, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, 1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OA, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OA, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(3, 0), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OA, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, 0), false));

        // OB
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OB, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, 2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OB, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 2), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OB, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.OB, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, 3), false));

        // AO
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AO, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 3), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AO, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 2), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AO, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-1, 2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AO, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 2), false));

        // AB
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AB, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-1, 0), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AB, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 0), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AB, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.AB, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(-2, 0), false));

        // BO
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BO, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(3, -2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BO, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, -2), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BO, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, -2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BO, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(2, -1), false));

        // BA
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BA, new Vector2Int(0, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, -1), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BA, new Vector2Int(0, 0), true),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, -2), true));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BA, new Vector2Int(1, 0), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(0, -2), false));
        Assert.AreEqual((Sphere.SegmentGroupNeighbor.BA, new Vector2Int(0, 1), false),
            Sphere.ConvertAbtToNeighborAndAbt(2, new(1, -2), false));
    }

    [Test]
    public void CheckCheckBottomOrTopFromParallelogram() {
        // n = 0
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.CheckBottomOrTopFromParallelogram(0, new(0, 0), false));

        // n = 1
        Assert.AreEqual(Sphere.ParallelogramGroup.Bottom, Sphere.CheckBottomOrTopFromParallelogram(1, new(0, 0), false));
        Assert.AreEqual(Sphere.ParallelogramGroup.Top, Sphere.CheckBottomOrTopFromParallelogram(1, new(0, 0), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Outside, Sphere.CheckBottomOrTopFromParallelogram(1, new(1, 0), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Outside, Sphere.CheckBottomOrTopFromParallelogram(1, new(0, 1), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Outside, Sphere.CheckBottomOrTopFromParallelogram(1, new(1, 1), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Outside, Sphere.CheckBottomOrTopFromParallelogram(1, new(-1, -1), true));

        // n = 2
        Assert.AreEqual(Sphere.ParallelogramGroup.Bottom, Sphere.CheckBottomOrTopFromParallelogram(2, new(0, 0), false));
        Assert.AreEqual(Sphere.ParallelogramGroup.Bottom, Sphere.CheckBottomOrTopFromParallelogram(2, new(0, 0), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Bottom, Sphere.CheckBottomOrTopFromParallelogram(2, new(1, 0), false));
        Assert.AreEqual(Sphere.ParallelogramGroup.Top, Sphere.CheckBottomOrTopFromParallelogram(2, new(1, 0), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Bottom, Sphere.CheckBottomOrTopFromParallelogram(2, new(0, 1), false));
        Assert.AreEqual(Sphere.ParallelogramGroup.Top, Sphere.CheckBottomOrTopFromParallelogram(2, new(0, 1), true));
        Assert.AreEqual(Sphere.ParallelogramGroup.Top, Sphere.CheckBottomOrTopFromParallelogram(2, new(1, 1), false));
        Assert.AreEqual(Sphere.ParallelogramGroup.Top, Sphere.CheckBottomOrTopFromParallelogram(2, new(1, 1), true));
    }

    [Test]
    public void TestGetNeighborsOfLocalSegmentIndex() {
        Assert.AreEqual(new (Sphere.SegmentGroupNeighbor, int)[] {
            (Sphere.SegmentGroupNeighbor.Inside, 16),
            (Sphere.SegmentGroupNeighbor.Inside, 17),
            (Sphere.SegmentGroupNeighbor.Inside, 18),
            (Sphere.SegmentGroupNeighbor.Inside, 25),
            (Sphere.SegmentGroupNeighbor.Inside, 26),
            (Sphere.SegmentGroupNeighbor.Inside, 28),
            (Sphere.SegmentGroupNeighbor.Inside, 29),
            (Sphere.SegmentGroupNeighbor.Inside, 33),
            (Sphere.SegmentGroupNeighbor.Inside, 34),
            (Sphere.SegmentGroupNeighbor.Inside, 35),
            (Sphere.SegmentGroupNeighbor.Inside, 36),
            (Sphere.SegmentGroupNeighbor.Inside, 37),
        }, Sphere.GetNeighborsOfLocalSegmentIndex(7, 27));
    }

    [Test]
    public void CheckCheckSegmentGroupNeighbor() {
        Assert.Catch<ArgumentOutOfRangeException>(() => Sphere.CheckSegmentGroupNeighbor(0, new(0, 0), false));

        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Inside, Sphere.CheckSegmentGroupNeighbor(1, new(0, 0), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.O, Sphere.CheckSegmentGroupNeighbor(1, new(0, 0), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.OA, Sphere.CheckSegmentGroupNeighbor(1, new(1, 0), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.AB, Sphere.CheckSegmentGroupNeighbor(1, new(-1, 0), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.A, Sphere.CheckSegmentGroupNeighbor(1, new(-1, 0), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.AO, Sphere.CheckSegmentGroupNeighbor(1, new(-1, 1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.OB, Sphere.CheckSegmentGroupNeighbor(1, new(0, 1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.BA, Sphere.CheckSegmentGroupNeighbor(1, new(0, -1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.B, Sphere.CheckSegmentGroupNeighbor(1, new(0, -1), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.BO, Sphere.CheckSegmentGroupNeighbor(1, new(1, -1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(1, 0), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(0, 1), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(1, 1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(1, -1), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(-1, 1), true));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(-1, -1), false));
        Assert.AreEqual(Sphere.SegmentGroupNeighbor.Outside, Sphere.CheckSegmentGroupNeighbor(1, new(-1, -1), true));
    }

    [Test]
    public void TestDetermineEdgeNeighborOrigin() {
        // Neighbor O
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.Op, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                30,
                1,
                2,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.Op, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                30,
                2,
                1,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.A, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                1,
                2,
                30,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.A, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                1,
                30,
                2,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.B, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                2,
                1,
                30,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.O, Sphere.EdgeNeighborOrigin.B, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                2,
                30,
                1,
            }));

        // Neighbor A
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.O, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                0,
                20,
                2,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.O, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                0,
                2,
                20,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.Ap, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                20,
                2,
                0,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.Ap, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                20,
                0,
                2,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.B, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                2,
                20,
                0,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.A, Sphere.EdgeNeighborOrigin.B, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                2,
                0,
                20,
            }));

        // Neighbor B
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.O, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                0,
                1,
                10,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.O, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                0,
                10,
                1,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.A, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                1,
                10,
                0,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.A, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                1,
                0,
                10,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.Bp, Sphere.AxisOrientation.CCW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                10,
                1,
                0,
            }));
        Assert.AreEqual((Sphere.EdgeNeighbor.B, Sphere.EdgeNeighborOrigin.Bp, Sphere.AxisOrientation.CW), Sphere.DetermineCoordinate(
            new[] {
                0,
                1,
                2,
            }, Sphere.AxisOrientation.CCW, new[] {
                10,
                0,
                1,
            }));
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