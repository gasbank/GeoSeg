using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class Geocoding {
    // 황금비를 이루는 직사각형의 너비와 높이 계산
    // (직사각형의 중심에서 각 꼭지점까지의 거리는 1)
    // Hh: 직사각형 높이의 절반
    // Wh: 직사각형 너비의 절반
    static readonly float Hh = 2 / Mathf.Sqrt(10 + 2 * Mathf.Sqrt(5));
    static readonly float Wh = Hh * (1 + Mathf.Sqrt(5)) / 2;

    public static readonly int[][] VertIndexPerFaces = {
        // Face 0
        new[] {
            0,
            1,
            7,
        },
        // Face 1
        new[] {
            0,
            4,
            1,
        },
        // Face 2
        new[] {
            0,
            7,
            9,
        },
        // Face 3
        new[] {
            0,
            8,
            4,
        },
        // Face 4
        new[] {
            0,
            9,
            8,
        },
        // Face 5
        new[] {
            1,
            11,
            10,
        },
        // Face 6
        new[] {
            1,
            10,
            7,
        },
        // Face 7
        new[] {
            1,
            4,
            11,
        },
        // Face 8
        new[] {
            2,
            3,
            6,
        },
        // Face 9
        new[] {
            2,
            5,
            3,
        },
        // Face 10
        new[] {
            2,
            6,
            10,
        },
        // Face 11
        new[] {
            2,
            10,
            11,
        },
        // Face 12
        new[] {
            2,
            11,
            5,
        },
        // Face 13
        new[] {
            3,
            5,
            8,
        },
        // Face 14
        new[] {
            3,
            8,
            9,
        },
        // Face 15
        new[] {
            3,
            9,
            6,
        },
        // Face 16
        new[] {
            4,
            5,
            11,
        },
        // Face 17
        new[] {
            4,
            8,
            5,
        },
        // Face 18
        new[] {
            6,
            7,
            10,
        },
        // Face 19
        new[] {
            6,
            9,
            7,
        },
    };

    public static readonly Vector3[] Vertices = {
        new(0, -Hh, -Wh),
        new(0, +Hh, -Wh),
        new(0, +Hh, +Wh),
        new(0, -Hh, +Wh),
        new(-Wh, 0, -Hh),
        new(-Wh, 0, +Hh),
        new(+Wh, 0, +Hh),
        new(+Wh, 0, -Hh),
        new(-Hh, -Wh, 0),
        new(+Hh, -Wh, 0),
        new(+Hh, +Wh, 0),
        new(-Hh, +Wh, 0),
    };

    public static readonly Vector3[][] SegmentGroupTriList = BuildSegmentGroupTriList();

    static readonly AxisOrientation[] FaceAxisOrientationList = BuildFaceAxisOrientationList();

    public static readonly (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)[][] NeighborFaceInfoList =
        BuildNeighborFaceInfoList();

    static AxisOrientation DetermineAxisOrientation(int[] face) {
        var faceVerts = face.Select(e => Vertices[e]).ToArray();
        var axis = ((faceVerts[0] + faceVerts[1] + faceVerts[2]) / 3).normalized;
        return Vector3.SignedAngle(faceVerts[1] - faceVerts[0], faceVerts[2] - faceVerts[0], axis) > 0
            ? AxisOrientation.CW
            : AxisOrientation.CCW;
    }

    public static (EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) DetermineCoordinate(int[] face,
        AxisOrientation faceAxisOrientation, int[] neighbor) {
        if (face.Length != 3) {
            throw new ArgumentOutOfRangeException(nameof(face), face, null);
        }

        if (neighbor.Length != 3) {
            throw new ArgumentOutOfRangeException(nameof(neighbor), face, null);
        }

        if (face.Intersect(neighbor).Count() != 2) {
            throw new("Not neighbor");
        }

        var neighborListed = neighbor.ToList();
        var neighborIndexOfFaceVerts = face.Select(e => neighborListed.IndexOf(e)).ToArray();

        EdgeNeighbor edgeNeighbor;
        EdgeNeighborOrigin edgeNeighborOrigin;
        AxisOrientation axisOrientation;

        if (neighborIndexOfFaceVerts[0] == -1) {
            edgeNeighbor = EdgeNeighbor.O;

            if (neighborIndexOfFaceVerts[1] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.A;
                axisOrientation = neighbor[1] == face[2] ? AxisOrientation.CW : AxisOrientation.CCW;
            } else if (neighborIndexOfFaceVerts[2] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.B;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.CCW : AxisOrientation.CW;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Op;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.CW : AxisOrientation.CCW;
            }
        } else if (neighborIndexOfFaceVerts[1] == -1) {
            edgeNeighbor = EdgeNeighbor.A;

            if (neighborIndexOfFaceVerts[0] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.O;
                axisOrientation = neighbor[1] == face[2] ? AxisOrientation.CCW : AxisOrientation.CW;
            } else if (neighborIndexOfFaceVerts[2] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.B;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CW : AxisOrientation.CCW;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Ap;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CCW : AxisOrientation.CW;
            }
        } else if (neighborIndexOfFaceVerts[2] == -1) {
            edgeNeighbor = EdgeNeighbor.B;

            if (neighborIndexOfFaceVerts[0] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.O;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.CW : AxisOrientation.CCW;
            } else if (neighborIndexOfFaceVerts[1] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.A;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CCW : AxisOrientation.CW;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Bp;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CW : AxisOrientation.CCW;
            }
        } else {
            throw new("Logic Error");
        }

        return (edgeNeighbor, edgeNeighborOrigin,
            faceAxisOrientation == AxisOrientation.CCW ? axisOrientation : InvertAxisOrientation(axisOrientation));
    }

    static AxisOrientation InvertAxisOrientation(AxisOrientation axisOrientation) {
        return axisOrientation == AxisOrientation.CW ? AxisOrientation.CCW : AxisOrientation.CW;
    }

    static AxisOrientation[] BuildFaceAxisOrientationList() {
        return VertIndexPerFaces.Select(DetermineAxisOrientation).ToArray();
    }

    static (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)[][] BuildNeighborFaceInfoList() {
        List<(int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)[]> neighbors = new();
        for (var i = 0; i < VertIndexPerFaces.Length; i++) {
            var neighborO = -1;
            var neighborA = -1;
            var neighborB = -1;
            var ei = VertIndexPerFaces[i];
            for (var j = 0; j < VertIndexPerFaces.Length; j++) {
                // 자기 자신은 당연히 이웃이 될 수 없다.
                if (i == j) {
                    continue;
                }

                var ej = VertIndexPerFaces[j];

                if (ej.Contains(ei[1]) && ej.Contains(ei[2])) {
                    // 원점에서 가장 멀게 맞닿은 면은 N_O다.
                    neighborO = j;
                } else if (ej.Contains(ei[0]) && ej.Contains(ei[2])) {
                    // B축에 맞닿은 면은 N_A다.
                    neighborA = j;
                } else if (ej.Contains(ei[0]) && ej.Contains(ei[1])) {
                    // A축에 맞닿은 면은 N_B다.
                    neighborB = j;
                }
            }

            if (neighborO < 0 || neighborA < 0 || neighborB < 0) {
                throw new("Logic Error");
            }


            var faceAxisOrientation = FaceAxisOrientationList[i];

            var (edgeNeighborO, edgeNeighborOriginO, axisOrientationO) =
                DetermineCoordinate(ei, faceAxisOrientation, VertIndexPerFaces[neighborO]);
            var (edgeNeighborA, edgeNeighborOriginA, axisOrientationA) =
                DetermineCoordinate(ei, faceAxisOrientation, VertIndexPerFaces[neighborA]);
            var (edgeNeighborB, edgeNeighborOriginB, axisOrientationB) =
                DetermineCoordinate(ei, faceAxisOrientation, VertIndexPerFaces[neighborB]);

            neighbors.Add(new[] {
                (neighborO, edgeNeighborO, edgeNeighborOriginO, axisOrientationO),
                (neighborA, edgeNeighborA, edgeNeighborOriginA, axisOrientationA),
                (neighborB, edgeNeighborB, edgeNeighborOriginB, axisOrientationB),
            });
        }

        return neighbors.ToArray();
    }


    public static int CalculateSegmentIndexFromLatLng(int n, float userPosLat, float userPosLng, out Vector3 intersect) {

        var userPosFromLatLng = CalculateUnitSpherePosition(userPosLat, userPosLng) * 2;

        var segGroupIndex = -1;
        intersect = Vector3.zero;
        for (var index = 0; index < SegmentGroupTriList.Length; index++) {
            var segTriList = SegmentGroupTriList[index];
            var intersectTuv = Intersection.GetTimeAndUvCoord(userPosFromLatLng, -userPosFromLatLng, segTriList[0], segTriList[1],
                segTriList[2]);
            if (intersectTuv != null) {
                segGroupIndex = index;
                intersect =
                    Intersection.GetTrilinearCoordinateOfTheHit(intersectTuv.Value.x, userPosFromLatLng, -userPosFromLatLng);
                break;
            }
        }

        if (segGroupIndex is < 0 or >= GroupCount) {
            throw new("Logic Error (no intersection)");
        }

        var triList = SegmentGroupTriList[segGroupIndex];

        var (abCoords, top) = CalculateAbCoords(n, triList[0], triList[1], triList[2], intersect);

        return ConvertToSegmentIndex(segGroupIndex, n, abCoords.x, abCoords.y, top);
    }
    public static Vector3 CalculateUnitSpherePosition(float lat, float lng) {
        var cLat = Mathf.Cos(lat);
        var sLat = Mathf.Sin(lat);

        var cLng = Mathf.Cos(lng);
        var sLng = Mathf.Sin(lng);

        return new Vector3(cLng * cLat, sLat, sLng * cLat);
    }

    static Vector3[][] BuildSegmentGroupTriList() {
        return VertIndexPerFaces.Select(e => {
            var edgeVertices = e.Select(ee => Vertices[ee]).ToArray();

            // 세그먼트 그룹 삼각형이 너무 정확히 맞닿아있으면, 삼각형 연결되는 틈새로
            // 레이 테스트 시 틈 사이로 지나가서 실패하는 경우가 있다.
            // 세그먼트 그룹 삼각형을 약간씩 키운다.
            var center = edgeVertices.Aggregate(Vector3.zero, (s, v) => s + v) / edgeVertices.Length;
            return edgeVertices.Select(ee => ee + (ee - center).normalized * 1e-6f).ToArray();
        }).ToArray();
    }

    // AB 좌표의 B 좌표로 시작되는 세그먼트 서브 인덱스의 시작값을 계산한다.
    public static int CalculateLocalSegmentIndexForB(int n, int b) {
        if (n <= 0) {
            throw new IndexOutOfRangeException(nameof(n));
        }

        return ConvertToLocalSegmentIndex(n, 0, b, false);
    }

    // 세그먼트 서브 인덱스가 주어졌을 때, B 좌표를 이진 탐색 방법으로 찾아낸다.
    // 단, 찾아낸 B 좌표는 b0 ~ b1 범위에 있다고 가정한다.
    public static int SearchForB(int n, int b0, int b1, int localSegmentIndex) {
        if (n <= 0) {
            throw new IndexOutOfRangeException(nameof(n));
        }

        if (b0 < 0) {
            throw new IndexOutOfRangeException(nameof(b0));
        }

        if (b1 >= n) {
            throw new IndexOutOfRangeException(nameof(b1));
        }

        if (b0 > b1) {
            throw new IndexOutOfRangeException($"{nameof(b0)}, {nameof(b1)}");
        }

        var localIndex0 = CalculateLocalSegmentIndexForB(n, b0);
        var localIndex1 = CalculateLocalSegmentIndexForB(n, b1);

        if (localIndex0 > localSegmentIndex || localIndex1 < localSegmentIndex) {
            throw new IndexOutOfRangeException(nameof(localSegmentIndex));
        }

        if (localIndex0 == localSegmentIndex) {
            return b0;
        }

        if (localIndex1 == localSegmentIndex) {
            return b1;
        }

        while (b1 - b0 > 1) {
            var bMid = (b0 + b1) / 2;
            switch (CalculateLocalSegmentIndexForB(n, bMid) - localSegmentIndex) {
                case < 0:
                    b0 = bMid;
                    continue;
                case > 0:
                    b1 = bMid;
                    continue;
                default:
                    return bMid;
            }
        }

        return b0;
    }

    public static Tuple<Vector2Int, bool> SplitLocalSegmentIndexToAbt(int n, int localSegmentIndex) {
        if (n <= 0) {
            throw new IndexOutOfRangeException(nameof(n));
        }

        var b = SearchForB(n, 0, n - 1, localSegmentIndex);
        var a = (localSegmentIndex - CalculateLocalSegmentIndexForB(n, b)) / 2;
        return new(new(a, b), (b % 2 == 0 && localSegmentIndex % 2 == 0 || b % 2 == 1 && localSegmentIndex % 2 == 1) == false);
    }

    public const int GroupCount = 20;

    public static (int, int) SplitSegIndexToSegGroupAndLocalSegmentIndex(int n, int segmentIndex) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n), n, null);
        }

        var segmentCountPerGroup = CalculateSegmentCountPerGroup(n);

        var unsignedMaxSegCount = (long)segmentCountPerGroup * GroupCount;

        if (unsignedMaxSegCount > (long)uint.MaxValue + 1) {
            throw new ArgumentOutOfRangeException(nameof(n), n, null);
        }

        var unsignedSegmentIndex = (uint)segmentIndex;
        if (unsignedSegmentIndex >= unsignedMaxSegCount) {
            throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, null);
        }

        var quotient = Math.DivRem(unsignedSegmentIndex, segmentCountPerGroup, out var remainder);
        if (quotient is < 0 or >= GroupCount) {
            throw new("Logic Error; quotient out of range");
        }

        if (remainder < 0 || remainder >= segmentCountPerGroup) {
            throw new("Logic Error; remainder out of range");
        }

        // (segmentGroupIndex, localSegIndex)
        return ((int)quotient, (int)remainder);
    }

    static int CalculateSegmentCountPerGroup(int n) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n), n, null);
        }

        return n * n;
    }

    // Seg Index를 Seg Group & ABT로 변환해서 반환
    public static Tuple<int, Vector2Int, bool> SplitSegIndexToSegGroupAndAbt(int n, int segmentIndex) {
        var (segmentGroupIndex, localSegIndex) = SplitSegIndexToSegGroupAndLocalSegmentIndex(n, segmentIndex);
        var (abCoords, top) = SplitLocalSegmentIndexToAbt(n, localSegIndex);
        return Tuple.Create(segmentGroupIndex, abCoords, top);
    }

    // Seg Index의 세 정점 위치를 계산해서 반환
    public static Vector3[] CalculateSegmentCorners(int n, int segmentIndex, bool normalize) {
        var (segGroupIndex, ab, t) = SplitSegIndexToSegGroupAndAbt(n, segmentIndex);
        var segGroupVerts = VertIndexPerFaces[segGroupIndex].Select(e => Vertices[e]).ToArray();
        var axisA = (segGroupVerts[1] - segGroupVerts[0]) / n;
        var axisB = (segGroupVerts[2] - segGroupVerts[0]) / n;

        var parallelogramCorner = segGroupVerts[0] + ab.x * axisA + ab.y * axisB;

        var ret = t
            ? new[] {
                parallelogramCorner + axisA + axisB,
                parallelogramCorner + axisA,
                parallelogramCorner + axisB,
            }
            : new[] {
                parallelogramCorner,
                parallelogramCorner + axisA,
                parallelogramCorner + axisB,
            };

        return normalize ? ret.Select(e => e.normalized).ToArray() : ret;
    }

    // Seg Index의 중심 좌표를 계산해서 반환 (정규화되어 단위구 위의 점으로 변환되어 반환)
    public static Vector3 CalculateSegmentCenter(int n, int segmentIndex) {
        var (segGroupIndex, ab, t) = SplitSegIndexToSegGroupAndAbt(n, segmentIndex);
        var segGroupVerts = VertIndexPerFaces[segGroupIndex].Select(e => Vertices[e]).ToArray();
        var axisA = (segGroupVerts[1] - segGroupVerts[0]) / n;
        var axisB = (segGroupVerts[2] - segGroupVerts[0]) / n;

        var parallelogramCorner = segGroupVerts[0] + ab.x * axisA + ab.y * axisB;
        var offset = axisA + axisB;
        return (parallelogramCorner + offset / 3 * (t ? 2 : 1)).normalized;
    }

    // Seg Index의 중심 좌표의 위도 경도를 계산해서 반환
    public static (float, float) CalculateSegmentCenterLatLng(int n, int segmentIndex) {
        return CalculateLatLng(CalculateSegmentCenter(n, segmentIndex));
    }

    // n(분할 횟수), AB 좌표, top여부 세 개를 조합해 세그먼트 그룹 내 인덱스를 계산하여 반환한다.
    static int ConvertToLocalSegmentIndex(int n, int a, int b, bool top) {
        if (n <= 0) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        if (a + b >= n) {
            throw new ArgumentOutOfRangeException($"{nameof(a)} + {nameof(b)}");
        }

        if (a + b == n - 1 && top) {
            // 오른쪽 가장자리 변에 맞닿은 세그먼트는 top일 수 없다.
            throw new ArgumentOutOfRangeException($"{nameof(a)} + {nameof(b)} + {nameof(top)}");
        }

        var parallelogramIndex = b * n - (b - 1) * b / 2 + a;
        return parallelogramIndex * 2 - b + (top ? 1 : 0);
    }

    // 세그먼트 그룹 인덱스, n(분할 횟수), AB 좌표, top여부 네 개를 조합 해 전역 세그먼트 인덱스를 계산하여 반환한다.
    static int ConvertToSegmentIndex(int segmentGroupIndex, int n, int a, int b, bool top) {
        var localSegmentIndex = ConvertToLocalSegmentIndex(n, a, b, top);

        return ConvertToSegmentIndex(n, segmentGroupIndex, localSegmentIndex);
    }

    public static int ConvertToSegmentIndex(int n, int segmentGroupIndex, int localSegmentIndex) {
        var segmentCountPerGroup = CalculateSegmentCountPerGroup(n);
        if (segmentGroupIndex is < 0 or >= GroupCount) {
            throw new ArgumentOutOfRangeException(nameof(segmentGroupIndex), segmentGroupIndex, null);
        }

        if (localSegmentIndex < 0 || localSegmentIndex >= segmentCountPerGroup) {
            throw new ArgumentOutOfRangeException(nameof(localSegmentIndex), localSegmentIndex, null);
        }

        return (int)((long)segmentCountPerGroup * segmentGroupIndex + localSegmentIndex);
    }

    // 세 꼭지점(ip0, ip1, ip2)으로 정의되는 삼각형 내의 특정 지점(intersect)를 AB 좌표, top여부로 변환하여 반환한다.
    static Tuple<Vector2Int, bool> CalculateAbCoords(int n, Vector3 ip0, Vector3 ip1, Vector3 ip2, Vector3 intersect) {
        var p = intersect - ip0;
        var p01 = ip1 - ip0;
        var p02 = ip2 - ip0;

        var a = Vector3.Dot(p, p01) / p01.sqrMagnitude;
        var b = Vector3.Dot(p, p02) / p02.sqrMagnitude;

        var tanDelta = Vector3.Cross(p01, p02).magnitude / Vector3.Dot(p01, p02);

        var ap = a - (p - a * p01).magnitude / (tanDelta * p01.magnitude);
        var bp = b - (p - b * p02).magnitude / (tanDelta * p02.magnitude);

        //Debug.Log($"Original: {intersect.x}, {intersect.y}, {intersect.z}");
        //var check = ip0 + ap * p01 + bp * p02;
        //Debug.Log($"Check: {check.x}, {check.y}, {check.z}");

        var apf = math.modf(ap * n, out var api);
        var bpf = math.modf(bp * n, out var bpi);

        //Debug.Log($"api: {apf}, bpi: {bpf}, sum: {apf + bpf}");

        //ap * SubdivisionCount
        return Tuple.Create(new Vector2Int((int)api, (int)bpi), apf + bpf > 1);
    }

    // 임의의 지점 p의 위도, 경도를 계산하여 라디안으로 반환한다.
    // 위도는 -pi/2 ~ +pi/2 범위
    // 경도는 -pi ~ pi 범위다.
    public static (float, float) CalculateLatLng(Vector3 p) {
        var pNormalized = p.normalized;

        var lng = Normalize(Mathf.Atan2(pNormalized.z, pNormalized.x), -Mathf.PI, Mathf.PI);

        var lngVec = new Vector3(Mathf.Cos(lng), 0, Mathf.Sin(lng));

        var lat = Normalize(Mathf.Sign(pNormalized.y) * Vector3.Angle(lngVec, pNormalized) * Mathf.Deg2Rad, -Mathf.PI / 2,
            Mathf.PI / 2);
        return (lat, lng);
    }

    // https://stackoverflow.com/questions/1628386/normalise-orientation-between-0-and-360
    // Normalizes any number to an arbitrary range 
    // by assuming the range wraps around when going below min or above max 
    static float Normalize(float value, float start, float end) {
        var width = end - start; // 
        var offsetValue = value - start; // value relative to 0

        return (offsetValue - (Mathf.Floor(offsetValue / width) * width)) + start;
        // + start to reset back to start of original range
    }

    // 세그먼트 그룹 내에서 완전히 모든 이웃 세그먼트가 찾아지는 경우에 대해
    // 이웃 세그먼트 서브 인덱스를 모두 반환한다.
    public static int[] GetInsideNeighborsOfLocalSegmentIndex(int n, int localSegmentIndex) {
        if (n < 4) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        if (localSegmentIndex < 0 || localSegmentIndex >= n * n) {
            throw new ArgumentOutOfRangeException(nameof(localSegmentIndex));
        }

        var (ab, t) = SplitLocalSegmentIndexToAbt(n, localSegmentIndex);

        List<int> ret = new();

        if (t == false) {
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x - 1, ab.y - 1, true));
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x, ab.y - 1, false));
        }

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x, ab.y - 1, true));

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x + 1, ab.y - 1, false));
        ret.Add(ConvertToLocalSegmentIndex(n, ab.x + 1, ab.y - 1, true));

        if (t == false) {
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x - 1, ab.y, false));
        }

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x - 1, ab.y, true));

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x, ab.y, t == false));

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x + 1, ab.y, false));
        if (t) {
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x + 1, ab.y, true));
        }

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x - 1, ab.y + 1, false));
        ret.Add(ConvertToLocalSegmentIndex(n, ab.x - 1, ab.y + 1, true));

        ret.Add(ConvertToLocalSegmentIndex(n, ab.x, ab.y + 1, false));
        if (t) {
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x, ab.y + 1, true));
            ret.Add(ConvertToLocalSegmentIndex(n, ab.x + 1, ab.y + 1, false));
        }

        return ret.ToArray();
    }

    static (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) GetNeighborInfoOfSegGroupIndex(int segGroupIndex,
        SegmentGroupNeighbor neighbor) {
        if (segGroupIndex is < 0 or >= GroupCount) {
            throw new ArgumentOutOfRangeException(nameof(segGroupIndex), segGroupIndex, null);
        }

        switch (neighbor) {
            case SegmentGroupNeighbor.Inside:
                throw new ArgumentException(nameof(neighbor));
            case SegmentGroupNeighbor.O:
                return NeighborFaceInfoList[segGroupIndex][0];
            case SegmentGroupNeighbor.A:
                return NeighborFaceInfoList[segGroupIndex][1];
            case SegmentGroupNeighbor.B:
                return NeighborFaceInfoList[segGroupIndex][2];
            case SegmentGroupNeighbor.OA:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 0, 1);
            case SegmentGroupNeighbor.OB:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 0, 2);
            case SegmentGroupNeighbor.AO:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 1, 0);
            case SegmentGroupNeighbor.AB:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 1, 2);
            case SegmentGroupNeighbor.BO:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 2, 0);
            case SegmentGroupNeighbor.BA:
                return GetNeighbor2SegGroupIndex(segGroupIndex, 2, 1);
            case SegmentGroupNeighbor.Outside:
            default:
                throw new ArgumentOutOfRangeException(nameof(neighbor), neighbor, null);
        }
    }

    static (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) GetNeighbor2SegGroupIndex(int segGroupIndex, int n1Index,
        int n2Index) {
        var segGroupVertIndexList = VertIndexPerFaces[segGroupIndex];
        var (n1SegGroupIndex, _, _, _) = NeighborFaceInfoList[segGroupIndex][n1Index];
        return NeighborFaceInfoList[n1SegGroupIndex].ToList().FirstOrDefault(e => {
            var (n2SegGroupIndex, _, _, _) = e;
            return n2SegGroupIndex != segGroupIndex
                   && VertIndexPerFaces[n2SegGroupIndex].Contains(segGroupVertIndexList[n2Index]) == false;
        });
    }

    // 세그먼트 그룹 내에서 완전히 모든 이웃 세그먼트가 찾아지지 않고,
    // 세그먼트 그룹 경계를 벗어나는 이웃이 포함되는 경우에 
    // 이웃 세그먼트 인덱스를 모두 반환한다.
    // 여러 세그먼트 그룹에 걸쳐야하므로, 세그먼트 서브 인덱스로 조회할 수는 없다.
    public static int[] GetNeighborsOfSegmentIndex(int n, int segmentIndex) {
        var (segGroupIndex, localSegmentIndex) = SplitSegIndexToSegGroupAndLocalSegmentIndex(n, segmentIndex);

        var baseAxisOrientation = FaceAxisOrientationList[segGroupIndex];

        List<int> neighborSegIndexList = new();
        var neighborInfo = NeighborFaceInfoList[segGroupIndex];

        var neighborsAsRelativeAbt = GetLocalSegmentIndexNeighborsAsAbt(false, n, localSegmentIndex);

        foreach (var (neighbor, neighborAb, neighborT) in neighborsAsRelativeAbt) {
            switch (neighbor) {
                case SegmentGroupNeighbor.Inside:
                    var neighborSegIndex = ConvertToSegmentIndex(n, segGroupIndex,
                        ConvertToLocalSegmentIndex(n, neighborAb.x, neighborAb.y, neighborT));

                    neighborSegIndexList.Add(neighborSegIndex);
                    break;
                case SegmentGroupNeighbor.O:
                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighborInfo[0], n, neighborAb,
                        neighborT));
                    break;
                case SegmentGroupNeighbor.A:
                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighborInfo[1], n, neighborAb,
                        neighborT));
                    break;
                case SegmentGroupNeighbor.B:
                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighborInfo[2], n, neighborAb,
                        neighborT));
                    break;
                case SegmentGroupNeighbor.OA:
                case SegmentGroupNeighbor.OB: {
                    var neighbor1Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, SegmentGroupNeighbor.O);
                    var (neighborAb1, neighborT1) = ConvertCoordinate(baseAxisOrientation, neighbor1Info, n, neighborAb, neighborT);
                    var neighbor2Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, neighbor);

                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighbor2Info, n, neighborAb1,
                        neighborT1));
                    break;
                }
                case SegmentGroupNeighbor.AO:
                case SegmentGroupNeighbor.AB: {
                    var neighbor1Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, SegmentGroupNeighbor.A);
                    var (neighborAb1, neighborT1) = ConvertCoordinate(baseAxisOrientation, neighbor1Info, n, neighborAb, neighborT);
                    var neighbor2Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, neighbor);

                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighbor2Info, n, neighborAb1,
                        neighborT1));
                    break;
                }
                case SegmentGroupNeighbor.BO:
                case SegmentGroupNeighbor.BA: {
                    var neighbor1Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, SegmentGroupNeighbor.B);
                    var (neighborAbx, neighborTx) = ConvertCoordinate(baseAxisOrientation, neighbor1Info, n, neighborAb, neighborT);
                    var neighbor2Info = GetNeighborInfoOfSegGroupIndex(segGroupIndex, neighbor);

                    neighborSegIndexList.Add(ConvertCoordinateByNeighborInfo(baseAxisOrientation, neighbor2Info, n, neighborAbx,
                        neighborTx));
                    break;
                }
                case SegmentGroupNeighbor.Outside:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return neighborSegIndexList.ToArray();
    }

    static int ConvertCoordinateByNeighborInfo(AxisOrientation baseAxisOrientation,
        (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) neighborInfo, int n, Vector2Int neighborAb, bool neighborT) {
        var (neighborSegGroupIndex, edgeNeighbor, edgeNeighborOrigin, axisOrientation) = neighborInfo;
        var (convertedAb, convertedT) = ConvertCoordinate(baseAxisOrientation, edgeNeighbor, edgeNeighborOrigin, axisOrientation, n,
            neighborAb, neighborT);
        var convertedNeighborLocalSegIndex = ConvertToLocalSegmentIndex(n, convertedAb.x, convertedAb.y, convertedT);
        var convertedNeighborSegIndex = ConvertToSegmentIndex(n, neighborSegGroupIndex, convertedNeighborLocalSegIndex);
        return convertedNeighborSegIndex;
    }

    public static (SegmentGroupNeighbor, int)[] GetLocalSegmentIndexNeighbors(int n, int localSegmentIndex) {
        return GetLocalSegmentIndexNeighborsAsAbt(true, n, localSegmentIndex).Select(e => {
            var (neighbor, abNeighbor, tNeighbor) = e;
            return (neighbor, ConvertToLocalSegmentIndex(n, abNeighbor.x, abNeighbor.y, tNeighbor));
        }).ToArray();
    }

    static (SegmentGroupNeighbor, Vector2Int, bool)[]
        GetLocalSegmentIndexNeighborsAsAbt(bool canonical, int n, int localSegmentIndex) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        var (ab, t) = SplitLocalSegmentIndexToAbt(n, localSegmentIndex);

        // i) 정이십면체 그대로인 상태일 때는 특수한 케이스이다. (테스트 안해봄)
        if (n == 1) {
            // n=1일 때는 top, bottom이라는 개념 없이 모두 bottom이다.
            return NeighborOffsetSubdivisionOne.Select(e => {
                var (da, db, dt) = e;
                return ConvertAbtToNeighborAbt(canonical, n, new(ab.x + da, ab.y + db), dt);
            }).ToArray();
        }

        // iii) 그 밖의 경우
        if (t) {
            // top인 경우
            return NeighborOffsetTop.Select(e => {
                var (da, db, dt) = e;
                return ConvertAbtToNeighborAbt(canonical, n, new(ab.x + da, ab.y + db), dt);
            }).ToArray();
        }

        // bottom인 경우에는 코너인지 아닌지에 따라서도 처리가 달라진다.
        return GetLocalSegmentIndexNeighborsAsAbtCase3Bottom(canonical, n, ab);
    }

    static readonly (int, int, bool)[] NeighborOffsetSubdivisionOne = {
        // 하단 행
        (0, -1, false),
        (0, -1, true),
        (1, -1, false),
        // 지금 행
        (-1, 0, false),
        (-1, 0, true),
        (0, 0, true),
        (1, 0, false),
        // 상단 행
        (-1, 1, false),
        (0, 1, false),
    };

    static readonly (int, int, bool)[] NeighborOffsetTop = {
        // 하단 행
        (0, -1, true),
        (1, -1, false),
        (1, -1, true),
        // 지금 행
        (-1, 0, true),
        (0, 0, false),
        (1, 0, false),
        (1, 0, true),
        // 상단 행
        (-1, 1, false),
        (-1, 1, true),
        (0, 1, false),
        (0, 1, true),
        (1, 1, false),
    };

    static readonly (int, int, bool)[] NeighborOffsetBottom = {
        // 하단 행
        (-1, -1, true),
        (0, -1, false),
        (0, -1, true),
        (1, -1, false),
        (1, -1, true),
        // 지금 행
        (-1, 0, false),
        (-1, 0, true),
        (0, 0, true),
        (1, 0, false),
        // 상단 행
        (-1, 1, false),
        (-1, 1, true),
        (0, 1, false),
    };

    static (SegmentGroupNeighbor, Vector2Int, bool)[] GetLocalSegmentIndexNeighborsAsAbtCase3Bottom(bool canonical, int n,
        Vector2Int ab) {

        var skipIndex = -1;
        if (ab.x == 0 && ab.y == 0) {
            skipIndex = 0;
        } else if (ab.x == n - 1 && ab.y == 0) {
            skipIndex = 4;
        } else if (ab.x == 0 && ab.y == n - 1) {
            skipIndex = 10;
        }

        return NeighborOffsetBottom.Where((_, i) => i != skipIndex).Select(e => {
            var (da, db, dt) = e;
            return ConvertAbtToNeighborAbt(canonical, n, new(ab.x + da, ab.y + db), dt);
        }).ToArray();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SegmentGroupNeighbor {
        Inside,
        O,
        A,
        B,
        OA,
        OB,
        AO,
        AB,
        BO,
        BA,
        Outside,
    }

    public enum EdgeNeighbor {
        O,
        A,
        B,
    }

    public enum EdgeNeighborOrigin {
        O,
        A,
        B,
        Op,
        Ap,
        Bp,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AxisOrientation {
        CCW,
        CW,
    }

    static (Vector2Int, bool) ConvertCoordinateToO(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(AxisOrientation.CCW, EdgeNeighbor.O, EdgeNeighborOrigin.Op, AxisOrientation.CCW, n, ab, t);
    }

    static (Vector2Int, bool) ConvertCoordinateToA(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(AxisOrientation.CCW, EdgeNeighbor.A, EdgeNeighborOrigin.O, AxisOrientation.CW, n, ab, t);
    }

    static (Vector2Int, bool) ConvertCoordinateToB(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(AxisOrientation.CCW, EdgeNeighbor.B, EdgeNeighborOrigin.O, AxisOrientation.CW, n, ab, t);
    }

    static Vector2Int Swap(int a, int b, bool swap) {
        return swap == false ? new(a, b) : new(b, a);
    }

    static (Vector2Int, bool) ConvertCoordinate(AxisOrientation baseAxisOrientation,
        (int, EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) neighborInfo, int n, Vector2Int ab, bool t) {
        var (_, edgeNeighbor, edgeNeighborOrigin, axisOrientation) = neighborInfo;
        return ConvertCoordinate(baseAxisOrientation, edgeNeighbor, edgeNeighborOrigin, axisOrientation, n, ab, t);
    }

    public static (Vector2Int, bool) ConvertCoordinate(AxisOrientation baseAxisOrientation, EdgeNeighbor edgeNeighbor,
        EdgeNeighborOrigin edgeNeighborOrigin, AxisOrientation axisOrientation, int n, Vector2Int ab, bool t) {
        var (a, b) = (ab.x, ab.y);
        var tv = t ? 1 : 0;
        var tInvert = t == false;
        var swap = baseAxisOrientation != axisOrientation;

        switch (edgeNeighbor) {
            case EdgeNeighbor.O:
                switch (edgeNeighborOrigin) {
                    case EdgeNeighborOrigin.A:
                        return (Swap(a + b + tv - n, -a + (n - 1), swap), tInvert);
                    case EdgeNeighborOrigin.B:
                        return (Swap(-b + (n - 1), a + b + tv - n, swap), tInvert);
                    case EdgeNeighborOrigin.Op:
                        return (Swap(-a + (n - 1), -b + (n - 1), swap), tInvert);
                    case EdgeNeighborOrigin.O:
                    case EdgeNeighborOrigin.Ap:
                    case EdgeNeighborOrigin.Bp:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(edgeNeighborOrigin), edgeNeighborOrigin, null);
                }
            case EdgeNeighbor.A:
                switch (edgeNeighborOrigin) {
                    case EdgeNeighborOrigin.Ap:
                        return (Swap(-b + (n - 1), a + b + tv, swap), tInvert);
                    case EdgeNeighborOrigin.B:
                        return (Swap(-a - 1, -b + (n - 1), swap), tInvert);
                    case EdgeNeighborOrigin.O:
                        return (Swap(a + b + tv, -a - 1, swap), tInvert);
                    case EdgeNeighborOrigin.A:
                    case EdgeNeighborOrigin.Bp:
                    case EdgeNeighborOrigin.Op:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(edgeNeighborOrigin), edgeNeighborOrigin, null);
                }
            case EdgeNeighbor.B:
                switch (edgeNeighborOrigin) {
                    case EdgeNeighborOrigin.A:
                        return (Swap(-a + (n - 1), -b - 1, swap), tInvert);
                    case EdgeNeighborOrigin.Bp:
                        return (Swap(a + b + tv, -a + (n - 1), swap), tInvert);
                    case EdgeNeighborOrigin.O:
                        return (Swap(-b - 1, a + b + tv, swap), tInvert);
                    case EdgeNeighborOrigin.Ap:
                    case EdgeNeighborOrigin.B:
                    case EdgeNeighborOrigin.Op:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(edgeNeighborOrigin), edgeNeighborOrigin, null);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(edgeNeighbor), edgeNeighbor, null);
        }
    }

    public enum ParallelogramGroup {
        Bottom,
        Top,
        Outside,
    }

    // ABT 좌표계가 가리키는 위치가 평행사변형의 상단인지 하단인지 판단하여 반환한다.
    // 상단이면 true, 하단이면 false를 반환하며, 범위를 벗어나는 경우에는 예외를 던진다.
    public static ParallelogramGroup CheckBottomOrTopFromParallelogram(int n, Vector2Int ab, bool t) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        if (ab.x < 0 || ab.x >= n || ab.y < 0 || ab.y >= n) {
            return ParallelogramGroup.Outside;
        }

        if (ab.x + ab.y < n - 1) {
            return ParallelogramGroup.Bottom;
        }

        return ab.x + ab.y != n - 1 || t ? ParallelogramGroup.Top : ParallelogramGroup.Bottom;
    }

    // ABT 좌표가 어떤 SegmentGroupNeighbor에 속하는지를 체크해서 반환한다. 
    public static SegmentGroupNeighbor CheckSegmentGroupNeighbor(int n, Vector2Int ab, bool t) {
        switch (CheckBottomOrTopFromParallelogram(n, ab, t)) {
            case ParallelogramGroup.Bottom:
                return SegmentGroupNeighbor.Inside;
            case ParallelogramGroup.Top:
                return SegmentGroupNeighbor.O;
            case ParallelogramGroup.Outside:
                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x - n, ab.y), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.OB;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.Outside;
                    case ParallelogramGroup.Outside:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x + n, ab.y), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.AB;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.A;
                    case ParallelogramGroup.Outside:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x, ab.y - n), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.OA;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.Outside;
                    case ParallelogramGroup.Outside:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x, ab.y + n), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.BA;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.B;
                    case ParallelogramGroup.Outside:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x + n, ab.y - n), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.AO;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.Outside;
                    case ParallelogramGroup.Outside:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (CheckBottomOrTopFromParallelogram(n, new(ab.x - n, ab.y + n), t)) {
                    case ParallelogramGroup.Bottom:
                        return SegmentGroupNeighbor.BO;
                    case ParallelogramGroup.Top:
                        return SegmentGroupNeighbor.Outside;
                    case ParallelogramGroup.Outside:
                        return SegmentGroupNeighbor.Outside;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // 하나의 세그먼트 그룹을 벗어나 인접한 세그먼트 그룹 내 세그먼트를 가리키는
    // ABT 좌표를 인접한 세그먼트 그룹과 해당 세그먼트 그룹 내의 유효한 ABT 좌표로 변환하여 반환한다.
    public static (SegmentGroupNeighbor, Vector2Int, bool) ConvertAbtToNeighborAbt(bool canonical, int n, Vector2Int ab, bool t) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        var segmentGroupNeighbor = CheckSegmentGroupNeighbor(n, ab, t);

        switch (segmentGroupNeighbor) {
            case SegmentGroupNeighbor.Inside:
                return (segmentGroupNeighbor, ab, t);
            case SegmentGroupNeighbor.O: {
                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                var (abO, tO) = ConvertCoordinateToO(n, ab, t);
                return (SegmentGroupNeighbor.O, abO, tO);

            }
            case SegmentGroupNeighbor.A: {
                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                var (abA, tA) = ConvertCoordinateToA(n, ab, t);
                return (SegmentGroupNeighbor.A, abA, tA);

            }
            case SegmentGroupNeighbor.B: {
                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                var (abB, tB) = ConvertCoordinateToB(n, ab, t);
                return (SegmentGroupNeighbor.B, abB, tB);
            }
            case SegmentGroupNeighbor.OA:
            case SegmentGroupNeighbor.OB: {
                var (abO, tO) = ConvertCoordinateToO(n, ab, t);
                var (neighbor, abOx, tOx) = ConvertAbtToNeighborAbt(canonical, n, abO, tO);

                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                return neighbor switch {
                    SegmentGroupNeighbor.A => (SegmentGroupNeighbor.OA, abOx, tOx),
                    SegmentGroupNeighbor.B => (SegmentGroupNeighbor.OB, abOx, tOx),
                    _ => throw new("Logic Error"),
                };
            }
            case SegmentGroupNeighbor.AO:
            case SegmentGroupNeighbor.AB: {
                var (abA, tA) = ConvertCoordinateToA(n, ab, t);
                var (neighbor, abAx, tAx) = ConvertAbtToNeighborAbt(canonical, n, abA, tA);

                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                return neighbor switch {
                    SegmentGroupNeighbor.O => (SegmentGroupNeighbor.AO, abAx, tAx),
                    SegmentGroupNeighbor.B => (SegmentGroupNeighbor.AB, abAx, tAx),
                    _ => throw new("Logic Error"),
                };
            }
            case SegmentGroupNeighbor.BO:
            case SegmentGroupNeighbor.BA: {
                var (abB, tB) = ConvertCoordinateToB(n, ab, t);
                var (neighbor, abBx, tBx) = ConvertAbtToNeighborAbt(canonical, n, abB, tB);

                if (!canonical) {
                    return (segmentGroupNeighbor, ab, t);
                }

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                return neighbor switch {
                    SegmentGroupNeighbor.O => (SegmentGroupNeighbor.BO, abBx, tBx),
                    SegmentGroupNeighbor.A => (SegmentGroupNeighbor.BA, abBx, tBx),
                    _ => throw new("Logic Error"),
                };
            }
            case SegmentGroupNeighbor.Outside:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new();
    }
    public static StringBuilder GenerateSourceCode() {

        StringBuilder sb = new();

        sb.AppendLine($"const int VertIndexPerFaces[{GroupCount}][3] = {{");
        for (var i = 0; i < VertIndexPerFaces.Length; i++) {
            var e = VertIndexPerFaces[i];
            sb.AppendLine($"    {{{e[0]}, {e[1]}, {e[2]}}}, // Face {i}");
        }

        sb.AppendLine("};");

        sb.AppendLine();

        sb.AppendLine("const Vector3 Vertices[] = {");
        foreach (var e in Vertices) {
            sb.AppendLine($"    {{{e.x}, {e.y}, {e.z}}},");
        }

        sb.AppendLine("};");

        sb.AppendLine();

        sb.AppendLine($"const Vector3 SegmentGroupTriList[{GroupCount}][3] = {{");
        foreach (var e in SegmentGroupTriList) {
            sb.AppendLine("    {");
            foreach (var ee in e) {
                sb.AppendLine($"        {{{ee.x}, {ee.y}, {ee.z}}},");
            }

            sb.AppendLine("    },");
        }

        sb.AppendLine("};");

        sb.AppendLine();

        sb.AppendLine($"const AxisOrientation FaceAxisOrientationList[{GroupCount}] = {{");
        foreach (var e in FaceAxisOrientationList) {
            sb.AppendLine($"    AxisOrientation_{e},");
        }

        sb.AppendLine("};");

        sb.AppendLine();

        sb.AppendLine($"const NeighborInfo NeighborFaceInfoList[{GroupCount}][3] = {{");
        foreach (var e in NeighborFaceInfoList) {
            sb.AppendLine("    {");
            foreach (var ee in e) {
                var (neighborSegGroupIndex, edgeNeighbor, edgeNeighborOrigin, axisOrientation) = ee;
                sb.AppendLine(
                    $"        {{{neighborSegGroupIndex}, EdgeNeighbor_{edgeNeighbor}, EdgeNeighborOrigin_{edgeNeighborOrigin}, AxisOrientation_{axisOrientation}}},");
            }

            sb.AppendLine("    },");
        }

        sb.AppendLine("};");

        return sb;
    }
}