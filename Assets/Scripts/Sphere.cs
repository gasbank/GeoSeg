using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Sphere : MonoBehaviour {
    public Material mat;
    public Material[] matRgb;
    public MeshFilter meshFilter;
    public Transform userPos;
    public Transform intersectPos;
    public Transform centerPos;
    public Transform[] neighborPosList;
    public int intersectedSegmentGroupIndex = -1;

    public static string OverlayText;

    // 황금비를 이루는 직사각형의 너비와 높이 계산
    // (직사각형의 중심에서 각 꼭지점까지의 거리는 1)
    // Hh: 직사각형 높이의 절반
    // Wh: 직사각형 너비의 절반
    static readonly float Hh = 2 / Mathf.Sqrt(10 + 2 * Mathf.Sqrt(5));
    static readonly float Wh = Hh * (1 + Mathf.Sqrt(5)) / 2;

    static readonly int[][] VertIndexPerFaces = {
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

    static readonly (int[], EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)[] NeighborFaceIndices = BuildFaceNeighbors();

    static readonly Vector3[] Vertices = {
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

    const int SubdivisionCount = 128; //8192;
    const int RenderingSubdivisionCountLimit = 128;

    void Start() {
        var mesh = new Mesh { vertices = Vertices };

        var triangles = VertIndexPerFaces.SelectMany(e => e).ToArray();

        mesh.triangles = triangles;
        mesh.normals = Vertices;
        mesh.RecalculateNormals();

        // foreach (var v in vertices) {
        //     Debug.Log(v.magnitude);
        // }

        meshFilter.mesh = mesh;

        for (var index = 0; index < VertIndexPerFaces.Length; index++) {
            var edgesPerFace = VertIndexPerFaces[index];
            var go = new GameObject();
            var mf = go.AddComponent<MeshFilter>();
            var segmentGroupTri = new[] {
                Vertices[edgesPerFace[0]],
                Vertices[edgesPerFace[1]],
                Vertices[edgesPerFace[2]],
            };

            mf.mesh = CreateSubdividedTri(segmentGroupTri, Mathf.Min(SubdivisionCount, RenderingSubdivisionCountLimit));
            var mr = go.AddComponent<MeshRenderer>();
            mr.materials = new[] {
                mat,
                matRgb[0],
                matRgb[1],
                matRgb[2],
            };
            go.name = $"Segment Group {index}";
        }
    }

    public static (EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation) DetermineEdgeNeighborOrigin(int[] face, int[] neighbor) {
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
                axisOrientation = neighbor[1] == face[2] ? AxisOrientation.Clockwise : AxisOrientation.CounterClockwise;
            } else if (neighborIndexOfFaceVerts[2] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.B;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.CounterClockwise : AxisOrientation.Clockwise;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Op;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.Clockwise : AxisOrientation.CounterClockwise;
            }
        } else if (neighborIndexOfFaceVerts[1] == -1) {
            edgeNeighbor = EdgeNeighbor.A;

            if (neighborIndexOfFaceVerts[0] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.O;
                axisOrientation = neighbor[1] == face[2] ? AxisOrientation.CounterClockwise : AxisOrientation.Clockwise;
            } else if (neighborIndexOfFaceVerts[2] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.B;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.Clockwise : AxisOrientation.CounterClockwise;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Ap;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CounterClockwise : AxisOrientation.Clockwise;
            }
        } else if (neighborIndexOfFaceVerts[2] == -1) {
            edgeNeighbor = EdgeNeighbor.B;

            if (neighborIndexOfFaceVerts[0] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.O;
                axisOrientation = neighbor[1] == face[1] ? AxisOrientation.Clockwise : AxisOrientation.CounterClockwise;
            } else if (neighborIndexOfFaceVerts[1] == 0) {
                edgeNeighborOrigin = EdgeNeighborOrigin.A;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.CounterClockwise : AxisOrientation.Clockwise;
            } else {
                edgeNeighborOrigin = EdgeNeighborOrigin.Bp;
                axisOrientation = neighbor[1] == face[0] ? AxisOrientation.Clockwise : AxisOrientation.CounterClockwise;
            }
        } else {
            throw new("Logic Error");
        }

        return (edgeNeighbor, edgeNeighborOrigin, axisOrientation);
    }

    static (int[], EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)[] BuildFaceNeighbors() {
        List<(int[], EdgeNeighbor, EdgeNeighborOrigin, AxisOrientation)> neighbors = new();
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
                    //ej[0] == ei[0]
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

            // 면의 이웃 인덱스는 N_O, N_A, N_B 순으로 넣는다.
            neighbors.Add((new[] {
                neighborO,
                neighborA,
                neighborB,
            }, EdgeNeighbor.O, EdgeNeighborOrigin.O, AxisOrientation.Clockwise));
        }

        return neighbors.ToArray();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        for (var index = 0; index < Vertices.Length; index++) {
            var v = Vertices[index];
            Gizmos.DrawSphere(v, 0.025f);
            Handles.Label(v, $"vt{index}");
        }

        GUIStyle handleStyle = new() {
            normal = { textColor = Color.grey },
            fontSize = 20
        };
        GUIStyle selectedHandleStyle = new() {
            normal = { textColor = Color.red },
            fontSize = 20
        };

        List<Vector3[]> segmentGroupTriList = new();


        for (var index = 0; index < VertIndexPerFaces.Length; index++) {
            var e = VertIndexPerFaces[index];
            var edgeVertices = e.Select(ee => Vertices[ee]).ToArray();
            Gizmos.DrawLineStrip(edgeVertices, true);

            var center = edgeVertices.Aggregate(Vector3.zero, (s, v) => s + v) / edgeVertices.Length;
            var faceAngle = Vector3.Angle(SceneView.currentDrawingSceneView.camera.transform.position, center);
            if (faceAngle is >= 0 and <= 90) {
                Handles.Label(center, $"f{index}", index == intersectedSegmentGroupIndex ? selectedHandleStyle : handleStyle);
            }

            segmentGroupTriList.Add(edgeVertices);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(Vector3.zero, userPos.position);

        if (userPos != null) {
            intersectedSegmentGroupIndex = -1;
            var intersectPosCoords = Vector3.zero;
            for (var index = 0; index < segmentGroupTriList.Count; index++) {
                var triList = segmentGroupTriList[index];
                var position = userPos.position;
                var intersectTuv = Intersection.GetTimeAndUvCoord(position, -position, triList[0], triList[1], triList[2]);
                if (intersectTuv != null) {
                    intersectedSegmentGroupIndex = index;
                    intersectPosCoords = Intersection.GetTrilinearCoordinateOfTheHit(intersectTuv.Value.x, position, -position);
                    intersectPos.position = intersectPosCoords.normalized;
                    break;
                }
            }

            if (intersectedSegmentGroupIndex >= 0) {
                var triList = segmentGroupTriList[intersectedSegmentGroupIndex];

                var (lat, lng) = CalculateLatLng(intersectPosCoords);

                var (abCoords, top) = CalculateAbCoords(triList[0], triList[1], triList[2], intersectPosCoords);

                var segmentSubIndex = ConvertToSegmentSubIndex(SubdivisionCount, abCoords.x, abCoords.y, top);
                var segmentIndex = ConvertToSegmentIndex(intersectedSegmentGroupIndex, SubdivisionCount, abCoords.x, abCoords.y, top);

                centerPos.position = CalculateSegmentCenter(SubdivisionCount, segmentIndex);

                var neighborPosListIndex = 0;
                foreach (var (neighbor, neighborSegSubIndex) in GetNeighborsOfSubSegmentIndex(SubdivisionCount, segmentSubIndex)) {
                    if (neighbor == SegmentGroupNeighbor.Inside) {
                        var neighborSegmentIndex = ConvertToSegmentIndex(intersectedSegmentGroupIndex, neighborSegSubIndex);
                        neighborPosList[neighborPosListIndex].position =
                            CalculateSegmentCenter(SubdivisionCount, neighborSegmentIndex);
                        neighborPosList[neighborPosListIndex].gameObject.SetActive(true);
                        neighborPosListIndex++;
                    }
                }

                for (var i = neighborPosListIndex; i < neighborPosList.Length; i++) {
                    neighborPosList[i].gameObject.SetActive(false);
                }

                var (centerLat, centerLng) = CalculateSegmentCenterLatLng(SubdivisionCount, segmentIndex);

                // Neighbor (depth=1)
                var (neighborFaces, _, _, _) = NeighborFaceIndices[intersectedSegmentGroupIndex];

                OverlayText = $"Intersection Lat: {lat * Mathf.Rad2Deg}°, Lng: {lng * Mathf.Rad2Deg}°\n"
                              + $"Segment Group: {intersectedSegmentGroupIndex} ABT: {(abCoords, top)}\n"
                              + $" * Segment sub index {segmentSubIndex}\n"
                              + $" * Segment index {segmentIndex}\n"
                              + "-----------\n"
                              + $" * ABT (check): {ConvertSubSegIndexToAbt(SubdivisionCount, segmentSubIndex)}\n"
                              + $" * Segment Group & ABT (check): {ConvertSegIndexToSegGroupAndAbt(SubdivisionCount, segmentIndex)}\n"
                              + "-----------\n"
                              + $"Segment Center Lat: {centerLat * Mathf.Rad2Deg}°, Lng: {centerLng * Mathf.Rad2Deg}°\n"
                              + $"Neighbor Faces: {neighborFaces[0]}, {neighborFaces[1]}, {neighborFaces[2]}";
            } else {
                Debug.Log("?! Not intersected !?");
                OverlayText = "?! Not intersected !?";
            }
        }
    }

    // AB 좌표의 B 좌표로 시작되는 세그먼트 서브 인덱스의 시작값을 계산한다.
    public static int CalculateSegmentSubIndexForB(int n, int b) {
        if (n <= 0) {
            throw new IndexOutOfRangeException(nameof(n));
        }

        return ConvertToSegmentSubIndex(n, 0, b, false);
    }

    // 세그먼트 서브 인덱스가 주어졌을 때, B 좌표를 이진 탐색 방법으로 찾아낸다.
    // 단, 찾아낸 B 좌표는 b0 ~ b1 범위에 있다고 가정한다.
    public static int SearchForB(int n, int b0, int b1, int segmentSubIndex) {
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

        var subIndex0 = CalculateSegmentSubIndexForB(n, b0);
        var subIndex1 = CalculateSegmentSubIndexForB(n, b1);

        if (subIndex0 > segmentSubIndex || subIndex1 < segmentSubIndex) {
            throw new IndexOutOfRangeException(nameof(segmentSubIndex));
        }

        if (subIndex0 == segmentSubIndex) {
            return b0;
        }

        if (subIndex1 == segmentSubIndex) {
            return b1;
        }

        while (b1 - b0 > 1) {
            var bMid = (b0 + b1) / 2;
            switch (CalculateSegmentSubIndexForB(n, bMid) - segmentSubIndex) {
                case < 0:
                    b0 = bMid;
                    continue;
                case > 0:
                    b1 = bMid;
                    break;
                default:
                    return bMid;
            }
        }

        return b0;
    }

    public static Tuple<Vector2Int, bool> ConvertSubSegIndexToAbt(int n, int segmentSubIndex) {
        if (n <= 0) {
            throw new IndexOutOfRangeException(nameof(n));
        }

        var b = SearchForB(n, 0, n - 1, segmentSubIndex);
        var a = (segmentSubIndex - CalculateSegmentSubIndexForB(n, b)) / 2;
        return new(new(a, b), (b % 2 == 0 && segmentSubIndex % 2 == 0 || b % 2 == 1 && segmentSubIndex % 2 == 1) == false);
    }

    // 32비트 중 MSB 1비트는 부호 비트로 남겨두고, 세그먼트 그룹 인덱스는 총 0~19 범위이므로 5비트가 필요하다.
    // 즉 32비트에서 1비트+5비트를 제외한 비트를 segment sub index 공간으로 쓸 수 있다.
    const int SegmentSubIndexBitCount = 32 - 1 - 5;

    // Seg Index를 Seg Group & ABT로 변환해서 반환
    static Tuple<int, Vector2Int, bool> ConvertSegIndexToSegGroupAndAbt(int n, int segmentIndex) {
        var segSubIndex = segmentIndex & ((1 << SegmentSubIndexBitCount) - 1);
        var (abCoords, top) = ConvertSubSegIndexToAbt(n, segSubIndex);
        return Tuple.Create(segmentIndex >> SegmentSubIndexBitCount, abCoords, top);
    }

    // Seg Index의 중심 좌표를 계산해서 반환
    static Vector3 CalculateSegmentCenter(int n, int segmentIndex) {
        var (segGroupIndex, ab, t) = ConvertSegIndexToSegGroupAndAbt(n, segmentIndex);
        var segGroupVerts = VertIndexPerFaces[segGroupIndex].Select(e => Vertices[e]).ToArray();
        var axisA = (segGroupVerts[1] - segGroupVerts[0]) / n;
        var axisB = (segGroupVerts[2] - segGroupVerts[0]) / n;

        var parallelogramCorner = segGroupVerts[0] + ab.x * axisA + ab.y * axisB;
        var offset = axisA + axisB;
        return (parallelogramCorner + offset / 3 * (t ? 2 : 1)).normalized;
    }

    // Seg Index의 중심 좌표의 위도 경도를 계산해서 반환
    static (float, float) CalculateSegmentCenterLatLng(int n, int segmentIndex) {
        return CalculateLatLng(CalculateSegmentCenter(n, segmentIndex));
    }

    // n(분할 횟수), AB 좌표, top여부 세 개를 조합해 세그먼트 그룹 내 인덱스를 계산하여 반환한다.
    static int ConvertToSegmentSubIndex(int n, int a, int b, bool top) {
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
        var segmentSubIndex = ConvertToSegmentSubIndex(n, a, b, top);

        return ConvertToSegmentIndex(segmentGroupIndex, segmentSubIndex);
    }
    static int ConvertToSegmentIndex(int segmentGroupIndex, int segmentSubIndex) {

        return (segmentGroupIndex << SegmentSubIndexBitCount) | segmentSubIndex;
    }

    // 세 꼭지점(ip0, ip1, ip2)으로 정의되는 삼각형 내의 특정 지점(intersect)를 AB 좌표, top여부로 변환하여 반환한다.
    static Tuple<Vector2Int, bool> CalculateAbCoords(Vector3 ip0, Vector3 ip1, Vector3 ip2, Vector3 intersect) {
        var p = intersect - ip0;
        var p01 = ip1 - ip0;
        var p02 = ip2 - ip0;

        var a = Vector3.Dot(p, p01) / p01.sqrMagnitude;
        var b = Vector3.Dot(p, p02) / p02.sqrMagnitude;

        var tanDelta = Vector3.Cross(p01, p02).magnitude / Vector3.Dot(p01, p02);

        var ap = a - (p - a * p01).magnitude / (tanDelta * p01.magnitude);
        var bp = b - (p - b * p02).magnitude / (tanDelta * p02.magnitude);

        var apf = math.modf(ap * SubdivisionCount, out var api);
        var bpf = math.modf(bp * SubdivisionCount, out var bpi);

        //ap * SubdivisionCount
        return Tuple.Create(new Vector2Int((int)api, (int)bpi), apf + bpf > 1);
    }

    // 임의의 지점 p의 위도, 경도를 계산하여 라디안으로 반환한다.
    // 위도는 -pi/2 ~ +pi/2 범위
    // 경도는 -pi ~ pi 범위다.
    static (float, float) CalculateLatLng(Vector3 p) {
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
    public static int[] GetNeighborsOfSegmentSubIndex(int n, int segmentSubIndex) {
        if (n < 4) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        if (segmentSubIndex < 0 || segmentSubIndex >= n * n) {
            throw new ArgumentOutOfRangeException(nameof(segmentSubIndex));
        }

        var (ab, t) = ConvertSubSegIndexToAbt(n, segmentSubIndex);

        List<int> ret = new();

        if (t == false) {
            ret.Add(ConvertToSegmentSubIndex(n, ab.x - 1, ab.y - 1, true));
            ret.Add(ConvertToSegmentSubIndex(n, ab.x, ab.y - 1, false));
        }

        ret.Add(ConvertToSegmentSubIndex(n, ab.x, ab.y - 1, true));

        ret.Add(ConvertToSegmentSubIndex(n, ab.x + 1, ab.y - 1, false));
        ret.Add(ConvertToSegmentSubIndex(n, ab.x + 1, ab.y - 1, true));

        if (t == false) {
            ret.Add(ConvertToSegmentSubIndex(n, ab.x - 1, ab.y, false));
        }

        ret.Add(ConvertToSegmentSubIndex(n, ab.x - 1, ab.y, true));

        ret.Add(ConvertToSegmentSubIndex(n, ab.x, ab.y, t == false));

        ret.Add(ConvertToSegmentSubIndex(n, ab.x + 1, ab.y, false));
        if (t) {
            ret.Add(ConvertToSegmentSubIndex(n, ab.x + 1, ab.y, true));
        }

        ret.Add(ConvertToSegmentSubIndex(n, ab.x - 1, ab.y + 1, false));
        ret.Add(ConvertToSegmentSubIndex(n, ab.x - 1, ab.y + 1, true));

        ret.Add(ConvertToSegmentSubIndex(n, ab.x, ab.y + 1, false));
        if (t) {
            ret.Add(ConvertToSegmentSubIndex(n, ab.x, ab.y + 1, true));
            ret.Add(ConvertToSegmentSubIndex(n, ab.x + 1, ab.y + 1, false));
        }

        return ret.ToArray();
    }

    // 세그먼트 그룹 내에서 완전히 모든 이웃 세그먼트가 찾아지자 않고,
    // 세그먼트 그룹 경계를 벗어나는 이웃이 포함되는 경우에 
    // 이웃 세그먼트 인덱스를 모두 반환한다.
    // 여러 세그먼트 그룹에 걸쳐야하므로, 세그먼트 서브 인덱스로 조회할 수는 없다.
    public static int[] GetNeighborsOfSegmentIndex(int n, int segmentIndex) {
        return new int[] {
        };
    }

    public static (SegmentGroupNeighbor, int)[] GetNeighborsOfSubSegmentIndex(int n, int segmentSubIndex) {
        var (ab, t) = ConvertSubSegIndexToAbt(n, segmentSubIndex);
        if (t) {
            return new[] {
                // 하단 행
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y - 1), true),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y - 1), false),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y - 1), true),
                // 지금 행
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y), true),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y), false),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y), false),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y), true),
                // 상단 행
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y + 1), false),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y + 1), true),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y + 1), false),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y + 1), true),
                ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y + 1), false),
            };
        }

        return new[] {
            // 하단 행
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y - 1), true),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y - 1), false),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y - 1), true),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y - 1), false),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y - 1), true),
            // 지금 행
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y), false),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y), true),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y), true),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x + 1, ab.y), false),
            // 상단 행
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y + 1), false),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x - 1, ab.y + 1), true),
            ConvertAbtToNeighborAndLocalSegmentIndex(n, new(ab.x, ab.y + 1), false),
        };
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

    public enum AxisOrientation {
        CounterClockwise,
        Clockwise,
    }

    static (Vector2Int, bool) ConvertCoordinateToO(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(EdgeNeighbor.O, EdgeNeighborOrigin.Op, AxisOrientation.CounterClockwise, n, ab, t);
    }

    static (Vector2Int, bool) ConvertCoordinateToA(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(EdgeNeighbor.A, EdgeNeighborOrigin.O, AxisOrientation.Clockwise, n, ab, t);
    }

    static (Vector2Int, bool) ConvertCoordinateToB(int n, Vector2Int ab, bool t) {
        return ConvertCoordinate(EdgeNeighbor.B, EdgeNeighborOrigin.O, AxisOrientation.Clockwise, n, ab, t);
    }

    static Vector2Int Swap(int a, int b, bool swap) {
        return swap == false ? new(a, b) : new(b, a);
    }

    static (Vector2Int, bool) ConvertCoordinate(EdgeNeighbor edgeNeighbor, EdgeNeighborOrigin edgeNeighborOrigin,
        AxisOrientation axisOrientation, int n, Vector2Int ab, bool t) {
        var (a, b) = (ab.x, ab.y);
        var tv = t ? 1 : 0;
        var tInvert = t == false;
        var swap = axisOrientation == AxisOrientation.Clockwise;

        switch (edgeNeighbor) {
            case EdgeNeighbor.O:
                switch (edgeNeighborOrigin) {
                    case EdgeNeighborOrigin.A:
                        return (Swap(a + b + tv - (n - 1) - 1, -a + (n - 1), swap), tInvert);
                    case EdgeNeighborOrigin.B:
                        return (Swap(-b + (n - 1), a + b + tv - (n - 1) - 1, swap), tInvert);
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
                        return SegmentGroupNeighbor.OA;
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
                        return SegmentGroupNeighbor.OB;
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
    public static (SegmentGroupNeighbor, Vector2Int, bool) ConvertAbtToNeighborAndAbt(int n, Vector2Int ab, bool t) {
        if (n < 1) {
            throw new ArgumentOutOfRangeException(nameof(n));
        }

        switch (CheckSegmentGroupNeighbor(n, ab, t)) {
            case SegmentGroupNeighbor.Inside:
                return (SegmentGroupNeighbor.Inside, ab, t);
            case SegmentGroupNeighbor.O: {
                var (abO, tO) = ConvertCoordinateToO(n, ab, t);
                return (SegmentGroupNeighbor.O, abO, tO);
            }
            case SegmentGroupNeighbor.A: {
                var (abA, tA) = ConvertCoordinateToA(n, ab, t);
                return (SegmentGroupNeighbor.A, abA, tA);
            }
            case SegmentGroupNeighbor.B: {
                var (abB, tB) = ConvertCoordinateToB(n, ab, t);
                return (SegmentGroupNeighbor.B, abB, tB);
            }
            case SegmentGroupNeighbor.OA:
            case SegmentGroupNeighbor.OB: {
                var (abO, tO) = ConvertCoordinateToO(n, ab, t);
                var (neighbor, abOx, tOx) = ConvertAbtToNeighborAndAbt(n, abO, tO);
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
                var (neighbor, abAx, tAx) = ConvertAbtToNeighborAndAbt(n, abA, tA);
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
                var (neighbor, abBx, tBx) = ConvertAbtToNeighborAndAbt(n, abB, tB);
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

    static (SegmentGroupNeighbor, int) ConvertAbtToNeighborAndLocalSegmentIndex(int n, Vector2Int ab, bool t) {
        var (neighbor, abNeighbor, tNeighbor) = ConvertAbtToNeighborAndAbt(n, ab, t);
        return (neighbor, ConvertToSegmentSubIndex(n, abNeighbor.x, abNeighbor.y, tNeighbor));
    }
#endif

    static Mesh CreateSubdividedTri(IReadOnlyList<Vector3> vList, int n) {
        var totalVCount = (n + 1) * (n + 2) / 2;
        var totalFCount = n * n;

        var v0 = vList[0];
        var v1 = vList[1];
        var v2 = vList[2];

        var uA = (v1 - v0) / n;
        var uB = (v2 - v0) / n;

        var vertices = new Vector3[totalVCount];
        var edgesPerFaces = new int[totalFCount][];

        var vIndex = 0;
        var fIndex = 0;
        for (var b = 0; b <= n; b++) {
            for (var a = 0; a <= n - b; a++) {
                vertices[vIndex] = v0 + (uA * a + uB * b);
                if (a <= n - b - 1) {
                    edgesPerFaces[fIndex] = new[] {
                        vIndex,
                        vIndex + 1,
                        vIndex + n + 1 - b,
                    };
                    fIndex++;

                    if (a <= n - b - 2) {
                        edgesPerFaces[fIndex] = new[] {
                            vIndex + 1,
                            vIndex + 1 + n + 1 - b,
                            vIndex + 1 + n + 1 - b - 1,
                        };
                        fIndex++;
                    }
                }

                vIndex++;
            }
        }

        for (var index = 0; index < vertices.Length; index++) {
            vertices[index] = vertices[index].normalized;
        }

        var mesh = new Mesh { vertices = vertices };

        var triangles = edgesPerFaces.SelectMany(e => e).ToArray();

        mesh.triangles = triangles;
        mesh.normals = vertices;
        mesh.RecalculateNormals();
        mesh.subMeshCount = 4;
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(triangles.Take(3).ToList(), 1);
        mesh.SetTriangles(triangles.Skip(3 * ((n - 1) * 2)).Take(3).ToList(), 2);
        mesh.SetTriangles(triangles.Skip(3 * (totalFCount - 1)).Take(3).ToList(), 3);

        return mesh;
    }
}