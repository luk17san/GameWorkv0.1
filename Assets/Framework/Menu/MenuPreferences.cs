using UnityEngine;

namespace Framework.Menu
{
    public static class MenuPreferences
    {
        private const string VolumeKey = "GameWork.Audio.MasterVolume";
        private const string MuteKey = "GameWork.Audio.Muted";
        private const string QualityKey = "GameWork.Graphics.Quality";

        public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        public static bool Muted => PlayerPrefs.GetInt(MuteKey, 0) != 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySaved()
        {
            ApplyAudio();
            if (PlayerPrefs.HasKey(QualityKey) && QualitySettings.names.Length > 0)
                QualitySettings.SetQualityLevel(Mathf.Clamp(PlayerPrefs.GetInt(QualityKey),
                    0, QualitySettings.names.Length - 1), true);
        }

        public static void SetVolume(float volume)
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(volume));
            ApplyAudio();
        }

        public static void ToggleMute()
        {
            PlayerPrefs.SetInt(MuteKey, Muted ? 0 : 1);
            ApplyAudio();
            PlayerPrefs.Save();
        }

        public static void SetQuality(int index)
        {
            if (QualitySettings.names.Length == 0) return;
            index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt(QualityKey, index);
            PlayerPrefs.Save();
        }

        private static void ApplyAudio() => AudioListener.volume = Muted ? 0f : Volume;
        public static void Flush() => PlayerPrefs.Save();
    }
}
