using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ThePirate.UI.HUD
{
    [DefaultExecutionOrder(100)]
    public sealed class HudMinimapView : MonoBehaviour
    {
        public RawImage terrain;
        public RectTransform markerRoot;
        public HudShapeGraphic markerTemplate;
        [Header("Zasięg minimapy")]
        public bool matchGameplayCamera = true;
        [Tooltip("Kamera gry. Puste pole używa Main Camera w tej samej scenie.")]
        public Camera gameplayCamera;
        [Tooltip("Stosunek powierzchni koła minimapy do obszaru ekranu na poziomie wody. 4 = czterokrotnie większy obszar. Zasięg reaguje na zoom kamery.")]
        [Min(.01f)] public float visibleAreaMultiplier = 4f;
        public float waterHeight = 0f;
        [Tooltip("Stały promień w metrach, gdy dopasowanie jest wyłączone lub kamera nie widzi całej płaszczyzny wody.")]
        [Min(1)] public float worldRadius = 120;
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
        private readonly Vector2[] footprint = new Vector2[4];
        public float CurrentWorldRadius { get; private set; } = 120f;

        public bool TryGetVisibleWorldArea(out float area)
        {
            area = 0;
            var camera = gameplayCamera != null ? gameplayCamera : Camera.main;
            if (camera == null || camera == mapCamera || camera.gameObject.scene != gameObject.scene) return false;
            var plane = new Plane(Vector3.up, new Vector3(0, waterHeight, 0));
            for (int i = 0; i < 4; i++)
            {
                Vector2 viewport = i == 0 ? Vector2.zero : i == 1 ? Vector2.right : i == 2 ? Vector2.one : Vector2.up;
                var ray = camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0));
                if (!plane.Raycast(ray, out float distance) || distance > camera.farClipPlane) return false;
                var hit = ray.GetPoint(distance); footprint[i] = new Vector2(hit.x, hit.z);
            }
            double twiceArea = 0;
            for (int i = 0; i < 4; i++)
            {
                var a = footprint[i]; var b = footprint[(i + 1) % 4];
                twiceArea += (double)a.x * b.y - (double)b.x * a.y;
            }
            area = (float)(System.Math.Abs(twiceArea) * .5);
            return area > .001f && !float.IsInfinity(area) && !float.IsNaN(area);
        }

        private float UpdateRadius()
        {
            CurrentWorldRadius = matchGameplayCamera && TryGetVisibleWorldArea(out float area)
                ? Mathf.Sqrt(area * Mathf.Max(.01f, visibleAreaMultiplier) / Mathf.PI)
                : Mathf.Max(1, worldRadius);
            return CurrentWorldRadius;
        }

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
            mapCamera.aspect = 1f;
            mapCamera.orthographicSize = UpdateRadius();
            mapCamera.farClipPlane = cameraHeight*2;
            mapCamera.transform.SetPositionAndRotation(player.position+Vector3.up*cameraHeight,Quaternion.Euler(90,0,0));
        }

        public void Render(IReadOnlyList<HudMapMarker> data)
        {
            if(markerRoot == null || markerTemplate == null) return;
            Vector3 center = player != null ? player.position : Vector3.zero;
            foreach(var item in data) if(item.Kind == HudMarkerKind.Player) { center=item.Position; break; }
            int shown=0;
            float radius=Mathf.Min(markerRoot.rect.width,markerRoot.rect.height)*0.5f;
            float worldRange=UpdateRadius();
            foreach(var item in data)
            {
                Vector2 delta=new Vector2(item.Position.x-center.x,item.Position.z-center.z)/worldRange;
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
