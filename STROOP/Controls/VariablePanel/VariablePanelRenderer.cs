using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Core.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;

namespace STROOP.Controls.VariablePanel
{
    partial class VariablePanel
    {
        class VariablePanelRenderer : Control
        {
            class OnDemand<T> where T : IDisposable
            {
                readonly Func<T> factory;
                Wrapper<OnDemandCall> disposeContext, invalidateContext;

                public OnDemand(Wrapper<OnDemandCall> disposeContext, Func<T> factory, Wrapper<OnDemandCall> invalidateContext = null)
                {
                    this.factory = factory;
                    this.disposeContext = disposeContext;
                    this.invalidateContext = invalidateContext;
                    if (invalidateContext != null)
                        invalidateContext.value += InvalidateValue;
                }

                T _value;
                bool valueCreated = false;

                public T Value
                {
                    get
                    {
                        if (!valueCreated)
                        {
                            _value = factory();
                            valueCreated = true;
                            disposeContext.value += DisposeValue;
                        }

                        return _value;
                    }
                }

                void InvalidateValue()
                {
                    _value?.Dispose();
                    valueCreated = false;
                }

                void DisposeValue()
                {
                    disposeContext.value -= DisposeValue;
                    if (invalidateContext != null)
                        invalidateContext.value -= InvalidateValue;
                    _value?.Dispose();
                }

                public static implicit operator T(OnDemand<T> obj) => obj.Value;
            }

            class VariableControlRenderData
            {
                public Vector2 positionInGrid;
                public Vector2 positionWhileMoving;
                public bool moving = false;
                public float nameTextOffset;
                public float nameTextLength;
                public string lastRenderedNameText;
            }

            private static readonly Image _pinnedImage = Properties.Resources.img_pin;

            Dictionary<WinFormsVariableControl, VariableControlRenderData> renderDatas = new Dictionary<WinFormsVariableControl, VariableControlRenderData>();

            VariableControlRenderData GetRenderData(WinFormsVariableControl ctrl)
            {
                VariableControlRenderData result;
                if (!renderDatas.TryGetValue(ctrl, out result))
                    renderDatas[ctrl] = result = new VariableControlRenderData();
                return result;
            }

            static int idleRefreshMilliseconds = 250;
            DateTime lastRefreshed;

            delegate void OnDemandCall();

            Wrapper<OnDemandCall> OnDispose = new Wrapper<OnDemandCall>(), OnInvalidateFonts = new Wrapper<OnDemandCall>();

            BufferedGraphics bufferedGraphics = null;

            OnDemand<Font> boldFont;
            OnDemand<Pen> cellBorderPen, cellSeparatorPen, insertionMarkerPen;
            OnDemand<StringFormat> rightAlignFormat;

            VariablePanel target;

            public int borderMargin { get; private set; } = 2;
            static int elementMarginTopBottom => (int)(uint)SavedSettingsConfig.WatchVarPanelVerticalMargin;
            static int elementMarginLeftRight => (int)(uint)SavedSettingsConfig.WatchVarPanelHorizontalMargin;
            public int elementHeight => (int)(Font.Height + 2 * elementMarginTopBottom);

            int elementNameWidth => target.elementNameWidth ?? (int)(uint)SavedSettingsConfig.WatchVarPanelNameWidth;
            int elementValueWidth => target.elementValueWidth ?? (int)(uint)SavedSettingsConfig.WatchVarPanelValueWidth;
            int elementWidth => (int)(elementNameWidth + elementValueWidth);

            public int GetMaxRows()
            {
                var effectiveHeight = target.Height - borderMargin * 2;
                var totalVariableCount = target.GetCurrentlyVisibleCells().Count;
                var knolz = Math.Max(1, effectiveHeight / elementHeight);
                var numColumnsWithoutScrollbar = totalVariableCount / knolz;
                if (totalVariableCount % knolz > 0)
                    numColumnsWithoutScrollbar++;
                if (numColumnsWithoutScrollbar * (SavedSettingsConfig.WatchVarPanelNameWidth + SavedSettingsConfig.WatchVarPanelValueWidth) >
                    target.ClientRectangle.Width - 2 * borderMargin)
                    effectiveHeight -= SystemInformation.HorizontalScrollBarHeight;
                return Math.Max(1, effectiveHeight / elementHeight);
            }

            public VariablePanelRenderer(VariablePanel target)
            {
                this.target = target;
                boldFont = new OnDemand<Font>(
                    OnDispose,
                    () => Font.FontFamily.IsStyleAvailable(FontStyle.Bold) ? new Font(Font, FontStyle.Bold) : Font,
                    OnInvalidateFonts);

                cellBorderPen = new OnDemand<Pen>(OnDispose, () => new Pen(Color.Gray, 2));
                cellSeparatorPen = new OnDemand<Pen>(OnDispose, () => new Pen(Color.Gray, 1));
                insertionMarkerPen = new OnDemand<Pen>(OnDispose, () => new Pen(Color.Blue, 3));
                rightAlignFormat = new OnDemand<StringFormat>(OnDispose, () => new StringFormat() { Alignment = StringAlignment.Far });
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                bufferedGraphics.Render(e.Graphics);
            }

            public void Draw()
            {
                //Return if not focused and recently enough refreshed to save CPU
                var form = FindForm();
                if (form != Form.ActiveForm && (DateTime.Now - lastRefreshed).TotalMilliseconds < idleRefreshMilliseconds)
                    return;

                var displayedWatchVars = target.GetCurrentlyVisibleCells();
                var varNameFont = SavedSettingsConfig.WatchVarPanelBoldNames ? boldFont : Font;

                lastRefreshed = DateTime.Now;
                var searchForm = (form as StroopMainForm)?.searchVariableDialog;
                bool SearchHighlight(string text) => searchForm?.IsMatch(text) ?? false;

                int maxRows = GetMaxRows();
                var newRect = new Rectangle(0, 0, ((displayedWatchVars.Count - 1) / maxRows + 1) * elementWidth + borderMargin * 2, maxRows * elementHeight + borderMargin * 2);
                if (bufferedGraphics == null || Bounds.Width != newRect.Width || Bounds.Height != newRect.Height)
                {
                    bufferedGraphics?.Dispose();
                    target.HorizontalScroll.Value = 0;
                    Bounds = newRect;
                    bufferedGraphics = BufferedGraphicsManager.Current.Allocate(CreateGraphics(), new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));
                    bufferedGraphics.Graphics.TranslateTransform(borderMargin, borderMargin);
                }

                var g = bufferedGraphics.Graphics;
                var visibleRegion = new Rectangle(
                    target.HorizontalScroll.Value - borderMargin,
                    -borderMargin,
                    target.ClientRectangle.Width,
                    target.ClientRectangle.Height);

                void DrawFixImage(IWinFormsVariableCell cell, int baseX, int baseY)
                {
                    var xCoord = baseX + 2;
                    var iconHeight = elementHeight - elementMarginTopBottom * 2;
                    if (cell.memory?.fixedAddresses ?? false)
                        g.DrawImage(_pinnedImage,
                            new Rectangle(
                                xCoord,
                                baseY + elementMarginTopBottom,
                                (int)(iconHeight * (_pinnedImage.Width / (float)_pinnedImage.Height)),
                                iconHeight)
                        );
                }

                void DrawGrid()
                {
                    int x = 0, y = 0;
                    int iterator = 0;
                    var lastColumn = -1;
                    if (maxRows == 0)
                        return;

                    var cursorPos = PointToClient(Cursor.Position);
                    cursorPos.X -= borderMargin;
                    cursorPos.Y -= borderMargin;

                    void ResetIterators()
                    {
                        iterator = 0;
                        lastColumn = -1;
                    }

                    void GetColumn(int offset, int width, bool clip = true)
                    {
                        y = iterator % maxRows;
                        x = iterator / maxRows;
                        if (x > lastColumn)
                        {
                            lastColumn = x;
                            if (clip)
                                g.Clip = new Region(new Rectangle(x * elementWidth + offset, 0, width, Height));
                        }

                        iterator++;
                    }

                    g.ResetClip();
                    ResetIterators();
                    foreach (var cell in displayedWatchVars)
                    {
                        var ctrlData = GetRenderData(cell.control);

                        GetColumn(0, elementNameWidth, false);
                        var yCoord = y * elementHeight;
                        ctrlData.positionInGrid = new Vector2(x * elementWidth, yCoord);
                        if (!ctrlData.moving)
                            ctrlData.positionWhileMoving = ctrlData.positionInGrid;

                        //Skip if invisible
                        if (x * elementWidth > visibleRegion.Right ||
                            (x + 1) * elementWidth < visibleRegion.Left ||
                            yCoord > visibleRegion.Bottom ||
                            yCoord + elementHeight < visibleRegion.Top)
                            continue;

                        var c = cell.control.IsSelected ? Color.Blue : cell.control.currentColor;
                        if (c != Parent.BackColor)
                            using (var brush = new SolidBrush(c))
                                g.FillRectangle(brush, x * elementWidth, yCoord, elementWidth, elementHeight);
                    }

                    ResetIterators();
                    foreach (var cell in displayedWatchVars)
                    {
                        GetColumn(0, elementNameWidth);
                        var yCoord = y * elementHeight;

                        //Skip if invisible
                        if (x * elementWidth > visibleRegion.Right ||
                            x * elementWidth + elementNameWidth < visibleRegion.Left ||
                            yCoord > visibleRegion.Bottom ||
                            yCoord + elementHeight < visibleRegion.Top)
                            continue;

                        var txtPoint = new Point(x * elementWidth + elementMarginLeftRight, yCoord + elementMarginTopBottom);
                        var ctrlData = GetRenderData(cell.control);

                        if (cursorPos.Y > yCoord && cursorPos.Y < yCoord + elementHeight &&
                            cursorPos.X > x * elementWidth && cursorPos.X < x * elementWidth + elementNameWidth) //Cursor is hovering over the name field
                        {
                            if (ctrlData.lastRenderedNameText != cell.control.VarName)
                            {
                                var tmp = g.PageUnit;
                                g.PageUnit = GraphicsUnit.Pixel;
                                ctrlData.nameTextLength = g.MeasureString(cell.control.VarName, varNameFont).Width;
                                g.PageUnit = tmp;
                                ctrlData.lastRenderedNameText = cell.control.VarName;
                            }

                            var overEdge = ctrlData.nameTextLength - (elementNameWidth - elementMarginLeftRight * 2);
                            if (overEdge > 0)
                            {
                                ctrlData.nameTextOffset += (float)Config.CoreLoop.lastFrameTime * elementHeight;
                                if (ctrlData.nameTextOffset > overEdge + elementHeight * 2)
                                    ctrlData.nameTextOffset = 0;
                                txtPoint.X -= (int)Math.Max(0, Math.Min(overEdge, ctrlData.nameTextOffset - elementHeight));
                            }
                        }
                        else
                            ctrlData.nameTextOffset = 0;

                        g.DrawString(cell.control.VarName, varNameFont, cell.control.IsSelected ? Brushes.White : Brushes.Black, txtPoint);
                    }

                    ResetIterators();
                    foreach (var cell in displayedWatchVars)
                    {
                        GetColumn(elementNameWidth, elementValueWidth);

                        //Skip if invisible
                        var yCoord = y * elementHeight;
                        if (x * elementWidth + elementNameWidth > visibleRegion.Right ||
                            (x + 1) * elementWidth < visibleRegion.Left ||
                            yCoord > visibleRegion.Bottom ||
                            yCoord + elementHeight < visibleRegion.Top)
                            continue;

                        if (cell.CustomDrawOperation != null)
                            cell.CustomDrawOperation(
                                new VariablePanelUiContext(this, g,
                                    new Rectangle(
                                        x * elementWidth + elementNameWidth,
                                        y * elementHeight,
                                        elementValueWidth,
                                        elementHeight))
                            );
                        else
                        {
                            var txtPoint = new Point((x + 1) * elementWidth - elementMarginLeftRight, yCoord + elementMarginTopBottom);
                            g.DrawString(cell.GetValueText(), Font, cell.control.IsSelected ? Brushes.White : Brushes.Black, txtPoint, rightAlignFormat);
                        }

                        DrawFixImage(cell, x * elementWidth + elementNameWidth, yCoord);
                    }

                    g.ResetClip();

                    var numRows = x == 0 ? (y + 1) : maxRows;
                    var maxY = numRows * elementHeight;
                    var maxX = elementWidth * (x + 1);

                    if (y == maxRows - 1)
                        x++;
                    for (int dx = 0; dx <= x; dx++)
                    {
                        var xCoord = dx * elementWidth;
                        g.DrawLine(cellBorderPen, xCoord, 0, xCoord, maxY);
                        xCoord += elementNameWidth;
                        if (dx < x)
                            g.DrawLine(cellSeparatorPen, xCoord, 0, xCoord, maxY);
                    }

                    if (y != maxRows - 1)
                    {
                        var yCoord = (y + 1) * elementHeight;
                        g.DrawLine(cellBorderPen, maxX, 0, maxX, yCoord);
                        var xCoord = maxX - elementWidth + elementNameWidth;
                        g.DrawLine(cellSeparatorPen, xCoord, 0, xCoord, yCoord);
                    }

                    for (int dy = 0; dy <= numRows; dy++)
                    {
                        var yCoord = dy * elementHeight;
                        g.DrawLine(cellBorderPen, 0, yCoord, dy <= (y + 1) ? maxX : maxX - elementWidth, yCoord);
                    }

                    ResetIterators();
                    using (var highlightBrush = new SolidBrush(Color.FromArgb(0x40, Color.Blue)))
                    {
                        foreach (var ctrl in displayedWatchVars)
                        {
                            GetColumn(elementNameWidth, elementValueWidth, false);
                            if (ctrl.control.Highlighted)
                                using (var pen = new Pen(ctrl.control.HighlightColor, 3))
                                    g.DrawRectangle(pen, x * elementWidth, y * elementHeight, elementWidth, elementHeight);
                            if (SearchHighlight(ctrl.control.VarName))
                                g.FillRectangle(highlightBrush, x * elementWidth + 2, y * elementHeight + 2, elementWidth - 4, elementHeight - 4);
                        }
                    }
                }

                void DrawMovingVariables()
                {
                    if (target._reorderingWatchVarControls.Count == 0)
                        return;

                    var pt = PointToClient(Cursor.Position);
                    var cursorPosition = new Vector2(pt.X, pt.Y);
                    (var insertionIndex, _, _) = GetVariableAt(pt);
                    if (insertionIndex < 0)
                        insertionIndex = displayedWatchVars.Count;
                    var x = insertionIndex / maxRows;
                    var y = insertionIndex % maxRows;
                    g.DrawLine(insertionMarkerPen, x * elementWidth, y * elementHeight, (x + 1) * elementWidth, y * elementHeight);

                    int i = 0;
                    foreach (var cell in target._reorderingWatchVarControls)
                    {
                        var ctrlData = GetRenderData(cell.control);
                        using (var brush = new SolidBrush(cell.control.currentColor))
                        {
                            g.FillRectangle(brush, ctrlData.positionWhileMoving.X, ctrlData.positionWhileMoving.Y, elementWidth, elementHeight);
                        }

                        var yCoord = (int)ctrlData.positionWhileMoving.Y + elementMarginTopBottom;


                        g.Clip = new Region(
                            new Rectangle(
                                (int)ctrlData.positionWhileMoving.X,
                                (int)ctrlData.positionWhileMoving.Y,
                                elementNameWidth,
                                elementHeight));
                        var txtPoint = new Point((int)ctrlData.positionWhileMoving.X + elementMarginLeftRight, yCoord + elementMarginTopBottom);
                        g.DrawString(cell.control.VarName, varNameFont, Brushes.Black, txtPoint);

                        var valueX = (int)ctrlData.positionWhileMoving.X + elementNameWidth;
                        g.Clip = new Region(
                            new Rectangle(
                                valueX,
                                (int)ctrlData.positionWhileMoving.Y,
                                elementValueWidth,
                                elementHeight));
                        txtPoint = new Point((int)ctrlData.positionWhileMoving.X + elementWidth - elementMarginLeftRight, yCoord + elementMarginTopBottom);
                        g.DrawString(cell.GetValueText(), Font, Brushes.Black, txtPoint, rightAlignFormat);
                        DrawFixImage(cell, valueX, yCoord);

                        g.ResetClip();

                        var xCoord = (int)ctrlData.positionWhileMoving.X + elementNameWidth;
                        g.DrawLine(cellSeparatorPen, xCoord, yCoord, xCoord, yCoord + elementHeight);
                        g.DrawRectangle(cellBorderPen, ctrlData.positionWhileMoving.X, ctrlData.positionWhileMoving.Y, elementWidth, elementHeight);

                        var target = cursorPosition;
                        target.Y += elementHeight * i++;
                        ctrlData.positionWhileMoving += (target - ctrlData.positionWhileMoving) * Utilities.MoreMath.EaseIn(10 * (float)Config.CoreLoop.lastFrameTime);
                        if ((ctrlData.positionInGrid - ctrlData.positionWhileMoving).LengthSquared < 1)
                            ctrlData.moving = false;
                        else
                            ctrlData.moving = true;
                    }
                }

                g.Clear(Parent.BackColor);
                DrawGrid();
                DrawMovingVariables();

                if (Controls.Count == 0)
                    bufferedGraphics.Render();
            }

            public Rectangle GetVariableControlBounds(int index)
            {
                var maxRows = GetMaxRows();
                var x = index / maxRows;
                var y = index % maxRows;
                return new Rectangle(
                    borderMargin + x * elementWidth + elementNameWidth,
                    borderMargin + y * elementHeight,
                    elementValueWidth,
                    elementHeight);
            }

            public (int index, IWinFormsVariableCell cell, bool select) GetVariableAt(Point location)
            {
                location.X -= borderMargin;
                location.Y -= borderMargin;
                if (location.X < 0 || location.Y < 0)
                    return (-1, null, false);
                var x = location.X / elementWidth;
                var y = location.Y / elementHeight;
                int maxRows = GetMaxRows();
                if (y >= maxRows)
                    return (-1, null, false);
                int index = x * maxRows + y;
                if (index < 0)
                    return (0, null, false);
                var displayedWatchVars = target.GetCurrentlyVisibleCells();
                if (index < displayedWatchVars.Count)
                    return (index, displayedWatchVars[index], (location.X % elementWidth) < elementNameWidth);

                return (-1, null, false);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    OnDispose.value?.DynamicInvoke();
                base.Dispose(disposing);
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                OnInvalidateFonts.value?.DynamicInvoke();
            }
        }
    }
}
