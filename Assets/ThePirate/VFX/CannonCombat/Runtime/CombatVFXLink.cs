using GameWork.VFX;
using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.VFX
{
    [DisallowMultipleComponent]
    public sealed class CombatVFXLink : MonoBehaviour
    {
        private CombatShip ship;
        private ShipDurabilityVFX durability;
        private CannonCombatVFX effects;
        private bool sinking;
        private float smokeTimer;
        private Transform port, starboard;
        private static CannonCombatVFX impacts;

        public static CombatVFXLink Ensure(CombatShip target) =>
            target.GetComponent<CombatVFXLink>() ?? target.gameObject.AddComponent<CombatVFXLink>();

        private void Awake()
        {
            ship = GetComponent<CombatShip>();
            var anchor = new GameObject("CombatVFX");
            anchor.transform.SetParent(transform, false);
            anchor.transform.localPosition = Vector3.up;
            effects = anchor.AddComponent<CannonCombatVFX>();
            durability = anchor.AddComponent<ShipDurabilityVFX>();
            port = Waterline("PortFoam", -1f);
            starboard = Waterline("StarboardFoam", 1f);
            anchor.AddComponent<ShipWaterFoamVFX>().Configure(GetComponent<Rigidbody>(), port, starboard);
        }
        private Transform Waterline(string name, float side)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            float width = 1f;
            foreach (var box in GetComponentsInChildren<BoxCollider>())
                if (box.GetComponentInParent<CombatShip>() == ship)
                    width = Mathf.Max(width, Mathf.Abs(box.center.x) + box.size.x * 0.5f);
            go.transform.localPosition = new Vector3(side * width, 0f, 0f);
            return go.transform;
        }
        private void OnEnable()
        {
            ship.Hull.Changed += Changed;
            Changed(ship.Hull.CurrentHealth, ship.Hull.MaxHealth);
        }
        private void OnDisable() { if (ship != null && ship.Hull != null) ship.Hull.Changed -= Changed; }
        private void Changed(int current, int maximum) => durability.SetDurability01(current / (float)Mathf.Max(1, maximum));
        public void Repair(float fraction) => effects.PlayRepair(transform.position + transform.right + Vector3.up, transform.right, fraction);
        public void Sinking() { sinking = true; smokeTimer = 0f; }
        private void Update()
        {
            var p = port.position; p.y = 0.65f; port.position = p;
            p = starboard.position; p.y = 0.65f; starboard.position = p;
            if (!sinking || Time.deltaTime <= 0f) return;
            smokeTimer -= Time.deltaTime;
            if (smokeTimer > 0f) return;
            smokeTimer = 0.32f;
            VFXBurst.Sprite("Sinking_Smoke", transform.position + Vector3.up, Vector3.up,
                Resources.Load<Texture2D>("GameWork/CannonVFX/CannonSmokePuff"), new Color(0.28f, 0.27f, 0.25f, 0.58f), 0.8f, 1.1f, 1, Vector3.up * 0.18f);
            VFXBurst.Particles("Sinking_Bubbles", new Vector3(transform.position.x, 0.65f, transform.position.z), Vector3.up,
                new Color(0.68f, 0.88f, 0.95f, 0.65f), 6, 0.09f, 1.25f, 0.15f, 0.65f);
        }
        public static void Shot(Cannon cannon)
        {
            var flash = cannon.GetComponent<CannonMuzzleFlashVFX>() ?? cannon.gameObject.AddComponent<CannonMuzzleFlashVFX>();
            flash.Configure(cannon.Muzzle); flash.Play();
            var recoil = cannon.GetComponent<CannonBarrelRecoilVFX>();
            if (recoil != null) recoil.Play();
            // Each actual shot contributes smoke, including staggered batteries.
            Manager().PlayLingeringBatterySmoke(cannon.Muzzle);
        }
        public static void Trail(Transform projectile)
        {
            if (projectile.GetComponentInChildren<CannonballTrailVFX>() != null) return;
            var go = new GameObject("CannonTrail");
            go.transform.SetParent(projectile, false);
            go.transform.localScale = new Vector3(1f / Mathf.Max(0.001f, Mathf.Abs(projectile.lossyScale.x)),
                1f / Mathf.Max(0.001f, Mathf.Abs(projectile.lossyScale.y)), 1f / Mathf.Max(0.001f, Mathf.Abs(projectile.lossyScale.z)));
            go.AddComponent<CannonballTrailVFX>();
        }
        private static CannonCombatVFX Manager()
        {
            if (impacts == null) impacts = new GameObject("CombatImpactVFX").AddComponent<CannonCombatVFX>();
            return impacts;
        }
        public static void Impact(Collider collider, Vector3 point, Vector3 normal, bool special)
        {
            var surface = collider == null ? CannonImpactSurface.Water : collider.GetComponentInParent<CombatShip>() != null
                ? CannonImpactSurface.Wood : CannonImpactSurface.Ground;
            if (surface == CannonImpactSurface.Water) point.y += 0.65f;
            if (special) Manager().PlaySpecialImpact(point, normal, surface);
            else if (surface == CannonImpactSurface.Water) Manager().PlayWaterImpact(point, normal);
            else if (surface == CannonImpactSurface.Wood) Manager().PlayHullImpact(point, normal);
            else Manager().PlayGroundImpact(point, normal);
        }
    }
}
