using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Redback.Components
{
    // Fixes missing .NET 8 shared-framework assemblies when Redback loads inside Rhino Inside
    // Revit. The host process (Revit 2024, .NET Framework 4.8) bootstraps the .NET 8 CoreCLR
    // via RiR, but the resulting runtime probe paths don't always include the .NET 8
    // shared-framework directories. The [ModuleInitializer] fires before any type initialiser
    // (including RedbackGHBase's static ConcurrentDictionary field), so the resolver is always
    // registered in time.
    //
    // NOTE: We cannot use AppDomain.CurrentDomain.AssemblyResolve here. In Revit 2024 + RiR
    // the .NET Framework 4.8 GAC provides System.Runtime Version=4.0.0.0. Because our plugin
    // targets net8.0, the GAC's v4 cannot satisfy our v8 reference — the CLR probes the plugin
    // directory instead (where we bundle System.Runtime v8). But System.AppDomain is NOT in the
    // .NET Framework 4.8 version of System.Runtime's type-forward list, so any reference to
    // AppDomain anywhere in the module causes TypeLoadException.
    // AssemblyLoadContext comes from System.Runtime.Loader.dll (also bundled as v8), which is
    // NOT in the .NET Framework 4.8 GAC, so there is no version collision.
    static class AssemblyResolver
    {
        // Microsoft .NET public key tokens we're willing to redirect:
        //   b03f5f7f11d50a3a — most System.* and Microsoft.Win32.* assemblies
        //   cc7b13ffcd2ddd51 — System.Drawing.Common and related
        //   7cec85d7bea7798e — System.Private.CoreLib
        //     Safety net for any third-party dependency compiled against an older TFM that
        //     carries a direct reference to a different version of System.Private.CoreLib.
        //     We redirect by name to the already-loaded copy.
        static readonly byte[][] s_knownPKTs =
        {
            new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a },
            new byte[] { 0xcc, 0x7b, 0x13, 0xff, 0xcd, 0x2d, 0xdd, 0x51 },
            new byte[] { 0x7c, 0xec, 0x85, 0xd7, 0xbe, 0xa7, 0x79, 0x8e },
        };

        [ModuleInitializer]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2255")]
        internal static void Register()
        {
            var self = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            if (self != null && self != AssemblyLoadContext.Default)
                self.Resolving += OnResolving;
            AssemblyLoadContext.Default.Resolving += OnResolving;
        }

        static Assembly OnResolving(AssemblyLoadContext ctx, AssemblyName name)
        {
            if (!IsMicrosoftNetAssembly(name)) return null;

            // First choice: return a compatible assembly that's already loaded in any ALC.
            // This handles the common RiR case where the host runs .NET 8 and already has
            // Version=8.0.0.0 of the same facade loaded; returning it is safe because these
            // are thin type-forwarding shims that all bottom out in System.Private.CoreLib.
            foreach (var alc in AssemblyLoadContext.All)
            {
                foreach (var loaded in alc.Assemblies)
                {
                    if (string.Equals(loaded.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase))
                        return loaded;
                }
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
                        return ctx.LoadFromAssemblyPath(candidate);
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
