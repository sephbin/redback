using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Redback.Components.DataTable
{
    /// <summary>
    /// Modal WinForms table editor for <see cref="GH_TableComponent"/>.
    /// In editable (unwired) mode: allows adding/removing rows and columns, then
    /// commits the result back to the component on Apply.
    /// In read-only (wired) mode: displays incoming data with editing blocked and
    /// shows a status message if the user attempts to type.
    /// </summary>
    internal sealed class TableEditorForm : Form
    {
        private readonly bool _rowsAreBranches;
        private readonly bool _isWired;

        private DataGridView  _grid;
        private ToolStrip     _toolbar;
        private StatusStrip   _status;
        private ToolStripStatusLabel _statusLabel;
        private Button        _applyButton;
        private Button        _closeButton;

        /// <summary>
        /// The parsed GH_Structure built from the grid on Apply.
        /// Null if the form was cancelled or is in read-only mode.
        /// </summary>
        public GH_Structure<IGH_Goo> ResultData { get; private set; }

        public TableEditorForm(
            GH_Structure<IGH_Goo> data,
            bool rowsAreBranches,
            bool isWired)
        {
            _rowsAreBranches = rowsAreBranches;
            _isWired         = isWired;

            BuildLayout();
            PopulateGrid(data);

            if (isWired) ApplyReadOnlyMode();
        }

        // ── Layout ────────────────────────────────────────────────────────────────

        private void BuildLayout()
        {
            Text            = _isWired ? "Table — Read Only" : "Table";
            Size            = new Size(700, 480);
            MinimumSize     = new Size(400, 280);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Toolbar (hidden in read-only mode)
            _toolbar = new ToolStrip { Dock = DockStyle.Top };
            _toolbar.Items.Add(new ToolStripButton("Add Row",    null, OnAddRow)    { DisplayStyle = ToolStripItemDisplayStyle.Text });
            _toolbar.Items.Add(new ToolStripButton("Remove Row", null, OnRemoveRow) { DisplayStyle = ToolStripItemDisplayStyle.Text });
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripButton("Add Column",    null, OnAddColumn)    { DisplayStyle = ToolStripItemDisplayStyle.Text });
            _toolbar.Items.Add(new ToolStripButton("Remove Column", null, OnRemoveColumn) { DisplayStyle = ToolStripItemDisplayStyle.Text });

            // Grid
            _grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                RowHeadersVisible     = true,
                ColumnHeadersVisible  = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.AllCells,
                ClipboardCopyMode     = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText,
            };

            // Status strip (shown in read-only mode)
            _statusLabel = new ToolStripStatusLabel
            {
                Spring    = true,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _status = new StatusStrip { Dock = DockStyle.Bottom };
            _status.Items.Add(_statusLabel);
            _status.Visible = false;

            // Button panel
            _applyButton = new Button { Text = "Apply", DialogResult = DialogResult.None, Width = 80 };
            _closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, Width = 80 };
            _applyButton.Click += OnApply;

            var buttonPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize      = true,
                Padding       = new Padding(4),
            };
            buttonPanel.Controls.Add(_closeButton);
            buttonPanel.Controls.Add(_applyButton);

            Controls.Add(_grid);
            Controls.Add(_toolbar);
            Controls.Add(_status);
            Controls.Add(buttonPanel);

            AcceptButton = _applyButton;
            CancelButton = _closeButton;
        }

        // ── Grid population ───────────────────────────────────────────────────────

        private void PopulateGrid(GH_Structure<IGH_Goo> data)
        {
            _grid.Columns.Clear();
            _grid.Rows.Clear();

            int numBranches = data?.PathCount ?? 0;
            int maxItems    = 0;
            if (data != null)
                foreach (var branch in data.Branches)
                    if (branch.Count > maxItems) maxItems = branch.Count;

            // Determine grid dimensions based on orientation.
            int numRows = _rowsAreBranches ? numBranches : maxItems;
            int numCols = _rowsAreBranches ? maxItems    : numBranches;

            // Ensure at least a 1×1 editable grid when empty.
            numRows = Math.Max(numRows, 1);
            numCols = Math.Max(numCols, 1);

            // Add columns with numeric headers.
            for (int c = 0; c < numCols; c++)
            {
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = (c + 1).ToString(),
                    SortMode   = DataGridViewColumnSortMode.NotSortable,
                    MinimumWidth = 40,
                });
            }

            // Add rows and populate cells.
            for (int r = 0; r < numRows; r++)
            {
                int rowIndex = _grid.Rows.Add();
                _grid.Rows[rowIndex].HeaderCell.Value = (r + 1).ToString();

                for (int c = 0; c < numCols; c++)
                {
                    IGH_Goo item = GetItem(data, r, c);
                    if (item != null)
                        _grid.Rows[rowIndex].Cells[c].Value = GooToDisplayString(item);
                }
            }
        }

        /// <summary>
        /// Gets the GH_Goo item at the given (row, col) from the data tree,
        /// respecting the current row/column orientation.
        /// </summary>
        private IGH_Goo GetItem(GH_Structure<IGH_Goo> data, int row, int col)
        {
            if (data == null) return null;

            int branchIndex = _rowsAreBranches ? row : col;
            int itemIndex   = _rowsAreBranches ? col : row;

            if (branchIndex >= data.PathCount) return null;
            var branch = data.Branches[branchIndex];
            if (itemIndex >= branch.Count) return null;
            return branch[itemIndex];
        }

        private static string GooToDisplayString(IGH_Goo goo)
        {
            if (goo is GH_Number n) return n.Value.ToString(CultureInfo.InvariantCulture);
            if (goo is GH_Boolean b) return b.Value.ToString().ToLower();  // "true" / "false"
            if (goo is GH_String s) return s.Value;
            return goo.ToString();
        }

        // ── Read-only mode ────────────────────────────────────────────────────────

        private void ApplyReadOnlyMode()
        {
            _grid.ReadOnly            = true;
            _grid.BackgroundColor     = SystemColors.Control;
            _grid.DefaultCellStyle.BackColor = SystemColors.Control;
            _toolbar.Visible          = false;
            _applyButton.Visible      = false;
            _closeButton.Text         = "Done";
            _status.Visible           = true;
            _statusLabel.Text         = "Read-only view. Disconnect the input wire to edit.";

            _grid.CellBeginEdit += (_, e) =>
            {
                e.Cancel = true;
                _statusLabel.Text = "Disconnect the input wire to edit.";
            };
        }

        // ── Toolbar actions ───────────────────────────────────────────────────────

        private void OnAddRow(object sender, EventArgs e)
        {
            int rowIndex = _grid.Rows.Add();
            _grid.Rows[rowIndex].HeaderCell.Value = (rowIndex + 1).ToString();
        }

        private void OnRemoveRow(object sender, EventArgs e)
        {
            if (_grid.Rows.Count == 0) return;
            int target = _grid.CurrentRow?.Index ?? _grid.Rows.Count - 1;
            _grid.Rows.RemoveAt(target);
            // Refresh row header numbers.
            for (int r = 0; r < _grid.Rows.Count; r++)
                _grid.Rows[r].HeaderCell.Value = (r + 1).ToString();
        }

        private void OnAddColumn(object sender, EventArgs e)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText   = (_grid.Columns.Count + 1).ToString(),
                SortMode     = DataGridViewColumnSortMode.NotSortable,
                MinimumWidth = 40,
            });
        }

        private void OnRemoveColumn(object sender, EventArgs e)
        {
            if (_grid.Columns.Count == 0) return;
            int target = _grid.CurrentCell?.ColumnIndex ?? _grid.Columns.Count - 1;
            _grid.Columns.RemoveAt(target);
            // Refresh column header numbers.
            for (int c = 0; c < _grid.Columns.Count; c++)
                _grid.Columns[c].HeaderText = (c + 1).ToString();
        }

        // ── Apply ─────────────────────────────────────────────────────────────────

        private void OnApply(object sender, EventArgs e)
        {
            ResultData = BuildResultData();
            DialogResult = DialogResult.OK;
            Close();
        }

        private GH_Structure<IGH_Goo> BuildResultData()
        {
            var result     = new GH_Structure<IGH_Goo>();
            int numBranches = _rowsAreBranches ? _grid.Rows.Count : _grid.Columns.Count;
            int itemsPerBranch = _rowsAreBranches ? _grid.Columns.Count : _grid.Rows.Count;

            for (int b = 0; b < numBranches; b++)
            {
                var items = new List<IGH_Goo>(itemsPerBranch);
                for (int i = 0; i < itemsPerBranch; i++)
                {
                    string text = _rowsAreBranches
                        ? _grid.Rows[b].Cells[i].Value?.ToString() ?? ""
                        : _grid.Rows[i].Cells[b].Value?.ToString() ?? "";
                    items.Add(ParseCell(text));
                }
                result.AppendRange(items, new GH_Path(b));
            }

            return result;
        }

        // ── Cell parsing ──────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a cell string with auto-coercion:
        /// tries double first, then bool, then falls back to GH_String.
        /// Uses InvariantCulture so "3.5" always parses as 3.5 regardless of locale.
        /// </summary>
        private static IGH_Goo ParseCell(string text)
        {
            if (text == null) return new GH_String("");

            if (double.TryParse(text,
                    NumberStyles.Float | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out double d))
                return new GH_Number(d);

            if (bool.TryParse(text, out bool b))
                return new GH_Boolean(b);

            return new GH_String(text);
        }
    }
}
