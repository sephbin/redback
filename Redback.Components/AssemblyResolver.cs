using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Redback.Components
{
    // Fixes missing .NET 7 shared-framework assemblies when Redback loads inside Rhino Inside
    // Revit. The host process (Revit 2024, .NET Framework 4.8) bootstraps the CoreCLR via RiR,
    // but the resulting runtime probe paths don't always include the .NET 7 shared-framework
    // directories. The [ModuleInitializer] fires before any type initialiser (including
    // RedbackGHBase's static ConcurrentDictionary field), so the resolver is always registered
    // in time.
    static class AssemblyResolver
    {
        // Microsoft .NET public key tokens we're willing to redirect:
        //   b03f5f7f11d50a3a — most System.* and Microsoft.Win32.* assemblies
        //   cc7b13ffcd2ddd51 — System.Drawing.Common and related
        static readonly byte[][] s_knownPKTs =
        {
            new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a },
            new byte[] { 0xcc, 0x7b, 0x13, 0xff, 0xcd, 0x2d, 0xdd, 0x51 },
        };

        [ModuleInitializer]
        internal static void Register()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (!IsMicrosoftNetAssembly(name)) return null;

            // First choice: return a compatible assembly that's already loaded in the AppDomain.
            // This handles the common RiR case where the host runs .NET 8 and already has
            // Version=8.0.0.0 of the same facade loaded; returning it is safe because these
            // are thin type-forwarding shims that all bottom out in System.Private.CoreLib.
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(loaded.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase))
                    return loaded;
            }

            // Second choice: load from the .NET shared framework on disk.
            // This handles the case where the host probe paths are broken (seen in RiR and
            // some Rhino 8.3x configurations). Search all installed runtimes, preferring the
            // highest version so that .NET 8 hosts find 8.x before falling back to 7.x.
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            foreach (var sharedRoot in new[]
            {
                Path.Combine(pf, "dotnet", "shared", "Microsoft.NETCore.App"),
                Path.Combine(pf, "dotnet", "shared", "Microsoft.WindowsDesktop.App"),
            })
            {
                if (!Directory.Exists(sharedRoot)) continue;
                var versionDirs = Directory.GetDirectories(sharedRoot);
                Array.Sort(versionDirs, StringComparer.OrdinalIgnoreCase);
                Array.Reverse(versionDirs); // highest version first
                foreach (var versionDir in versionDirs)
                {
                    var candidate = Path.Combine(versionDir, name.Name + ".dll");
                    if (File.Exists(candidate))
                        return Assembly.LoadFrom(candidate);
                }
            }

            return null;
        }

        static bool IsMicrosoftNetAssembly(AssemblyName name)
        {
            var pkt = name.GetPublicKeyToken();
            if (pkt == null || pkt.Length != 8) return false;
            foreach (var known in s_knownPKTs)
            {
                bool match = true;
                for (int i = 0; i < 8; i++)
                    if (pkt[i] != known[i]) { match = false; break; }
                if (match) return true;
            }
            return false;
        }
    }
}
