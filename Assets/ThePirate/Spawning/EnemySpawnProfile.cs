using UnityEngine;

namespace ThePirate.Spawning
{
    [CreateAssetMenu(menuName = "ThePirate/Przeciwnicy/Profil przeciwnika")]
    public sealed class EnemySpawnProfile : ScriptableObject
    {
        [Header("Statek — komponenty edytuj na prefabie")]
        public GameObject prefab;
        [Min(2)] public int team = 2;
        [Header("Mnożniki statystyk względem prefabu; oś X = poziom")]
        public AnimationCurve hullMultiplier = AnimationCurve.Linear(1, 1, 10, 2);
        public AnimationCurve damageMultiplier = AnimationCurve.Linear(1, 1, 10, 1.8f);
        public AnimationCurve reloadMultiplier = AnimationCurve.Linear(1, 1, 10, .8f);
        public AnimationCurve speedMultiplier = AnimationCurve.Linear(1, 1, 10, 1);
        [Header("AI — opcjonalne nadpisanie prefabu")]
        public bool overrideAI;
        [Min(1)] public float detectionRange = 80;
        [Min(1)] public float loseTargetRange = 110;
        [Min(1)] public float territoryRadius = 140;
        [Min(1)] public float approachDistance = 30;
        [Min(1)] public float minimumDistance = 18;
        [Min(.1f)] public float cruiseSpeed = 8;
        [Min(.1f)] public float maneuverSpeed = 4.5f;
        public bool enableWeapons = true;
    }
}

