using System;

namespace ThePirate.AI
{
    public static class EnemyNavigationMath
    {
        public static int ChooseSide(float bearing) => bearing >= 0f ? 1 : -1;

        public static bool ShouldGainDistance(bool escaping, float distance, float minimum, float margin) =>
            distance < minimum || (escaping && distance < minimum + margin);

        public static float RadialCorrection(float distance, float desired, float turnInDistance) =>
            Math.Max(-1f, Math.Min(1f, (distance - desired) / Math.Max(1f, turnInDistance)));

        public static float ManeuverSpeed(float cruise, float headingError) =>
            Math.Max(0f, cruise) * Math.Max(0f, 1f - Math.Abs(headingError) / 90f);

        // Unit direction toward target in the horizontal plane; +1 keeps it to starboard.
        public static void BroadsideDirection(float towardX, float towardZ, int side, float radial,
            bool escaping, out float x, out float z)
        {
            float tangentWeight = escaping ? 0.35f : 1f;
            float radialWeight = escaping ? -1f : radial;
            x = -towardZ * side * tangentWeight + towardX * radialWeight;
            z = towardX * side * tangentWeight + towardZ * radialWeight;
        }

        public static bool ShouldHold(bool holding, float distance, float desired, float margin) =>
            distance <= desired || (holding && distance <= desired + margin);

        public static float ApproachSpeed(float clearance, float braking, float cruise, float angle)
        {
            float safeSpeed = (float)Math.Sqrt(2f * Math.Max(0f, braking) * Math.Max(0f, clearance));
            float alignment = Math.Max(0f, Math.Min(1f, 1f - Math.Abs(angle) / 90f));
            return Math.Min(Math.Max(0f, cruise), safeSpeed * 0.8f) * alignment;
        }
    }
}
