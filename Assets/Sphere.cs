using System;
using System.Collections.Generic;
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
    public int intersectedIndex = -1;


    // 황금비를 이루는 직사각형의 너비와 높이 계산
    // (직사각형의 중심에서 각 꼭지점까지의 거리는 1)
    // Hh: 직사각형 높이의 절반
    // Wh: 직사각형 너비의 절반
    static readonly float Hh = 2 / Mathf.Sqrt(10 + 2 * Mathf.Sqrt(5));
    static readonly float Wh = Hh * (1 + Mathf.Sqrt(5)) / 2;

    readonly int[][] edgesPerFaces = {
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

    readonly Vector3[] vertices = {
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
    
    const int SubdivisionCount = 5;

    void Start() {
        var mesh = new Mesh { vertices = vertices };

        var triangles = edgesPerFaces.SelectMany(e => e).ToArray();

        mesh.triangles = triangles;
        mesh.normals = vertices;
        mesh.RecalculateNormals();

        foreach (var v in vertices) {
            Debug.Log(v.magnitude);
        }

        meshFilter.mesh = mesh;

        for (var index = 0; index < edgesPerFaces.Length; index++) {
            var edgesPerFace = edgesPerFaces[index];
            var go = new GameObject();
            var mf = go.AddComponent<MeshFilter>();
            var segmentGroupTri = new[] {
                vertices[edgesPerFace[0]],
                vertices[edgesPerFace[1]],
                vertices[edgesPerFace[2]],
            };
            
            mf.mesh = CreateSubdividedTri(segmentGroupTri, SubdivisionCount);
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

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        for (var index = 0; index < vertices.Length; index++) {
            var v = vertices[index];
            Gizmos.DrawSphere(v, 0.025f);
            Handles.Label(v, $"vt{index}");
        }
        
        GUIStyle handleStyle = new() { normal = { textColor = Color.grey }, fontSize = 20 };
        GUIStyle selectedHandleStyle = new() { normal = { textColor = Color.red }, fontSize = 20 };

        List<Vector3[]> segmentGroupTriList = new();


        
        for (var index = 0; index < edgesPerFaces.Length; index++) {
            var e = edgesPerFaces[index];
            var edgeVertices = e.Select(ee => vertices[ee]).ToArray();
            Gizmos.DrawLineStrip(edgeVertices, true);
            
            var center = edgeVertices.Aggregate(Vector3.zero, (s, v) => s + v) / edgeVertices.Length;
            var faceAngle = Vector3.Angle(SceneView.currentDrawingSceneView.camera.transform.position, center);
            if (faceAngle is >= 0 and <= 90) {
                Handles.Label(center, $"f{index}", index == intersectedIndex ? selectedHandleStyle : handleStyle);
            }
            
            segmentGroupTriList.Add(edgeVertices);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(Vector3.zero, userPos.position);
        
        if (userPos != null) {
            intersectedIndex = -1;
            var intersectPosCoords = Vector3.zero;
            for (var index = 0; index < segmentGroupTriList.Count; index++) {
                var triList = segmentGroupTriList[index];
                var position = userPos.position;
                var intersectTuv = Intersection.GetTimeAndUvCoord(position, -position, triList[0],
                    triList[1], triList[2]);
                if (intersectTuv != null) {
                    intersectedIndex = index;
                    intersectPosCoords = Intersection.GetTrilinearCoordinateOfTheHit(intersectTuv.Value.x, position, -position);
                    intersectPos.position = intersectPosCoords.normalized;
                    break;
                }
            }

            if (intersectedIndex >= 0) {
                var triList = segmentGroupTriList[intersectedIndex];
                var abCoords = CalculateAbCoords(triList[0], triList[1], triList[2], intersectPosCoords);
                Debug.Log($"Intersected with segment group {intersectedIndex}, {abCoords}");
            } else {
                Debug.Log($"Not intersected!?");
            }
        }
    }

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
    
    void Update() {
        
    }
}