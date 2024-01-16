using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Sphere : MonoBehaviour {
    public Material mat;
    public Material[] matRgb;
    public MeshFilter meshFilter;
    public Transform userPos;
    public Transform intersectPos;
    public Transform intersect2Pos;
    public Transform centerPos;
    public Transform[] neighborPosList;

    public static string overlayText;

    [Range(1, 14654)] public int subdivisionCount = 8192;
    //const int RenderingSubdivisionCountLimit = 360;
    const int RenderingSubdivisionCountLimit = 8;

    void Start() {
        var mesh = new Mesh { vertices = Geocoding.Vertices };

        var triangles = Geocoding.VertIndexPerFaces.SelectMany(e => e).ToArray();

        mesh.triangles = triangles;
        mesh.normals = Geocoding.Vertices;
        mesh.RecalculateNormals();

        // foreach (var v in vertices) {
        //     Debug.Log(v.magnitude);
        // }

        meshFilter.mesh = mesh;

        for (var index = 0; index < Geocoding.VertIndexPerFaces.Length; index++) {
            var edgesPerFace = Geocoding.VertIndexPerFaces[index];
            var go = new GameObject();
            var mf = go.AddComponent<MeshFilter>();
            var segmentGroupTri = new[] {
                Geocoding.Vertices[edgesPerFace[0]],
                Geocoding.Vertices[edgesPerFace[1]],
                Geocoding.Vertices[edgesPerFace[2]],
            };

            mf.mesh = CreateSubdividedTri(segmentGroupTri, Mathf.Min(subdivisionCount, RenderingSubdivisionCountLimit));
            var mr = go.AddComponent<MeshRenderer>();
            mr.materials = new[] {
                mat,
                matRgb[0],
                matRgb[1],
                matRgb[2],
            };
            go.name = $"Segment Group {index}";
        }

        const int testSubdivisionCount = 8192;
        Debug.Log($"Test Subdivision Count = {testSubdivisionCount}");
        Debug.Log($"Seg ID of (0, 0) = {Geocoding.CalculateSegmentIndexFromLatLng(testSubdivisionCount, 0, 0, out _)}");
        Debug.Log($"Neighbors of Seg ID (0) = {string.Join(", ", Geocoding.GetNeighborsOfSegmentIndex(testSubdivisionCount, 0))}");
        Debug.Log($"Neighbors of Seg ID (501257710) = {string.Join(", ", Geocoding.GetNeighborsOfSegmentIndex(testSubdivisionCount, 501257710))}");
        
        const float testLatDeg = 37.5275f;
        const float testLngDeg = 126.9165f;
        var testSegIndex =
            Geocoding.CalculateSegmentIndexFromLatLng(testSubdivisionCount, testLatDeg * Mathf.Deg2Rad, testLngDeg * Mathf.Deg2Rad, out _);
        var (testSegCenterLat, testSegCenterLng) = Geocoding.CalculateSegmentCenterLatLng(testSubdivisionCount, testSegIndex);
        Debug.Log($"Seg ID of ({testLatDeg} deg, {testLngDeg} deg): {testSegIndex} / Center: ({testSegCenterLat}, {testSegCenterLng}) / Center (deg): ({testSegCenterLat * Mathf.Rad2Deg}, {testSegCenterLng * Mathf.Rad2Deg})");
        
        for (var i = 0; i < Mathf.Min(subdivisionCount * subdivisionCount, 16); i++) {
            var (lat, lng) = Geocoding.CalculateSegmentCenterLatLng(testSubdivisionCount, i);
            Debug.Log($"Seg #{i} Center: {Geocoding.CalculateSegmentCenter(testSubdivisionCount, i)} / LL: {(lat, lng)} / LL (deg): {(lat * Mathf.Rad2Deg, lng * Mathf.Rad2Deg)}");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {

        var (userPosLat, userPosLng) = Geocoding.CalculateLatLng(userPos.position);
        
        intersectPos.position = Geocoding.CalculateUnitSpherePosition(userPosLat, userPosLng);
        intersectPos.LookAt(Vector3.zero);

        var segIndex = Geocoding.CalculateSegmentIndexFromLatLng(subdivisionCount, userPosLat, userPosLng, out var planeIntersectPos);

        intersect2Pos.position = planeIntersectPos;
        intersect2Pos.LookAt(Vector3.zero);

        var (segGroupIndex, localSegIndex) = Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCount, segIndex);

        
        var (centerLat, centerLng) = Geocoding.CalculateSegmentCenterLatLng(subdivisionCount, segIndex);

        // Neighbor (depth=1)
        var neighborFaces = Geocoding.NeighborFaceInfoList[segGroupIndex];

        //var (intersectLat, intersectLng) = CalculateLatLng(intersectPosCoords);

        var (abCoords, top) = Geocoding.SplitLocalSegmentIndexToAbt(subdivisionCount, localSegIndex);

        overlayText = $"Intersection Lat: {userPosLat * Mathf.Rad2Deg}°, Lng: {userPosLng * Mathf.Rad2Deg}°\n"
                      + $"Segment Group: {segGroupIndex} ABT: {(abCoords, top)}\n"
                      + $" * Local segment index {localSegIndex}\n"
                      + $" * Segment index {segIndex}\n"
                      + "-----------\n"
                      + $" * ABT (check): {Geocoding.SplitLocalSegmentIndexToAbt(subdivisionCount, localSegIndex)}\n"
                      + $" * Segment Group & ABT (check): {Geocoding.SplitSegIndexToSegGroupAndAbt(subdivisionCount, segIndex)}\n"
                      + "-----------\n"
                      + $"Segment Center Lat: {centerLat * Mathf.Rad2Deg}°, Lng: {centerLng * Mathf.Rad2Deg}°\n"
                      + $"Neighbor Faces: {neighborFaces[0]}, {neighborFaces[1]}, {neighborFaces[2]}";

        

        DrawDebugGizmos(segIndex);
    }
    void DrawDebugGizmos(int segIndex) {
        Gizmos.color = Color.magenta;

        for (var index = 0; index < Geocoding.Vertices.Length; index++) {
            var v = Geocoding.Vertices[index];
            Gizmos.DrawSphere(v, 0.025f);
            Handles.Label(v, $"vt{index}");
        }

        GUIStyle handleStyle = new() {
            normal = { textColor = Color.grey },
            fontSize = 20,
        };
        GUIStyle selectedHandleStyle = new() {
            normal = { textColor = Color.red },
            fontSize = 20,
        };
        
        centerPos.position = Geocoding.CalculateSegmentCenter(subdivisionCount, segIndex);

        var neighborPosListIndex = 0;
        foreach (var neighborSegIndex in Geocoding.GetNeighborsOfSegmentIndex(subdivisionCount, segIndex)) {
            neighborPosList[neighborPosListIndex].position = Geocoding.CalculateSegmentCenter(subdivisionCount, neighborSegIndex);
            neighborPosList[neighborPosListIndex].gameObject.SetActive(true);
            
            // Gizmos.color = Color.blue;
            //
            // var vertsNormalized = Geocoding.CalculateSegmentCorners(subdivisionCount, neighborSegIndex, true);
            // Gizmos.DrawLineStrip(vertsNormalized, true);
            //
            // Gizmos.color = Color.red;
            // var vertsNotNormalized = Geocoding.CalculateSegmentCorners(subdivisionCount, neighborSegIndex, false);
            // Gizmos.DrawLineStrip(vertsNotNormalized, true);
            //
            // Gizmos.DrawMesh(new() {
            //     vertices = vertsNotNormalized,
            //     normals = vertsNormalized,
            //     triangles = new[] { 0, 1, 2, 0, 2, 1 },
            // });

            
            neighborPosListIndex++;
        }

        for (var i = neighborPosListIndex; i < neighborPosList.Length; i++) {
            neighborPosList[i].gameObject.SetActive(false);
        }

        var (segGroupIndex, _) = Geocoding.SplitSegIndexToSegGroupAndLocalSegmentIndex(subdivisionCount, segIndex);

        for (var index = 0; index < Geocoding.SegmentGroupTriList.Length; index++) {
            var triList = Geocoding.SegmentGroupTriList[index];
            Gizmos.DrawLineStrip(triList, true);
            var center = triList.Aggregate(Vector3.zero, (s, v) => s + v) / triList.Length;
            var faceAngle = Vector3.Angle(SceneView.currentDrawingSceneView.camera.transform.position, center);
            if (faceAngle is >= 0 and <= 90) {
                Handles.Label(center, $"f{index}", index == segGroupIndex ? selectedHandleStyle : handleStyle);
            }
        }
        
        
        Gizmos.color = Color.white;
        Gizmos.DrawLine(Vector3.zero, userPos.position);

    }

    [ContextMenu("Generate Source Code")]
    void GenerateSourceCode() {
        File.WriteAllText("Code.c", Geocoding.GenerateSourceCode().ToString());
    }
#endif

    static Mesh CreateSubdividedTri(IReadOnlyList<Vector3> vList, int n) {
        var totalVCount = (n + 1) * (n + 2) / 2;
        var totalFCount = n * n;
        Debug.Log($"Total V count: {totalVCount}");
        Debug.Log($"Total F count: {totalFCount}");

        if (totalVCount > ushort.MaxValue) {
            // https://forum.unity.com/threads/meshes-may-not-have-more-than-65000-triangles-at-the-moment.43826/
            Debug.LogError($"Mesh cannot have more than {ushort.MaxValue} vertices.");
        }

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
        
        Debug.Log($"vIndex: {vIndex}");
        Debug.Log($"fIndex: {fIndex}");
        
        for (var index = 0; index < vertices.Length; index++) {
            vertices[index] = vertices[index].normalized;
        }

        var mesh = new Mesh { vertices = vertices };

        var triangles = edgesPerFaces.SelectMany(e => e).ToArray();

        if (triangles.Any(e => e < 0 || e >= totalVCount)) {
            Debug.LogError("Logic Error");
        }

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