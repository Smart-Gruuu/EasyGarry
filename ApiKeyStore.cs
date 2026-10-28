using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasyGarry
{
    /// <summary>
    /// The two API keys the app needs to run.
    /// </summary>
    internal sealed class ApiKeys
    {
        public string OpenAi { get; set; }
        public string Deepgram { get; set; }

        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(OpenAi) && !string.IsNullOrWhiteSpace(Deepgram);
    }

    /// <summary>
    /// Persists the user's own API keys under %APPDATA%\EasyGarry, encrypted with DPAPI
    /// under the current Windows account. No key is compiled into the app, so a copy of
    /// the binary is useless without the operator supplying their own keys on first run.
    ///
    /// Scope note: DPAPI CurrentUser ties the ciphertext to this Windows user on this
    /// machine - copying apikeys.dat elsewhere yields nothing. It is not a vault: anything
    /// already running as this user could ask DPAPI to decrypt it too.
    /// </summary>
    internal static class ApiKeyStore
    {
        private const string FileName = "apikeys.dat";
        private const char FieldSeparator = '\n';

        // Extra entropy so another app's DPAPI blob can never be mistaken for ours.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("EasyGarry.ApiKeys.v1");

        public static string StorageDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyGarry");

        public static string StoragePath => Path.Combine(StorageDirectory, FileName);

        /// <summary>
        /// Returns the saved keys, or null when nothing usable is stored. A blob written by
        /// a different Windows user, or a corrupted one, is reported the same as "not set"
        /// so the app simply asks for the keys again.
        /// </summary>
        public static ApiKeys Load()
        {
            try
            {
                if (!File.Exists(StoragePath))
                    return null;

                byte[] encrypted = File.ReadAllBytes(StoragePath);
                byte[] plain = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);

                string[] fields = Encoding.UTF8.GetString(plain).Split(FieldSeparator);
                if (fields.Length < 2)
                    return null;

                var keys = new ApiKeys { OpenAi = fields[0].Trim(), Deepgram = fields[1].Trim() };
                return keys.IsComplete ? keys : null;
            }
            catch
            {
                return null;
            }
        }

        public static bool HasSavedKeys() => Load() != null;

        /// <summary>
        /// Throws on failure so the caller can tell the user the keys were not saved,
        /// rather than silently sending them back to a setup screen that looks complete.
        /// </summary>
        public static void Save(ApiKeys keys)
        {
            if (keys == null || !keys.IsComplete)
                throw new ArgumentException("Both API keys are required.", nameof(keys));

            Directory.CreateDirectory(StorageDirectory);

            string payload = keys.OpenAi.Trim() + FieldSeparator + keys.Deepgram.Trim();
            byte[] encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(payload), Entropy, DataProtectionScope.CurrentUser);

            File.WriteAllBytes(StoragePath, encrypted);
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(StoragePath))
                    File.Delete(StoragePath);
            }
            catch
            {
                // Nothing useful to do - the setup screen will overwrite it on the next save.
            }
        }
    }
}
