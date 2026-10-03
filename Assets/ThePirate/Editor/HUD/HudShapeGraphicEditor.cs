using ThePirate.UI.HUD;
using UnityEditor;
using UnityEditor.UI;

namespace ThePirate.Editor.HUD
{
    [CustomEditor(typeof(HudShapeGraphic)), CanEditMultipleObjects]
    public sealed class HudShapeGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            foreach(var name in new [] { "shape", "thickness", "fill", "segments", "closed", "points" })
                EditorGUILayout.PropertyField(serializedObject.FindProperty(name),true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
