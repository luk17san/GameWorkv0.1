using ThePirate.Combat;
using UnityEngine;

namespace ThePirate.Spawning
{
    [DisallowMultipleComponent]
    public sealed class SpawnedEnemy : MonoBehaviour
    {
        public string InstanceId { get; private set; }
        public string ZoneId { get; private set; }
        public int GroupIndex { get; private set; }
        public int SlotIndex { get; private set; }
        public int Level { get; private set; }
        public Vector3 Home { get; private set; }
        public bool Retired { get; private set; }
        private EnemySpawnDirector owner;
        private Framework.Health.Health hull;
        public void Bind(EnemySpawnDirector director)
        {
            owner = director; hull = GetComponent<Framework.Health.Health>(); hull.Depleted += Defeated;
        }
        private void Defeated() { if (owner != null && !Retired) owner.NotifyDefeated(this); }
        private void OnDestroy() { if (hull != null) hull.Depleted -= Defeated; }
        public void Initialize(string id, string zone, int group, int slot, int level, Vector3 home)
        { InstanceId = id; ZoneId = zone; GroupIndex = group; SlotIndex = slot; Level = level; Home = home; }
        public void RetireWithoutReward()
        {
            Retired = true;
            // No health depletion: reward handlers are never invoked.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
