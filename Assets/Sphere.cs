using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    public MeshFilter meshFilter;

    static readonly float hh = 2 / Mathf.Sqrt(10 + 2 * Mathf.Sqrt(5));
    static readonly float wh = hh * (1 + Mathf.Sqrt(5)) / 2;
    
    readonly int[][] edges = {
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
        new(0, -hh,-wh),
        new(0, +hh,-wh),
        new(0, +hh,+wh),
        new(0, -hh,+wh),
            
        new(-wh, 0, -hh),
        new(-wh, 0, +hh),
        new(+wh, 0, +hh),
        new(+wh, 0, -hh),
            
        new(-hh,-wh,0),
        new(+hh,-wh,0),
        new(+hh,+wh,0),
        new(-hh,+wh,0),
    };
    
    void Start() {
        var mesh = new Mesh { vertices = vertices };

        var triangles = new [] {
            0,1,2,
            2,3,0,
            
            4+0,4+1,4+2,
            4+2,4+3,4+0,
            
            8+2,8+1,8+0,
            8+0,8+3,8+2,
        };

        triangles = edges.SelectMany(e => e).ToArray();
        
        mesh.triangles = triangles;
        mesh.normals = vertices;
        mesh.RecalculateNormals();

        foreach (var v in vertices) {
            Debug.Log(v.magnitude);
        }

        meshFilter.mesh = mesh;
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        
        for (var index = 0; index < vertices.Length; index++) {
            var v = vertices[index];
            Gizmos.DrawSphere(v, 0.025f);
            Handles.Label(v, $"vt{index}");
        }

        foreach (var e in edges) {
            Gizmos.DrawLineStrip(e.Select(ee => vertices[ee]).ToArray(), true);
        }
    }
#endif
}
