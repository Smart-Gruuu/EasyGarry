using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EasyGarry
{
    /// <summary>
    /// Makes EasyGarry.exe self-contained. Every referenced DLL is embedded in the
    /// executable as a resource named "EasyGarry.Lib.&lt;simple name&gt;.dll"; this hook hands
    /// them to the CLR on demand, so the exe runs with nothing beside it.
    ///
    /// Matching is by simple name only, ignoring version and public key. That is
    /// deliberate: the packages disagree on versions of shared assemblies (System.Memory,
    /// System.Runtime.CompilerServices.Unsafe and friends), which App.config normally
    /// reconciles with binding redirects. A lone .exe has no .config, so resolving by
    /// simple name performs the same job - an assembly returned from AssemblyResolve is
    /// accepted without the usual version check.
    /// </summary>
    internal static class EmbeddedAssemblies
    {
        private const string ResourcePrefix = "EasyGarry.Lib.";

        private static readonly Dictionary<string, Assembly> Loaded =
            new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private static readonly object Gate = new object();

        private static bool _installed;

        /// <summary>
        /// Must be called before anything touches a type from an embedded assembly.
        /// </summary>
        public static void Install()
        {
            lock (Gate)
            {
                if (_installed) return;
                AppDomain.CurrentDomain.AssemblyResolve += Resolve;
                _installed = true;
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string simpleName;
            try
            {
                simpleName = new AssemblyName(args.Name).Name;
            }
            catch
            {
                return null;
            }

            // Satellite/resource probes have no embedded counterpart. Returning null lets
            // the CLR fall back to the neutral resources instead of probing repeatedly.
            if (simpleName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                return null;

            lock (Gate)
            {
                Assembly cached;
                if (Loaded.TryGetValue(simpleName, out cached))
                    return cached;

                byte[] raw = ReadResource(ResourcePrefix + simpleName + ".dll");
                if (raw == null)
                {
                    // Cache the miss so a repeated probe does not re-read the manifest.
                    Loaded[simpleName] = null;
                    return null;
                }

                Assembly loadedAssembly;
                try
                {
                    loadedAssembly = Assembly.Load(raw);
                }
                catch
                {
                    loadedAssembly = null;
                }

                Loaded[simpleName] = loadedAssembly;
                return loadedAssembly;
            }
        }

        private static byte[] ReadResource(string resourceName)
        {
            Assembly self = typeof(EmbeddedAssemblies).Assembly;
            using (Stream stream = self.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                var buffer = new byte[stream.Length];
                int offset = 0;
                while (offset < buffer.Length)
                {
                    int read = stream.Read(buffer, offset, buffer.Length - offset);
                    if (read <= 0) break;
                    offset += read;
                }
                return buffer;
            }
        }

        /// <summary>
        /// Opens an embedded image ("EasyGarry.Img.&lt;file&gt;.png"), or null when absent.
        /// </summary>
        public static Stream OpenImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return typeof(EmbeddedAssemblies).Assembly
                .GetManifestResourceStream("EasyGarry.Img." + fileName);
        }
    }
}
