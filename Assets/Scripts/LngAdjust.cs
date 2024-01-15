using UnityEngine;

[ExecuteAlways]
public class LngAdjust : MonoBehaviour
{
    const float V = 90.2f;
    [Range(V - 0.5f, V + 0.5f)]
    public float longitude = V;

    void Update() {
        transform.localRotation = Quaternion.Euler(0, -longitude, 0);
    }
}
