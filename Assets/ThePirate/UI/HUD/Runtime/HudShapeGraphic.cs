using UnityEngine;
using UnityEngine.UI;

namespace ThePirate.UI.HUD
{
    // Geometria uGUI: edytowalne linie i punkty zamiast spłaszczonego obrazu HUD.
    [AddComponentMenu("GameWork/HUD/Shape Graphic")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HudShapeGraphic : MaskableGraphic
    {
        public enum ShapeKind { Rectangle, Border, Disc, Ring, Polyline, Polygon }
        public ShapeKind shape;
        [Min(0.1f)] public float thickness = 1.5f;
        [Range(0, 1)] public float fill = 1f;
        [Range(12, 128)] public int segments = 96;
        public bool closed;
        [Tooltip("Punkty w układzie 0..1 prostokąta. Polygon wymaga wypukłego kształtu.")]
        public Vector2[] points = new Vector2[0];

        public void SetFill(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(fill, value)) return;
            fill = value; SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = GetPixelAdjustedRect();
            if (shape == ShapeKind.Rectangle)
            { Quad(vh, new Vector2(r.xMin,r.yMin), new Vector2(r.xMin,r.yMax), new Vector2(r.xMin+r.width*fill,r.yMax), new Vector2(r.xMin+r.width*fill,r.yMin)); return; }
            if (shape == ShapeKind.Border)
            {
                var a = new Vector2(r.xMin+thickness/2,r.yMin+thickness/2);
                var b = new Vector2(r.xMin+thickness/2,r.yMax-thickness/2);
                var c = new Vector2(r.xMax-thickness/2,r.yMax-thickness/2);
                var d = new Vector2(r.xMax-thickness/2,r.yMin+thickness/2);
                Line(vh,a,b); Line(vh,b,c); Line(vh,c,d); Line(vh,d,a); return;
            }
            if (shape == ShapeKind.Disc || shape == ShapeKind.Ring)
            {
                float radius = Mathf.Min(r.width,r.height)*0.5f;
                int steps = Mathf.Max(1,Mathf.CeilToInt(segments*fill));
                for (int i=0;i<steps;i++)
                {
                    float a = (90f-360f*fill*i/steps)*Mathf.Deg2Rad;
                    float b = (90f-360f*fill*(i+1)/steps)*Mathf.Deg2Rad;
                    Vector2 va = new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                    Vector2 vb = new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                    float inner = shape == ShapeKind.Disc ? 0 : Mathf.Max(0,radius-thickness);
                    Quad(vh,r.center+va*inner,r.center+va*radius,r.center+vb*radius,r.center+vb*inner);
                }
                return;
            }
            if (points == null || points.Length < 2) return;
            Vector2 Map(Vector2 p) => new Vector2(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
            if (shape == ShapeKind.Polygon)
            {
                for(int i=1;i<points.Length-1;i++)
                {
                    int start = vh.currentVertCount;
                    vh.AddVert(Map(points[0]),color,Vector2.zero);
                    vh.AddVert(Map(points[i]),color,Vector2.zero);
                    vh.AddVert(Map(points[i+1]),color,Vector2.zero);
                    vh.AddTriangle(start,start+1,start+2);
                }
            }
            else
            {
                for(int i=1;i<points.Length;i++) Line(vh,Map(points[i-1]),Map(points[i]));
                if(closed) Line(vh,Map(points[points.Length-1]),Map(points[0]));
            }
        }

        private void Line(VertexHelper vh,Vector2 a,Vector2 b)
        {
            Vector2 delta=b-a;
            if(delta.sqrMagnitude<0.0001f) return;
            Vector2 normal=new Vector2(-delta.y,delta.x).normalized*thickness*0.5f;
            Quad(vh,a-normal,a+normal,b+normal,b-normal);
        }
        private void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            int start=vh.currentVertCount;
            vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero);
            vh.AddVert(c,color,Vector2.zero); vh.AddVert(d,color,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }
    }
}
