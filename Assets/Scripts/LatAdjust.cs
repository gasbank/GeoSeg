using UnityEngine;

[ExecuteAlways]
public class LatAdjust : MonoBehaviour {
    const float V = 32.2f;
    [Range(V - 0.5f, V + 0.5f)]
    public float latitude = V;

    void Update() {
        transform.localRotation = Quaternion.Euler(0, 0, latitude);
    }
}
