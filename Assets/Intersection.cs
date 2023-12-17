using UnityEngine;

public static class Intersection
{
    const double Epsilon = 0.000001d;

    public static Vector3? GetTimeAndUvCoord(Vector3 rayOrigin, Vector3 rayDirection, Vector3 vert0, Vector3 vert1, Vector3 vert2)
    {
        var edge1 = vert1 - vert0;
        var edge2 = vert2 - vert0;

        var pvec = Cross(rayDirection, edge2);

        var det = Dot(edge1, pvec);

        if (det > -Epsilon && det < Epsilon)
        {
            return null;
        }

        var invDet = 1d / det;

        var tvec = rayOrigin - vert0;

        var u = Dot(tvec, pvec) * invDet;

        if (u < 0 || u > 1)
        {
            return null;
        }

        var qvec = Cross(tvec, edge1);

        var v = Dot(rayDirection, qvec) * invDet;

        if (v < 0 || u + v > 1)
        {
            return null;
        }

        var t = Dot(edge2, qvec) * invDet;
        
        // ray 반대 방향으로 만나거나, 최대 길이를 지나쳐서 만나거나 하는 건 안만나는 걸로 친다.
        if (t is < 0 or > 1) {
            return null;
        }

        return new Vector3((float)t, (float)u, (float)v);
    }

    static double Dot(Vector3 v1, Vector3 v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }

    static Vector3 Cross(Vector3 v1, Vector3 v2)
    {
        Vector3 dest;
        
        dest.x = v1.y * v2.z - v1.z * v2.y;
        dest.y = v1.z * v2.x - v1.x * v2.z;
        dest.z = v1.x * v2.y - v1.y * v2.x;

        return dest;
    }

    public static Vector3 GetTrilinearCoordinateOfTheHit(float t, Vector3 rayOrigin, Vector3 rayDirection)
    {
        return rayDirection * t + rayOrigin;
    }
}
