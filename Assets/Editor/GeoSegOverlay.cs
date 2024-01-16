using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), OverlayId, "GeoSeg", true)]
[UsedImplicitly]
public class GeoSegOverlay : Overlay {
    const string OverlayId = "GeoSegToolbar";
    
    Label label;
    public override VisualElement CreatePanelContent() {
        var root = new VisualElement { name = "GeoSeg" };
        label = new() { text = Sphere.overlayText };
        root.Add(label);
        return root;
    }

    public override void OnCreated() {
        base.OnCreated();

        SceneView.duringSceneGui += view => {
            label.text = Sphere.overlayText;
            //Debug.Log("????");
        };
    }
}