using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ThePirate.UI.HUD
{
    public sealed class HudMinimapView : MonoBehaviour
    {
        public RawImage terrain;
        public RectTransform markerRoot;
        public HudShapeGraphic markerTemplate;
        [Min(10)] public float worldRadius = 120;
        [Min(10)] public float cameraHeight = 250;
        [Range(128,1024)] public int textureSize = 256;
        public LayerMask terrainLayers = ~(1 << 5);
        public Color playerColor = new Color(1f,0.91f,0.73f);
        public Color enemyColor = new Color(0.95f,0.23f,0.23f);
        public Color friendlyColor = new Color(0.2f,0.8f,0.7f);
        private readonly List<HudShapeGraphic> markers = new List<HudShapeGraphic>();
        private Camera mapCamera;
        private RenderTexture texture;
        private Transform player;

        public void SetPlayer(Transform value) => player = value;

        private void LateUpdate()
        {
            if (!Application.isPlaying || player == null || terrain == null) return;
            if (mapCamera == null)
            {
                var go = new GameObject("HUD Minimap Camera");
                SceneManager.MoveGameObjectToScene(go,gameObject.scene);
                mapCamera = go.AddComponent<Camera>();
                mapCamera.orthographic = true; mapCamera.clearFlags = CameraClearFlags.SolidColor;
                mapCamera.backgroundColor = new Color(0.025f,0.09f,0.12f);
                mapCamera.nearClipPlane = 0.1f; mapCamera.allowHDR = false; mapCamera.allowMSAA = false;
                mapCamera.cullingMask = terrainLayers.value & ~(1 << 5);
                texture = new RenderTexture(textureSize,textureSize,24) { name = "HUD Minimap (runtime)" };
                texture.Create(); mapCamera.targetTexture = texture; terrain.texture = texture; terrain.enabled = true;
            }
            mapCamera.orthographicSize = Mathf.Max(10,worldRadius);
            mapCamera.farClipPlane = cameraHeight*2;
            mapCamera.transform.SetPositionAndRotation(player.position+Vector3.up*cameraHeight,Quaternion.Euler(90,0,0));
        }

        public void Render(IReadOnlyList<HudMapMarker> data)
        {
            if(markerRoot == null || markerTemplate == null) return;
            Vector3 center = player != null ? player.position : Vector3.zero;
            foreach(var item in data) if(item.Kind == HudMarkerKind.Player) { center=item.Position; break; }
            int shown=0;
            float radius=Mathf.Min(markerRoot.rect.width,markerRoot.rect.height)*0.5f-10;
            foreach(var item in data)
            {
                Vector2 delta=new Vector2(item.Position.x-center.x,item.Position.z-center.z)/Mathf.Max(10,worldRadius);
                if(delta.sqrMagnitude>1) continue;
                if(shown==markers.Count)
                {
                    var marker=Instantiate(markerTemplate,markerRoot);
                    marker.name="Marker_"+shown;
                    markers.Add(marker);
                }
                var graphic=markers[shown++]; graphic.gameObject.SetActive(true);
                graphic.rectTransform.anchoredPosition=delta*radius;
                graphic.rectTransform.localRotation=Quaternion.Euler(0,0,-item.Heading);
                graphic.color=item.Kind==HudMarkerKind.Player ? playerColor : item.Kind==HudMarkerKind.Enemy ? enemyColor
                    : item.Kind==HudMarkerKind.Friendly ? friendlyColor : Color.gray;
            }
            for(int i=shown;i<markers.Count;i++) markers[i].gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if(terrain != null) { terrain.texture=null; terrain.enabled=false; }
            if(mapCamera != null) { mapCamera.targetTexture=null; Destroy(mapCamera.gameObject); }
            if(texture != null) { texture.Release(); Destroy(texture); }
            mapCamera=null; texture=null;
        }
    }
}
