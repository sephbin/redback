using System;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace Redback.Components.DataTable
{
    /// <summary>
    /// Plugin-level persistent defaults for GH_TableComponent colour overrides.
    /// Stored as JSON in %APPDATA%\Grasshopper\Libraries\Redback\redback-settings.json.
    /// Per-instance colour overrides (stored in the GHX) take precedence over these.
    /// </summary>
    internal static class TableComponentSettings
    {
        private static readonly string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Grasshopper", "Libraries", "Redback", "redback-settings.json");

        // Null means "use Panel default" — populated lazily from the JSON file.
        private static Color? _editableColour;
        private static Color? _uneditableColour;
        private static bool   _loaded;

        /// <summary>
        /// Default background colour for the editable (unwired) state.
        /// Falls back to the GH Panel background colour when no override has been saved.
        /// </summary>
        public static Color DefaultEditableColour
        {
            get { EnsureLoaded(); return _editableColour ?? PanelDefaultColour; }
        }

        /// <summary>
        /// Default background colour for the uneditable (wired) state.
        /// Falls back to the GH Panel background colour when no override has been saved.
        /// </summary>
        public static Color DefaultUneditableColour
        {
            get { EnsureLoaded(); return _uneditableColour ?? PanelDefaultColour; }
        }

        public static void SetDefaultEditableColour(Color c)
        {
            EnsureLoaded();
            _editableColour = c;
            Save();
        }

        public static void SetDefaultUneditableColour(Color c)
        {
            EnsureLoaded();
            _uneditableColour = c;
            Save();
        }

        // -- Panel default colour --

        private static Color? _panelColour;

        /// <summary>
        /// The Grasshopper Panel component's default background colour, retrieved at runtime.
        /// Used as the initial default so a fresh Table component looks like a Panel.
        /// NOTE: verify this matches the actual Panel appearance in GH 8.21 at build time.
        /// </summary>
        private static Color PanelDefaultColour
        {
            get
            {
                if (_panelColour.HasValue) return _panelColour.Value;
                try
                {
                    // Instantiate a temporary GH_Panel to read its default colour.
                    _panelColour = new global::Grasshopper.Kernel.Special.GH_Panel().Properties.Colour;
                }
                catch
                {
                    // Fallback if GH_Panel cannot be instantiated outside a document context.
                    _panelColour = SystemColors.Control;
                }
                return _panelColour.Value;
            }
        }

        // -- Load / Save --

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            if (!File.Exists(_settingsPath)) return;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(_settingsPath));
                var root = doc.RootElement;
                if (root.TryGetProperty("TableEditColour", out var ec) && ec.TryGetInt32(out int ea))
                    _editableColour = Color.FromArgb(ea);
                if (root.TryGetProperty("TableUneditColour", out var uc) && uc.TryGetInt32(out int ua))
                    _uneditableColour = Color.FromArgb(ua);
            }
            catch
            {
                // Corrupt or unreadable settings — ignore and use defaults.
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                var data = new
                {
                    TableEditColour   = _editableColour?.ToArgb(),
                    TableUneditColour = _uneditableColour?.ToArgb(),
                };
                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(data,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Non-fatal: if we can't save, next session just reverts to defaults.
            }
        }
    }
}
