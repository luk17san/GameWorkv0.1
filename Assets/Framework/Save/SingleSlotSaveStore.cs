using System;
using System.IO;
using UnityEngine;

namespace Framework.Save
{
    // Jeden plik na rozgrywkę. Format danych i jego walidacja należą do gry.
    public static class SingleSlotSaveStore
    {
        public const string FileName = "gamework-save-v1.json";
        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasAnyFile => File.Exists(SavePath) || File.Exists(SavePath + ".bak");

        public static bool TryRead<T>(Func<T, bool> valid, out T data, out string error) where T : class
        {
            data = null;
            if (TryReadFile(SavePath, valid, out data) || TryReadFile(SavePath + ".bak", valid, out data))
            {
                error = null;
                return true;
            }
            error = HasAnyFile ? "Zapis jest uszkodzony lub niezgodny z tą wersją gry."
                : "Nie ma jeszcze zapisanej gry.";
            return false;
        }

        public static bool TryWrite<T>(T data, Func<T, bool> valid, out string error) where T : class
        {
            error = null;
            if (data == null || valid == null || !valid(data))
            {
                error = "Nie można zapisać niepoprawnego stanu gry.";
                return false;
            }

            string path = SavePath;
            string temporary = path + ".tmp";
            string backup = path + ".bak";
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (!TryReadFile(temporary, valid, out T checkedData))
                    throw new InvalidDataException("Nowy plik zapisu nie przeszedł walidacji.");

                if (File.Exists(path))
                {
                    // Nie pozwól, żeby uszkodzony plik główny zastąpił poprawną kopię.
                    if (!TryReadFile(path, valid, out T existingData))
                    {
                        if (TryReadFile(backup, valid, out T backupData)) File.Copy(backup, path, true);
                        else File.Delete(path);
                    }
                }
                if (File.Exists(path)) File.Replace(temporary, path, backup);
                else File.Move(temporary, path);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
                || exception is NotSupportedException || exception is ArgumentException)
            {
                error = "Nie udało się zapisać gry. Sprawdź dostępne miejsce i uprawnienia do plików.";
                Debug.LogException(exception);
                return false;
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { /* Pozostałość .tmp nie jest używana przy wczytywaniu. */ }
                catch (UnauthorizedAccessException) { }
            }
        }

        private static bool TryReadFile<T>(string path, Func<T, bool> valid, out T data) where T : class
        {
            data = null;
            try
            {
                if (!File.Exists(path)) return false;
                if (new FileInfo(path).Length > 1024 * 1024) return false;
                T candidate = JsonUtility.FromJson<T>(File.ReadAllText(path));
                if (candidate == null || valid == null || !valid(candidate)) return false;
                data = candidate;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
