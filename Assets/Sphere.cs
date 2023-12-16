using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Sphere : MonoBehaviour {
    public Material mat;
    public MeshFilter meshFilter;

    // 황금비를 이루는 직사각형의 너비와 높이 계산
    // (직사각형의 중심에서 각 꼭지점까지의 거리는 1)
    // Hh: 직사각형 높이의 절반
    // Wh: 직사각형 너비의 절반
    static readonly float Hh = 2 / Mathf.Sqrt(10 + 2 * Mathf.Sqrt(5));
    static readonly float Wh = Hh * (1 + Mathf.Sqrt(5)) / 2;
    
    readonly int[][] edgesPerFaces = {
        new[]{0, 1, 7},
        new[]{0, 4, 1},
        new[]{0, 7, 9},
        new[]{0, 8, 4},
        new[]{0, 9, 8},
        new[]{1,11, 10},
        new[]{1,10, 7},
        new[]{1,4, 11},
        new[]{2,3, 6},
        new[]{2,5, 3},
        new[]{2,6, 10},
        new[]{2,10, 11},
        new[]{2,11, 5},
        new[]{3,5, 8},
        new[]{3,8, 9},
        new[]{3,9, 6},
        new[]{4,5, 11},
        new[]{4,8, 5},
        new[]{6,7, 10},
        new[]{6,9, 7},
    };

    readonly Vector3[] vertices = {
        new(0, -Hh,-Wh),
        new(0, +Hh,-Wh),
        new(0, +Hh,+Wh),
        new(0, -Hh,+Wh),
            
        new(-Wh, 0, -Hh),
        new(-Wh, 0, +Hh),
        new(+Wh, 0, +Hh),
        new(+Wh, 0, -Hh),
            
        new(-Hh,-Wh,0),
        new(+Hh,-Wh,0),
        new(+Hh,+Wh,0),
        new(-Hh,+Wh,0),
    };
    
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

        foreach (var edgesPerFace in edgesPerFaces) {
            var go = new GameObject();
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = CreateSubdividedTri(new [] { vertices[edgesPerFace[0]], vertices[edgesPerFace[1]], vertices[edgesPerFace[2]] }, 10);
            go.AddComponent<MeshRenderer>().material = mat;
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

        for (var index = 0; index < edgesPerFaces.Length; index++) {
            var e = edgesPerFaces[index];
            var edgeVertices = e.Select(ee => vertices[ee]).ToArray();
            Gizmos.DrawLineStrip(edgeVertices, true);
            var center = edgeVertices.Aggregate(Vector3.zero, (s, v) => s + v) / edgeVertices.Length;
            Handles.Label(center, $"f{index}");
        }
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

        return mesh;
    }
}

