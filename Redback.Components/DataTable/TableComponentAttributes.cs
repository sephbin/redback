using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Redback.Components.DataTable
{
    internal sealed class TableComponentAttributes : GH_ComponentAttributes
    {
        private const float RowHdrW      = 26f;
        private const float ColHdrH      = 18f;
        private const float ScrollBarW   = 12f;
        private const float ResizeTol    = 4f;
        private const float MinCellW     = 20f;
        private const float MinCellH     = 10f;
        private const float MinViewportW = 60f;
        private const float MinViewportH = 30f;

        private static readonly StringFormat _centred = new StringFormat
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming      = StringTrimming.EllipsisCharacter,
        };
        private static readonly StringFormat _cellFmt = new StringFormat
        {
            Alignment     = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming      = StringTrimming.EllipsisCharacter,
        };

        public TableComponentAttributes(GH_TableComponent owner) : base(owner) { }

        private GH_TableComponent Table => (GH_TableComponent)Owner;

        // ── Layout state ──────────────────────────────────────────────────────────

        private RectangleF _body;       // full rect including scrollbar strips
        private RectangleF _viewport;   // content area excluding scrollbar strips
        private bool       _hasHScroll;
        private bool       _hasVScroll;
        private RectangleF _hScrollTrack;
        private RectangleF _hScrollThumb;
        private RectangleF _vScrollTrack;
        private RectangleF _vScrollThumb;
        private float[]    _colX;       // cumulative column-width offsets from content left
        private float[]    _rowY;       // cumulative row-height offsets from content top

        // ── Inline edit state ─────────────────────────────────────────────────────

        private TextBox _activeTextBox;
        private int     _editRow;
        private int     _editCol;

        // ── Drag state ────────────────────────────────────────────────────────────

        private enum DragMode
        {
            None,
            ResizeColumn, ResizeRow,
            ResizeRight, ResizeBottom, ResizeCorner,
            ScrollH, ScrollV,
        }
        private DragMode _dragMode;
        private int      _dragIndex;
        private PointF   _dragAnchor;
        private float    _dragStartValue;
        private float    _dragStartW;
        private float    _dragStartH;
        private float    _scrollDragStart;

        // ── Layout ────────────────────────────────────────────────────────────────

        protected override void Layout()
        {
            var (rows, cols) = GetDimensions();

            _colX = new float[cols + 1];
            for (int c = 0; c < cols; c++)
                _colX[c + 1] = _colX[c] + Table.GetColumnWidth(c);

            _rowY = new float[rows + 1];
            for (int r = 0; r < rows; r++)
                _rowY[r + 1] = _rowY[r] + Table.GetRowHeight(r);

            float totalW = RowHdrW + _colX[cols];
            float totalH = ColHdrH + _rowY[rows];

            float vpW = Table.ViewportWidth  > 0 ? Table.ViewportWidth  : totalW;
            float vpH = Table.ViewportHeight > 0 ? Table.ViewportHeight : totalH;

            bool needH = totalW > vpW + 0.5f;
            bool needV = totalH > vpH + 0.5f;
            if (needH && !needV && totalH > vpH - ScrollBarW + 0.5f) needV = true;
            if (needV && !needH && totalW > vpW - ScrollBarW + 0.5f) needH = true;
            _hasHScroll = needH;
            _hasVScroll = needV;

            float bodyW = vpW + (needV ? ScrollBarW : 0);
            float bodyH = vpH + (needH ? ScrollBarW : 0);

            _body     = new RectangleF(Pivot.X - bodyW / 2f, Pivot.Y - bodyH / 2f, bodyW, bodyH);
            _viewport = new RectangleF(_body.X, _body.Y, vpW, vpH);

            float maxSX = Math.Max(0f, totalW - vpW);
            float maxSY = Math.Max(0f, totalH - vpH);
            Table.ScrollX = Math.Max(0f, Math.Min(Table.ScrollX, maxSX));
            Table.ScrollY = Math.Max(0f, Math.Min(Table.ScrollY, maxSY));

            if (_hasHScroll)
            {
                _hScrollTrack = new RectangleF(_body.X, _body.Bottom - ScrollBarW, vpW, ScrollBarW);
                float tw  = maxSX > 0 ? Math.Max(20f, vpW * vpW / totalW) : vpW;
                float tx  = maxSX > 0 ? _body.X + (vpW - tw) * Table.ScrollX / maxSX : _body.X;
                _hScrollThumb = new RectangleF(tx, _hScrollTrack.Y + 2f, tw, ScrollBarW - 4f);
            }
            if (_hasVScroll)
            {
                _vScrollTrack = new RectangleF(_body.Right - ScrollBarW, _body.Y, ScrollBarW, vpH);
                float th  = maxSY > 0 ? Math.Max(20f, vpH * vpH / totalH) : vpH;
                float ty  = maxSY > 0 ? _body.Y + (vpH - th) * Table.ScrollY / maxSY : _body.Y;
                _vScrollThumb = new RectangleF(_vScrollTrack.X + 2f, ty, ScrollBarW - 4f, th);
            }

            LayoutInputParams(Owner, _body);
            LayoutOutputParams(Owner, _body);

            Bounds = _body;
            foreach (var p in Owner.Params.Input)
                Bounds = RectangleF.Union(Bounds, p.Attributes.Bounds);
            foreach (var p in Owner.Params.Output)
                Bounds = RectangleF.Union(Bounds, p.Attributes.Bounds);
        }

        // ── Render ────────────────────────────────────────────────────────────────

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                DrawBody(graphics);
                var style = GH_CapsuleRenderEngine.GetImpliedStyle(GH_Palette.Normal, this);
                RenderComponentParameters(canvas, graphics, Owner, style);
            }
            else
            {
                base.Render(canvas, graphics, channel);
            }
        }

        private void DrawBody(Graphics graphics)
        {
            if (_colX == null || _rowY == null) return;

            var  data    = Table.DisplayData;
            bool isWired = Table.IsWired;
            bool byRows  = Table.RowsAreBranches;
            int  cols    = _colX.Length - 1;
            int  rows    = _rowY.Length - 1;

            var bgColour = Table.GetCurrentTintColour(isWired);
            if (bgColour.A == 0) bgColour = Color.WhiteSmoke;

            float ox       = _body.X;
            float oy       = _body.Y;
            float contentL = ox + RowHdrW;
            float contentT = oy + ColHdrH;
            float cellAreaW = Math.Max(0f, _viewport.Width  - RowHdrW);
            float cellAreaH = Math.Max(0f, _viewport.Height - ColHdrH);

            var cellArea   = new RectangleF(contentL, contentT, cellAreaW, cellAreaH);
            var colHdrArea = new RectangleF(contentL, oy, cellAreaW, ColHdrH);
            var rowHdrArea = new RectangleF(ox, contentT, RowHdrW, cellAreaH);

            var hdrFont = GH_FontServer.Standard;

            // Background.
            using (var br = new SolidBrush(bgColour))
                graphics.FillRectangle(br, _viewport);
            using (var br = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                graphics.FillRectangle(br, ox, oy, _viewport.Width, ColHdrH);
                graphics.FillRectangle(br, ox, oy, RowHdrW, _viewport.Height);
            }

            // Cell content — scrolled both axes.
            {
                var state = graphics.Save();
                graphics.SetClip(cellArea);
                graphics.TranslateTransform(-Table.ScrollX, -Table.ScrollY);

                using (var pen = new Pen(Color.FromArgb(55, 0, 0, 0), 0.5f))
                {
                    for (int c = 0; c <= cols; c++)
                        graphics.DrawLine(pen, contentL + _colX[c], contentT,
                                               contentL + _colX[c], contentT + _rowY[rows]);
                    for (int r = 0; r <= rows; r++)
                        graphics.DrawLine(pen, contentL,               contentT + _rowY[r],
                                               contentL + _colX[cols], contentT + _rowY[r]);
                }
                using (var br = new SolidBrush(Color.FromArgb(220, 20, 20, 20)))
                {
                    for (int r = 0; r < rows; r++)
                    {
                        float cy = contentT + _rowY[r];
                        float ch = _rowY[r + 1] - _rowY[r];
                        for (int c = 0; c < cols; c++)
                        {
                            var item = GetItem(data, r, c, byRows);
                            if (item == null) continue;
                            float cx = contentL + _colX[c];
                            float cw = _colX[c + 1] - _colX[c];
                            graphics.DrawString(ItemToString(item), hdrFont, br,
                                new RectangleF(cx + 3, cy, cw - 6, ch), _cellFmt);
                        }
                    }
                }
                graphics.Restore(state);
            }

            // Column headers — scrolled X only.
            {
                var state = graphics.Save();
                graphics.SetClip(colHdrArea);
                graphics.TranslateTransform(-Table.ScrollX, 0);

                using (var br = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                    for (int c = 0; c < cols; c++)
                        graphics.DrawString(Table.GetColumnHeaderName(c), hdrFont, br,
                            new RectangleF(contentL + _colX[c], oy, _colX[c + 1] - _colX[c], ColHdrH),
                            _centred);

                using (var pen = new Pen(Color.FromArgb(55, 0, 0, 0), 0.5f))
                    for (int c = 0; c <= cols; c++)
                        graphics.DrawLine(pen, contentL + _colX[c], oy,
                                               contentL + _colX[c], oy + ColHdrH);

                graphics.Restore(state);
            }

            // Row headers — scrolled Y only.
            {
                var state = graphics.Save();
                graphics.SetClip(rowHdrArea);
                graphics.TranslateTransform(0, -Table.ScrollY);

                using (var br = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                    for (int r = 0; r < rows; r++)
                        graphics.DrawString((r + 1).ToString(), hdrFont, br,
                            new RectangleF(ox, contentT + _rowY[r], RowHdrW, _rowY[r + 1] - _rowY[r]),
                            _centred);

                using (var pen = new Pen(Color.FromArgb(55, 0, 0, 0), 0.5f))
                    for (int r = 0; r <= rows; r++)
                        graphics.DrawLine(pen, ox, contentT + _rowY[r],
                                               ox + RowHdrW, contentT + _rowY[r]);

                graphics.Restore(state);
            }

            // Header separator lines (unscrolled).
            using (var pen = new Pen(Color.FromArgb(110, 0, 0, 0), 1f))
            {
                graphics.DrawLine(pen, ox, contentT, ox + _viewport.Width, contentT);
                graphics.DrawLine(pen, contentL, oy, contentL, oy + _viewport.Height);
            }

            // "read-only" label.
            if (isWired)
            {
                var roFmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                using (var br = new SolidBrush(Color.FromArgb(110, 110, 110)))
                    graphics.DrawString("read-only", hdrFont, br,
                        new RectangleF(ox, oy + 1, _viewport.Width - 3, ColHdrH - 2), roFmt);
            }

            // Scrollbars.
            if (_hasHScroll || _hasVScroll)
            {
                using var trackBr = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
                using var thumbBr = new SolidBrush(Color.FromArgb(130, 0, 0, 0));
                if (_hasHScroll) { graphics.FillRectangle(trackBr, _hScrollTrack); graphics.FillRectangle(thumbBr, _hScrollThumb); }
                if (_hasVScroll) { graphics.FillRectangle(trackBr, _vScrollTrack); graphics.FillRectangle(thumbBr, _vScrollThumb); }
                if (_hasHScroll && _hasVScroll)
                    graphics.FillRectangle(trackBr, new RectangleF(_vScrollTrack.X, _hScrollTrack.Y, ScrollBarW, ScrollBarW));
            }

            // Outer border.
            using (var pen = new Pen(
                Selected ? Color.FromArgb(255, 30, 140, 255) : Color.FromArgb(140, 80, 80, 80),
                Selected ? 2f : 1f))
            {
                graphics.DrawRectangle(pen, ox, oy, _body.Width, _body.Height);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private (int rows, int cols) GetDimensions()
        {
            var  data   = Table.DisplayData;
            bool byRows = Table.RowsAreBranches;
            int branches = data?.PathCount ?? 0;
            int maxItems = 0;
            if (data != null)
                foreach (var branch in data.Branches)
                    if (branch.Count > maxItems) maxItems = branch.Count;
            return (Math.Max(1, byRows ? branches : maxItems),
                    Math.Max(1, byRows ? maxItems  : branches));
        }

        private static IGH_Goo GetItem(GH_Structure<IGH_Goo> data, int row, int col, bool byRows)
        {
            if (data == null) return null;
            int bi = byRows ? row : col;
            int ii = byRows ? col : row;
            if (bi >= data.PathCount) return null;
            var branch = data.Branches[bi];
            return ii < branch.Count ? branch[ii] : null;
        }

        private static string ItemToString(IGH_Goo goo)
        {
            if (goo is GH_Number n)  return n.Value.ToString(CultureInfo.InvariantCulture);
            if (goo is GH_Boolean b) return b.Value ? "true" : "false";
            if (goo is GH_String s)  return s.Value;
            return goo.ToString();
        }

        private float TotalContentW() => _colX != null ? RowHdrW + _colX[_colX.Length - 1] : 0f;
        private float TotalContentH() => _rowY != null ? ColHdrH + _rowY[_rowY.Length - 1] : 0f;

        // ── Mouse interaction ─────────────────────────────────────────────────────

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button != MouseButtons.Left)
                return base.RespondToMouseDown(sender, e);

            var p = e.CanvasLocation;

            // Scrollbar thumbs.
            if (_hasHScroll && _hScrollThumb.Contains(p))
            {
                _dragMode = DragMode.ScrollH; _dragAnchor = p; _scrollDragStart = Table.ScrollX;
                return GH_ObjectResponse.Capture;
            }
            if (_hasVScroll && _vScrollThumb.Contains(p))
            {
                _dragMode = DragMode.ScrollV; _dragAnchor = p; _scrollDragStart = Table.ScrollY;
                return GH_ObjectResponse.Capture;
            }

            // Scrollbar tracks — jump to position.
            if (_hasHScroll && _hScrollTrack.Contains(p))
            {
                float maxSX   = TotalContentW() - _viewport.Width;
                Table.ScrollX = Math.Max(0f, Math.Min(maxSX,
                    (p.X - _hScrollTrack.X) / _hScrollTrack.Width * TotalContentW() - _viewport.Width * 0.5f));
                ExpireLayout(); sender.Refresh();
                return GH_ObjectResponse.Handled;
            }
            if (_hasVScroll && _vScrollTrack.Contains(p))
            {
                float maxSY   = TotalContentH() - _viewport.Height;
                Table.ScrollY = Math.Max(0f, Math.Min(maxSY,
                    (p.Y - _vScrollTrack.Y) / _vScrollTrack.Height * TotalContentH() - _viewport.Height * 0.5f));
                ExpireLayout(); sender.Refresh();
                return GH_ObjectResponse.Handled;
            }

            // Column resize (drag right edge of column header).
            int colR = HitTestColumnResize(p);
            if (colR >= 0)
            {
                _dragMode = DragMode.ResizeColumn; _dragIndex = colR;
                _dragAnchor = p; _dragStartValue = Table.GetColumnWidth(colR);
                return GH_ObjectResponse.Capture;
            }

            // Row resize (drag bottom edge of row header).
            int rowR = HitTestRowResize(p);
            if (rowR >= 0)
            {
                _dragMode = DragMode.ResizeRow; _dragIndex = rowR;
                _dragAnchor = p; _dragStartValue = Table.GetRowHeight(rowR);
                return GH_ObjectResponse.Capture;
            }

            // Viewport resize (drag body edges).
            var edge = HitTestViewportResize(p);
            if (edge != ViewportEdge.None)
            {
                _dragMode   = edge == ViewportEdge.Right  ? DragMode.ResizeRight  :
                              edge == ViewportEdge.Bottom ? DragMode.ResizeBottom : DragMode.ResizeCorner;
                _dragAnchor = p;
                _dragStartW = Table.ViewportWidth  > 0 ? Table.ViewportWidth  : _viewport.Width;
                _dragStartH = Table.ViewportHeight > 0 ? Table.ViewportHeight : _viewport.Height;
                return GH_ObjectResponse.Capture;
            }

            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            var   p  = e.CanvasLocation;
            float dx = p.X - _dragAnchor.X;
            float dy = p.Y - _dragAnchor.Y;

            if (_dragMode != DragMode.None)
            {
                switch (_dragMode)
                {
                    case DragMode.ResizeColumn:
                        Table.SetColumnWidth(_dragIndex, Math.Max(MinCellW, _dragStartValue + dx));
                        ExpireLayout(); sender.Refresh(); break;

                    case DragMode.ResizeRow:
                        Table.SetRowHeight(_dragIndex, Math.Max(MinCellH, _dragStartValue + dy));
                        ExpireLayout(); sender.Refresh(); break;

                    case DragMode.ResizeRight:
                        Table.ViewportWidth = Math.Max(MinViewportW, _dragStartW + dx);
                        ExpireLayout(); sender.Refresh(); break;

                    case DragMode.ResizeBottom:
                        Table.ViewportHeight = Math.Max(MinViewportH, _dragStartH + dy);
                        ExpireLayout(); sender.Refresh(); break;

                    case DragMode.ResizeCorner:
                        Table.ViewportWidth  = Math.Max(MinViewportW, _dragStartW + dx);
                        Table.ViewportHeight = Math.Max(MinViewportH, _dragStartH + dy);
                        ExpireLayout(); sender.Refresh(); break;

                    case DragMode.ScrollH:
                    {
                        float range = _hScrollTrack.Width - _hScrollThumb.Width;
                        float maxSX = TotalContentW() - _viewport.Width;
                        Table.ScrollX = range > 0
                            ? Math.Max(0f, Math.Min(maxSX, _scrollDragStart + dx * maxSX / range)) : 0f;
                        ExpireLayout(); sender.Refresh(); break;
                    }
                    case DragMode.ScrollV:
                    {
                        float range = _vScrollTrack.Height - _vScrollThumb.Height;
                        float maxSY = TotalContentH() - _viewport.Height;
                        Table.ScrollY = range > 0
                            ? Math.Max(0f, Math.Min(maxSY, _scrollDragStart + dy * maxSY / range)) : 0f;
                        ExpireLayout(); sender.Refresh(); break;
                    }
                }
                return GH_ObjectResponse.Capture;
            }

            // Cursor feedback.
            var ve = HitTestViewportResize(p);
            sender.Cursor =
                ve == ViewportEdge.Corner      ? Cursors.SizeNWSE :
                ve == ViewportEdge.Right       ? Cursors.SizeWE   :
                ve == ViewportEdge.Bottom      ? Cursors.SizeNS   :
                HitTestColumnResize(p) >= 0    ? Cursors.SizeWE   :
                HitTestRowResize(p)    >= 0    ? Cursors.SizeNS   :
                                                 Cursors.Default;

            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_dragMode != DragMode.None)
            {
                _dragMode = DragMode.None;
                sender.Cursor = Cursors.Default;
                ExpireLayout(); sender.Refresh();
                return GH_ObjectResponse.Release;
            }
            return base.RespondToMouseUp(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(
            GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!Table.IsWired)
            {
                int hdrCol = HitTestColumnHeader(e.CanvasLocation);
                if (hdrCol >= 0) { BeginHeaderEdit(sender, hdrCol); return GH_ObjectResponse.Handled; }

                var (row, col) = HitTestCell(e.CanvasLocation);
                if (row >= 0) { BeginCellEdit(sender, row, col); return GH_ObjectResponse.Handled; }
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }

        // ── Hit testing ───────────────────────────────────────────────────────────

        private int HitTestColumnHeader(PointF p)
        {
            if (_colX == null || p.Y < _body.Y || p.Y >= _body.Y + ColHdrH) return -1;
            float relX = p.X - (_body.X + RowHdrW) + Table.ScrollX;
            if (relX < 0) return -1;
            int cols = _colX.Length - 1;
            for (int c = 0; c < cols; c++)
                if (relX >= _colX[c] && relX < _colX[c + 1]) return c;
            return -1;
        }

        private (int row, int col) HitTestCell(PointF p)
        {
            if (_colX == null || _rowY == null) return (-1, -1);
            if (p.X > _viewport.Right || p.Y > _viewport.Bottom) return (-1, -1);
            float relX = p.X - (_body.X + RowHdrW) + Table.ScrollX;
            float relY = p.Y - (_body.Y + ColHdrH) + Table.ScrollY;
            if (relX < 0 || relY < 0) return (-1, -1);

            int cols = _colX.Length - 1, col = -1;
            for (int c = 0; c < cols; c++)
                if (relX >= _colX[c] && relX < _colX[c + 1]) { col = c; break; }
            if (col < 0) return (-1, -1);

            int rows = _rowY.Length - 1, row = -1;
            for (int r = 0; r < rows; r++)
                if (relY >= _rowY[r] && relY < _rowY[r + 1]) { row = r; break; }
            return row < 0 ? (-1, -1) : (row, col);
        }

        private int HitTestColumnResize(PointF p)
        {
            if (_colX == null || p.Y < _body.Y || p.Y > _body.Y + ColHdrH + ResizeTol) return -1;
            float relX = p.X - (_body.X + RowHdrW) + Table.ScrollX;
            int cols = _colX.Length - 1;
            for (int c = 0; c < cols; c++)
                if (Math.Abs(relX - _colX[c + 1]) <= ResizeTol) return c;
            return -1;
        }

        private int HitTestRowResize(PointF p)
        {
            if (_rowY == null || p.X < _body.X || p.X > _body.X + RowHdrW + ResizeTol) return -1;
            float relY = p.Y - (_body.Y + ColHdrH) + Table.ScrollY;
            if (relY < -ResizeTol) return -1;
            int rows = _rowY.Length - 1;
            for (int r = 0; r < rows; r++)
                if (Math.Abs(relY - _rowY[r + 1]) <= ResizeTol) return r;
            return -1;
        }

        private enum ViewportEdge { None, Right, Bottom, Corner }

        private ViewportEdge HitTestViewportResize(PointF p)
        {
            bool nearR = Math.Abs(p.X - _body.Right)  < ResizeTol && p.Y >= _body.Y - ResizeTol && p.Y <= _body.Bottom + ResizeTol;
            bool nearB = Math.Abs(p.Y - _body.Bottom) < ResizeTol && p.X >= _body.X - ResizeTol && p.X <= _body.Right  + ResizeTol;
            if (nearR && nearB) return ViewportEdge.Corner;
            if (nearR)          return ViewportEdge.Right;
            if (nearB)          return ViewportEdge.Bottom;
            return ViewportEdge.None;
        }

        // ── Cell / header inline editing ──────────────────────────────────────────

        private void BeginCellEdit(GH_Canvas canvas, int row, int col)
        {
            if (_activeTextBox != null) CancelCellEdit(canvas);
            if (_colX == null || _rowY == null) return;

            float cx = _body.X + RowHdrW + _colX[col]   - Table.ScrollX;
            float cy = _body.Y + ColHdrH + _rowY[row]   - Table.ScrollY;
            float cw = _colX[col + 1] - _colX[col];
            float ch = _rowY[row + 1] - _rowY[row];

            PointF tl = canvas.Viewport.ProjectPoint(new PointF(cx,      cy));
            PointF br = canvas.Viewport.ProjectPoint(new PointF(cx + cw, cy + ch));

            var item = GetItem(Table.DisplayData, row, col, Table.RowsAreBranches);
            _editRow = row; _editCol = col;

            var tb = CreateEditBox(tl, br, item != null ? ItemToString(item) : "");
            tb.KeyDown += (_, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Return: CommitEdit(tb.Text, canvas); e.Handled = e.SuppressKeyPress = true; break;
                    case Keys.Tab:    CommitEdit(tb.Text, canvas); TabToNextCell(canvas, row, col); e.Handled = e.SuppressKeyPress = true; break;
                    case Keys.Escape: CancelCellEdit(canvas);      e.Handled = e.SuppressKeyPress = true; break;
                }
            };
            tb.LostFocus += (_, __) => { if (_activeTextBox == tb) CommitEdit(tb.Text, canvas); };
            _activeTextBox = tb;
            canvas.Controls.Add(tb);
            tb.Focus();
        }

        private void TabToNextCell(GH_Canvas canvas, int row, int col)
        {
            var (rows, cols) = GetDimensions();
            int nextCol = col + 1, nextRow = row;
            if (nextCol >= cols) { nextCol = 0; nextRow = row + 1; }
            if (nextRow >= rows)   nextRow = 0;
            BeginCellEdit(canvas, nextRow, nextCol);
        }

        private void BeginHeaderEdit(GH_Canvas canvas, int col)
        {
            if (_activeTextBox != null) CancelCellEdit(canvas);
            if (_colX == null) return;

            float cx = _body.X + RowHdrW + _colX[col] - Table.ScrollX;
            float cw = _colX[col + 1] - _colX[col];

            PointF tl = canvas.Viewport.ProjectPoint(new PointF(cx,      _body.Y));
            PointF br = canvas.Viewport.ProjectPoint(new PointF(cx + cw, _body.Y + ColHdrH));

            var headers = Table.ColumnHeaders;
            string text = col < headers.Count && !string.IsNullOrWhiteSpace(headers[col]) ? headers[col] : "";
            _editRow = -1; _editCol = col;

            var tb = CreateEditBox(tl, br, text);
            tb.KeyDown += (_, e) =>
            {
                switch (e.KeyCode)
                {
                    case Keys.Return: CommitEdit(tb.Text, canvas); e.Handled = e.SuppressKeyPress = true; break;
                    case Keys.Tab:    CommitEdit(tb.Text, canvas); TabToNextHeader(canvas, col); e.Handled = e.SuppressKeyPress = true; break;
                    case Keys.Escape: CancelCellEdit(canvas);      e.Handled = e.SuppressKeyPress = true; break;
                }
            };
            tb.LostFocus += (_, __) => { if (_activeTextBox == tb) CommitEdit(tb.Text, canvas); };
            _activeTextBox = tb;
            canvas.Controls.Add(tb);
            tb.Focus();
        }

        private void TabToNextHeader(GH_Canvas canvas, int col)
        {
            var (_, cols) = GetDimensions();
            BeginHeaderEdit(canvas, col + 1 < cols ? col + 1 : 0);
        }

        private static TextBox CreateEditBox(PointF tl, PointF br, string text)
        {
            var tb = new TextBox
            {
                Text        = text,
                BorderStyle = BorderStyle.FixedSingle,
                Location    = new Point((int)tl.X, (int)tl.Y),
                Size        = new Size(Math.Max(80, (int)(br.X - tl.X)),
                                       Math.Max(20, (int)(br.Y - tl.Y))),
            };
            tb.SelectAll();
            return tb;
        }

        private void CommitEdit(string text, GH_Canvas canvas)
        {
            var tb = _activeTextBox;
            if (tb == null) return;
            _activeTextBox = null;
            canvas.Controls.Remove(tb);
            tb.Dispose();
            if (_editRow < 0)
                Table.SetColumnHeader(_editCol, text);
            else
                Table.SetCell(_editRow, _editCol, ParseCell(text));
        }

        private void CancelCellEdit(GH_Canvas canvas)
        {
            var tb = _activeTextBox;
            if (tb == null) return;
            _activeTextBox = null;
            canvas.Controls.Remove(tb);
            tb.Dispose();
        }

        private static IGH_Goo ParseCell(string text)
        {
            if (string.IsNullOrEmpty(text)) return new GH_String("");
            if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out double d))
                return new GH_Number(d);
            if (bool.TryParse(text, out bool b))
                return new GH_Boolean(b);
            return new GH_String(text);
        }
    }
}
