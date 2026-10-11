using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThePirate.Spawning
{
    public enum SpawnZoneShape { Circle, Rectangle }

    [Serializable]
    public sealed class EnemySpawnGroup
    {
        public string name = "Nowa grupa";
        public EnemySpawnProfile profile;
        [Min(0)] public int initialCount = 1;
        [Min(1)] public int maximumAlive = 2;
        [Min(1)] public int minimumLevel = 1;
        [Min(1)] public int maximumLevel = 3;
        public bool respawn = true;
        [Min(0)] public float minimumRespawnSeconds = 90;
        [Min(0)] public float maximumRespawnSeconds = 150;
        [Tooltip("-1 = bez limitu; 0 = bez odnowień")]
        [Min(-1)] public int respawnLimit = -1;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("ThePirate/Przeciwnicy/Strefa przeciwników")]
    public sealed class EnemySpawnZone : MonoBehaviour
    {
        [Tooltip("Stały identyfikator do przyszłego zapisu; duplikuj przez edytor przeciwników.")]
        public string zoneId;
        public string displayName = "Strefa zagrożenia";
        public SpawnZoneShape shape;
        [Min(1)] public float radius = 140;
        public Vector2 rectangleSize = new Vector2(280, 280);
        public Color color = new Color(1, .3f, .15f, .7f);
        [Header("Aktywacja i ograniczenia")]
        [Min(1)] public float activationDistance = 250;
        [Min(1)] public float despawnDistance = 400;
        [Min(0)] public float minimumPlayerDistance = 100;
        [Min(1)] public int maximumAlive = 4;
        [Min(.1f)] public float spawnInterval = 3;
        public bool requireOutsideCamera = true;
        [Header("Punkty — pusta lista oznacza losowanie w obszarze")]
        public List<Transform> spawnPoints = new List<Transform>();
        [Min(.1f)] public float clearanceRadius = 8;
        [Tooltip("Tylko warstwy przeszkód, wysp i statków. Nie zaznaczaj wody.")]
        public LayerMask obstacleMask;
        [Header("Wykluczenia ręczne — np. okolice portów")]
        public List<Transform> exclusionCenters = new List<Transform>();
        [Min(1)] public float exclusionRadius = 60;
        public List<EnemySpawnGroup> groups = new List<EnemySpawnGroup>();

        // XZ world metres, yaw from Transform; scale deliberately ignored.
        public bool Contains(Vector3 world)
        {
            Vector3 local = Quaternion.Inverse(Quaternion.Euler(0, transform.eulerAngles.y, 0)) * (world - transform.position);
            return shape == SpawnZoneShape.Circle
                ? local.x * local.x + local.z * local.z <= radius * radius
                : Mathf.Abs(local.x) <= rectangleSize.x * .5f && Mathf.Abs(local.z) <= rectangleSize.y * .5f;
        }

        public Vector3 SamplePoint(float u, float v)
        {
            u = Mathf.Clamp01(u); v = Mathf.Clamp01(v);
            Vector3 local;
            if (shape == SpawnZoneShape.Circle)
            {
                float angle = v * Mathf.PI * 2, distance = Mathf.Sqrt(u) * radius;
                local = new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
            }
            else local = new Vector3((u - .5f) * rectangleSize.x, 0, (v - .5f) * rectangleSize.y);
            return transform.position + Quaternion.Euler(0, transform.eulerAngles.y, 0) * local;
        }

        private void Reset() { zoneId = Guid.NewGuid().ToString("N"); }
        private void Start() { EnemySpawnDirector.Ensure(gameObject.scene); }
        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Vector3 previous = SamplePoint(1, 0);
            if (shape == SpawnZoneShape.Circle)
                for (int i = 1; i <= 64; i++)
                {
                    Vector3 next = SamplePoint(1, i / 64f);
                    Gizmos.DrawLine(previous, next); previous = next;
                }
            else
            {
                Matrix4x4 old = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, transform.eulerAngles.y, 0), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(rectangleSize.x, 0, rectangleSize.y));
                Gizmos.matrix = old;
            }
            foreach (var point in spawnPoints)
                if (point != null) { Gizmos.DrawWireSphere(point.position, clearanceRadius); Gizmos.DrawRay(point.position, point.forward * 10); }
            Gizmos.color = Color.cyan;
            foreach (var center in exclusionCenters)
                if (center != null) Gizmos.DrawWireSphere(center.position, exclusionRadius);
        }
    }
}

