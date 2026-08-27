using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Redback.Components.Grasshopper;

namespace Redback.Components.DataTable
{
    /// <summary>
    /// Dual-mode data table component.
    /// <list type="bullet">
    ///   <item><description>Unwired: acts as an editable grid — double-click to open the editor, outputs whatever has been typed in.</description></item>
    ///   <item><description>Wired: read-only viewer for the incoming data tree, passes it straight through to the output.</description></item>
    /// </list>
    /// Mode is determined by whether the input has a source connected, matching the Panel convention.
    /// </summary>
    public class GH_TableComponent : RedbackGHBase
    {
        // Hand-entered data. Persisted in the GHX. Never cleared by wiring/unwiring.
        private GH_Structure<IGH_Goo> _manualData;

        // Solve-time only; populated from the input when wired. Not persisted.
        private GH_Structure<IGH_Goo> _incomingData;

        // Persisted per-instance settings.
        private bool         _rowsAreBranches = true;
        private List<string> _columnHeaders   = new List<string>();
        private bool         _jsonOutput;
        private List<float>  _columnWidths    = new List<float>();
        private List<float>  _rowHeights      = new List<float>();
        private float        _viewportWidth   = -1f;  // -1 = auto-fit content
        private float        _viewportHeight  = -1f;
        private float        _scrollX;
        private float        _scrollY;
        private Color?       _editableColourOverride;
        private Color?       _uneditableColourOverride;

        // Stored so it can be unsubscribed in RemovedFromDocument.
        private IGH_DocumentObject.ObjectChangedEventHandler _onSourceChanged;

        public GH_TableComponent()
            : base("Table", "Table",
                   "Editable data table. Double-click to open editor. " +
                   "Connect an input to view data read-only and pass it through.",
                   "2-Data - Table") { }

        public override Guid ComponentGuid => new Guid("3a7f8e2d-1c9b-4f56-a0d3-7e2b1c4f9a8e");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => LoadIcon("Icon-Table.png");

        // ── Registration ──────────────────────────────────────────────────────────

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddGenericParameter("Data", "D",
                "Connect data to view as a read-only table. Disconnect to enter data manually.",
                GH_ParamAccess.tree);
            p[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddGenericParameter("Data", "D", "Table data as a data tree.", GH_ParamAccess.tree);
        }

        public override void CreateAttributes() => m_attributes = new TableComponentAttributes(this);

        // ── Display accessors (read by TableComponentAttributes) ──────────────

        internal GH_Structure<IGH_Goo> DisplayData =>
            Params.Input[0].SourceCount > 0 ? _incomingData : _manualData;

        internal bool IsWired         => Params.Input[0].SourceCount > 0;
        internal bool RowsAreBranches => _rowsAreBranches;

        internal IReadOnlyList<string> ColumnHeaders => _columnHeaders;

        internal string GetColumnHeaderName(int col) =>
            col < _columnHeaders.Count && !string.IsNullOrWhiteSpace(_columnHeaders[col])
                ? _columnHeaders[col]
                : (col + 1).ToString();

        internal float GetColumnWidth(int col) =>
            col < _columnWidths.Count && _columnWidths[col] > 0 ? _columnWidths[col] : 64f;
        internal float GetRowHeight(int row) =>
            row < _rowHeights.Count && _rowHeights[row] > 0 ? _rowHeights[row] : 22f;

        internal float ViewportWidth  { get => _viewportWidth;  set => _viewportWidth  = value; }
        internal float ViewportHeight { get => _viewportHeight; set => _viewportHeight = value; }
        internal float ScrollX        { get => _scrollX;        set => _scrollX        = value; }
        internal float ScrollY        { get => _scrollY;        set => _scrollY        = value; }

        // ── Document lifecycle ────────────────────────────────────────────────────

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            _onSourceChanged = (_, e) =>
            {
                if (e.Type == GH_ObjectEventType.Sources)
                {
                    ExpireSolution(true);
                    Attributes.ExpireLayout();
                }
            };
            Params.Input[0].ObjectChanged += _onSourceChanged;
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            Params.Input[0].ObjectChanged -= _onSourceChanged;
            base.RemovedFromDocument(document);
        }

        // ── Solve ─────────────────────────────────────────────────────────────────

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool isWired = Params.Input[0].SourceCount > 0;
            GH_Structure<IGH_Goo> sourceData;
            if (isWired)
            {
                DA.GetDataTree(0, out GH_Structure<IGH_Goo> incoming);
                _incomingData = incoming;
                sourceData = _incomingData;
            }
            else
            {
                sourceData = _manualData ?? new GH_Structure<IGH_Goo>();
            }

            DA.SetDataTree(0, _jsonOutput ? BuildJsonOutput(sourceData) : sourceData);
            Attributes.ExpireLayout();
        }

        // ── Editor ────────────────────────────────────────────────────────────────

        /// <summary>Opens the table editor dialog. Called by <see cref="TableComponentAttributes"/> on double-click.</summary>
        public void OpenEditor()
        {
            bool isWired   = Params.Input[0].SourceCount > 0;
            var displayData = isWired ? _incomingData : _manualData;
            using var form  = new TableEditorForm(displayData, _rowsAreBranches, isWired);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK && !isWired)
            {
                RecordUndoEvent("Edit table data");
                _manualData = form.ResultData;
                ExpireSolution(true);
            }
        }

        /// <summary>Writes a single cell value from inline canvas editing.</summary>
        internal void SetCell(int row, int col, IGH_Goo value)
        {
            if (IsWired) return;
            RecordUndoEvent("Edit cell");
            if (_manualData == null) _manualData = new GH_Structure<IGH_Goo>();

            int branchIdx = _rowsAreBranches ? row : col;
            int itemIdx   = _rowsAreBranches ? col : row;

            var branch = _manualData.EnsurePath(new GH_Path(branchIdx));
            while (branch.Count <= itemIdx)
                branch.Add(null);
            branch[itemIdx] = value;

            ExpireSolution(true);
        }

        internal void SetColumnHeader(int col, string name)
        {
            RecordUndoEvent("Rename column header");
            while (_columnHeaders.Count <= col)
                _columnHeaders.Add(null);
            _columnHeaders[col] = string.IsNullOrWhiteSpace(name) ? null : name;
            ExpireSolution(true);
        }

        internal void SetColumnWidth(int col, float width)
        {
            while (_columnWidths.Count <= col) _columnWidths.Add(0f);
            _columnWidths[col] = Math.Max(20f, width);
        }

        internal void SetRowHeight(int row, float height)
        {
            while (_rowHeights.Count <= row) _rowHeights.Add(0f);
            _rowHeights[row] = Math.Max(10f, height);
        }

        // ── JSON output ───────────────────────────────────────────────────────────

        private GH_Structure<IGH_Goo> BuildJsonOutput(GH_Structure<IGH_Goo> data)
        {
            var result = new GH_Structure<IGH_Goo>();
            var (rows, cols) = GetDisplayDimensions(data);
            var items = new List<IGH_Goo>(rows);
            for (int r = 0; r < rows; r++)
            {
                var sb = new StringBuilder("{");
                for (int c = 0; c < cols; c++)
                {
                    if (c > 0) sb.Append(", ");
                    sb.Append('"').Append(EscapeJson(GetColumnHeaderName(c))).Append("\": ");
                    sb.Append(GooToJson(GetDisplayItem(data, r, c)));
                }
                sb.Append('}');
                items.Add(new GH_String(sb.ToString()));
            }
            result.AppendRange(items, new GH_Path(0));
            return result;
        }

        private (int rows, int cols) GetDisplayDimensions(GH_Structure<IGH_Goo> data)
        {
            int branches = data?.PathCount ?? 0;
            int maxItems = 0;
            if (data != null)
                foreach (var branch in data.Branches)
                    if (branch.Count > maxItems) maxItems = branch.Count;
            return (Math.Max(1, _rowsAreBranches ? branches : maxItems),
                    Math.Max(1, _rowsAreBranches ? maxItems  : branches));
        }

        private IGH_Goo GetDisplayItem(GH_Structure<IGH_Goo> data, int row, int col)
        {
            if (data == null) return null;
            int bi = _rowsAreBranches ? row : col;
            int ii = _rowsAreBranches ? col : row;
            if (bi >= data.PathCount) return null;
            var branch = data.Branches[bi];
            return ii < branch.Count ? branch[ii] : null;
        }

        private static string EscapeJson(string s) =>
            s?.Replace("\\", "\\\\").Replace("\"", "\\\"")
              .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") ?? "";

        private static string GooToJson(IGH_Goo goo)
        {
            if (goo == null)          return "null";
            if (goo is GH_Number n)  return n.Value.ToString(CultureInfo.InvariantCulture);
            if (goo is GH_Boolean b) return b.Value ? "true" : "false";
            if (goo is GH_String s)  return "\"" + EscapeJson(s.Value) + "\"";
            return "\"" + EscapeJson(goo.ToString()) + "\"";
        }

        // ── Colour tinting ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the effective tint colour for the component in its current mode.
        /// Per-instance override takes precedence over the plugin-level default.
        /// A colour with Alpha == 0 means "no tint".
        /// </summary>
        public Color GetCurrentTintColour(bool isWired)
        {
            return isWired
                ? (_uneditableColourOverride ?? TableComponentSettings.DefaultUneditableColour)
                : (_editableColourOverride   ?? TableComponentSettings.DefaultEditableColour);
        }

        // ── Context menu ──────────────────────────────────────────────────────────

        protected override void AppendAdditionalComponentMenuItems(
            System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            Menu_AppendItem(menu, "Rows are branches", OnToggleOrientation,
                enabled: true, @checked: _rowsAreBranches);
            Menu_AppendItem(menu, "Output as JSON", OnToggleJsonOutput,
                enabled: true, @checked: _jsonOutput);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Open editor...", (_, __) => OpenEditor());
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Set editable colour...",   OnSetEditableColour);
            Menu_AppendItem(menu, "Set uneditable colour...", OnSetUneditableColour);
            Menu_AppendItem(menu, "Set as default editable colour",   OnSetDefaultEditableColour);
            Menu_AppendItem(menu, "Set as default uneditable colour", OnSetDefaultUneditableColour);
        }

        private void OnToggleOrientation(object sender, EventArgs e)
        {
            RecordUndoEvent("Toggle table orientation");
            _rowsAreBranches = !_rowsAreBranches;
            ExpireSolution(true);
        }

        private void OnToggleJsonOutput(object sender, EventArgs e)
        {
            RecordUndoEvent("Toggle JSON output");
            _jsonOutput = !_jsonOutput;
            ExpireSolution(true);
        }

        private void OnSetEditableColour(object sender, EventArgs e)
        {
            using var dlg = new System.Windows.Forms.ColorDialog
            {
                FullOpen = true,
                Color    = _editableColourOverride ?? TableComponentSettings.DefaultEditableColour,
            };
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            _editableColourOverride = dlg.Color;
            Attributes.ExpireLayout();
            global::Grasshopper.Instances.ActiveCanvas?.Refresh();
        }

        private void OnSetUneditableColour(object sender, EventArgs e)
        {
            using var dlg = new System.Windows.Forms.ColorDialog
            {
                FullOpen = true,
                Color    = _uneditableColourOverride ?? TableComponentSettings.DefaultUneditableColour,
            };
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            _uneditableColourOverride = dlg.Color;
            Attributes.ExpireLayout();
            global::Grasshopper.Instances.ActiveCanvas?.Refresh();
        }

        private void OnSetDefaultEditableColour(object sender, EventArgs e)
        {
            TableComponentSettings.SetDefaultEditableColour(GetCurrentTintColour(false));
        }

        private void OnSetDefaultUneditableColour(object sender, EventArgs e)
        {
            TableComponentSettings.SetDefaultUneditableColour(GetCurrentTintColour(true));
        }

        // ── Persistence (GHX) ─────────────────────────────────────────────────────

        public override bool Write(GH_IWriter writer)
        {
            if (!base.Write(writer)) return false;

            // Serialize _manualData using explicit per-cell type tags.
            // GH_Structure<IGH_Goo>.Write is not used directly because type erasure on
            // the interface prevents reliable round-tripping of mixed concrete types.
            var md = writer.CreateChunk("ManualData");
            int branchCount = _manualData?.PathCount ?? 0;
            md.SetInt32("BC", branchCount);
            for (int b = 0; b < branchCount; b++)
            {
                var path   = _manualData.Paths[b];
                var branch = _manualData.Branches[b];
                var bc = md.CreateChunk("B", b);
                bc.SetInt32("P", path[0]);       // paths are always single-index {n}
                bc.SetInt32("C", branch.Count);
                for (int i = 0; i < branch.Count; i++)
                    SerializeCell(bc.CreateChunk("I", i), branch[i]);
            }

            writer.SetBoolean("RowsAreBranches", _rowsAreBranches);
            writer.SetBoolean("JsonOutput", _jsonOutput);

            var ch = writer.CreateChunk("ColumnHeaders");
            ch.SetInt32("Count", _columnHeaders.Count);
            for (int i = 0; i < _columnHeaders.Count; i++)
                ch.SetString("H" + i, _columnHeaders[i] ?? "");

            writer.SetDouble("ViewportWidth",  _viewportWidth);
            writer.SetDouble("ViewportHeight", _viewportHeight);
            writer.SetDouble("ScrollX", _scrollX);
            writer.SetDouble("ScrollY", _scrollY);

            var cw = writer.CreateChunk("ColumnWidths");
            cw.SetInt32("Count", _columnWidths.Count);
            for (int i = 0; i < _columnWidths.Count; i++)
                cw.SetDouble("W" + i, _columnWidths[i]);

            var rh = writer.CreateChunk("RowHeights");
            rh.SetInt32("Count", _rowHeights.Count);
            for (int i = 0; i < _rowHeights.Count; i++)
                rh.SetDouble("H" + i, _rowHeights[i]);

            if (_editableColourOverride.HasValue)
                writer.SetInt32("EditColour", _editableColourOverride.Value.ToArgb());
            if (_uneditableColourOverride.HasValue)
                writer.SetInt32("UneditColour", _uneditableColourOverride.Value.ToArgb());

            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            if (!base.Read(reader)) return false;

            _manualData = new GH_Structure<IGH_Goo>();
            var md = reader.FindChunk("ManualData");
            if (md != null)
            {
                int branchCount = 0;
                md.TryGetInt32("BC", ref branchCount);
                for (int b = 0; b < branchCount; b++)
                {
                    var bc = md.FindChunk("B", b);
                    if (bc == null) continue;
                    int pathIdx = 0;
                    bc.TryGetInt32("P", ref pathIdx);
                    int itemCount = 0;
                    bc.TryGetInt32("C", ref itemCount);
                    var items = new List<IGH_Goo>(itemCount);
                    for (int i = 0; i < itemCount; i++)
                    {
                        var ic = bc.FindChunk("I", i);
                        items.Add(ic != null ? DeserializeCell(ic) : null);
                    }
                    _manualData.AppendRange(items, new GH_Path(pathIdx));
                }
            }

            _rowsAreBranches = true;
            reader.TryGetBoolean("RowsAreBranches", ref _rowsAreBranches);
            _jsonOutput = false;
            reader.TryGetBoolean("JsonOutput", ref _jsonOutput);

            _columnHeaders = new List<string>();
            var ch = reader.FindChunk("ColumnHeaders");
            if (ch != null)
            {
                int count = 0;
                ch.TryGetInt32("Count", ref count);
                for (int i = 0; i < count; i++)
                {
                    string h = "";
                    ch.TryGetString("H" + i, ref h);
                    _columnHeaders.Add(string.IsNullOrWhiteSpace(h) ? null : h);
                }
            }

            _viewportWidth = -1f; _viewportHeight = -1f;
            double vpw = -1, vph = -1, sx = 0, sy = 0;
            if (reader.TryGetDouble("ViewportWidth",  ref vpw)) _viewportWidth  = (float)vpw;
            if (reader.TryGetDouble("ViewportHeight", ref vph)) _viewportHeight = (float)vph;
            if (reader.TryGetDouble("ScrollX", ref sx)) _scrollX = (float)sx;
            if (reader.TryGetDouble("ScrollY", ref sy)) _scrollY = (float)sy;

            _columnWidths = new List<float>();
            var cw = reader.FindChunk("ColumnWidths");
            if (cw != null)
            {
                int count = 0; cw.TryGetInt32("Count", ref count);
                for (int i = 0; i < count; i++)
                {
                    double w = 0; cw.TryGetDouble("W" + i, ref w);
                    _columnWidths.Add((float)w);
                }
            }

            _rowHeights = new List<float>();
            var rh = reader.FindChunk("RowHeights");
            if (rh != null)
            {
                int count = 0; rh.TryGetInt32("Count", ref count);
                for (int i = 0; i < count; i++)
                {
                    double h = 0; rh.TryGetDouble("H" + i, ref h);
                    _rowHeights.Add((float)h);
                }
            }

            int argb = 0;
            if (reader.TryGetInt32("EditColour", ref argb))
                _editableColourOverride = Color.FromArgb(argb);
            if (reader.TryGetInt32("UneditColour", ref argb))
                _uneditableColourOverride = Color.FromArgb(argb);

            return true;
        }

        // ── Cell serialization helpers ────────────────────────────────────────────

        private static void SerializeCell(GH_IWriter w, IGH_Goo goo)
        {
            if (goo is GH_Number n) { w.SetString("T", "N"); w.SetDouble("V", n.Value); }
            else if (goo is GH_Boolean b) { w.SetString("T", "B"); w.SetBoolean("V", b.Value); }
            else if (goo is GH_String s) { w.SetString("T", "S"); w.SetString("V", s.Value); }
            else w.SetString("T", "");  // null or unrecognised type
        }

        private static IGH_Goo DeserializeCell(GH_IReader r)
        {
            string tag = "";
            if (!r.TryGetString("T", ref tag)) return null;
            switch (tag)
            {
                case "N":
                    double d = 0.0;
                    r.TryGetDouble("V", ref d);
                    return new GH_Number(d);
                case "B":
                    bool bv = false;
                    r.TryGetBoolean("V", ref bv);
                    return new GH_Boolean(bv);
                case "S":
                    string sv = "";
                    r.TryGetString("V", ref sv);
                    return new GH_String(sv);
                default:
                    return null;
            }
        }
    }
}
