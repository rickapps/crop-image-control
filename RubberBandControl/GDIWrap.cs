// ----------------------------------------------------
// <copyright file="GDIWrap.cs" company="StellerJay Enterprises, LLC." >
// Copyright (C) 2010 Rick A. Eichhorn. All rights reserved.
// Substitute for DrawReversableFrame - that method caused lots of flickering. So we are using GDI methods
// in this program instead to create the cropping rectangle.
// </copyright>
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StellerJay.FitToList
{
    #region Enumerations
    public enum RasterOps
    {
        R2_BLACK = 1,
        R2_NOTMERGEPEN,
        R2_MASKNOTPEN,
        R2_NOTCOPYPEN,
        R2_MASKPENNOT,
        R2_NOT,
        R2_XORPEN,
        R2_NOTMASKPEN,
        R2_MASKPEN,
        R2_NOTXORPEN,
        R2_NOP,
        R2_MERGENOTPEN,
        R2_COPYPEN,
        R2_MERGEPENNOT,
        R2_MERGEPEN,
        R2_WHITE,
        R2_LAST
    }

    public enum BrushStyles
    {
        BS_SOLID = 0,            
        BS_NULL = 1,             
        BS_HATCHED = 2,          
        BS_PATTERN = 3,          
        BS_INDEXED = 4,          
        BS_DIBPATTERN = 5,      
        BS_DIBPATTERNPT = 6,     
        BS_PATTERN8X8 = 7,       
        BS_MONOPATTERN = 9
    }

    public enum PenStyles
    {
        PS_SOLID = 0,
        PS_DASH = 1,
        PS_DOT = 2,
        PS_DASHDOT = 3,
        PS_DASHDOTDOT = 4
    }
    #endregion

    public sealed class GDIWrap : IDisposable
    {
        #region Variables
        private Color _borderColor;
        private Color _fillColor;
        private int _lineWidth;
        private IntPtr _hdc, _oldBrush, _oldPen, _gdiPen, _gdiBrush;
        private BrushStyles _brushStyle;
        private PenStyles _penStyle;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GDIWrap class.
        /// </summary>
        public GDIWrap()
        {   // Set up for XOR drawing to begin with
            this._borderColor = Color.Transparent;
            this._fillColor = Color.Black;
            this._lineWidth = 2;
            this._brushStyle = BrushStyles.BS_NULL;
            this._penStyle = PenStyles.PS_SOLID;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current BrushColor
        /// </summary>
        public Color BrushColor
        {
            get { return _fillColor; }

            set { _fillColor = value; }
        }

        /// <summary>
        /// Gets or sets the current BrushStyle.  Set to BS_NULL for no brush.
        /// </summary>
        public BrushStyles BrushStyle
        {
            get { return _brushStyle; }

            set { _brushStyle = value; }
        }

        /// <summary>
        /// Gets or sets the current PenColor.  Set to Color.Transparent for a XOR line.
        /// </summary>
        public Color PenColor
        {
            get { return _borderColor; }

            set { _borderColor = value; }
        }

        /// <summary>
        /// Gets or sets the current PenStyle.
        /// </summary>
        public PenStyles PenStyle
        {
            get { return _penStyle; }

            set { _penStyle = value; }
        }

        /// <summary>
        /// Gets or sets the current PenWidth.
        /// </summary>
        public int PenWidth
        {
            get { return _lineWidth; }

            set { _lineWidth = value; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Draws a line with the pen that has been set by the user.  Uses gdi32->MoveToEx and gdi32->LineTo
        /// </summary>
        /// <param name="g">Graphics object.  You can use CreateGraphics().</param>
        /// <param name="p1">Initial point of line.</param>
        /// <param name="p2">Termination point of line.</param>
        public void DrawLine(Graphics g, Point p1, Point p2)
        {
            InitPenAndBrush(g);
            NativeMethods.MoveToEx(_hdc, p1.X, p1.Y, (IntPtr)null);
            NativeMethods.LineTo(_hdc, p2.X, p2.Y);
            Dispose(g);
        }

        /// <summary>
        /// Draws a rectangle with the pen and brush that have been set by the user.  Uses gdi32->Rectangle
        /// </summary>
        /// <param name="g">Graphics object.  You can use CreateGraphics().</param>
        /// <param name="myRect">The shape to draw.</param>
        public void DrawRectangle(Graphics g, Rectangle myRect)
        {
            InitPenAndBrush(g);
            NativeMethods.Rectangle(_hdc, myRect.Left, myRect.Top, myRect.Right, myRect.Bottom);
            Dispose(g);
        }

        /// <summary>
        /// Draws an ellipse with the pen and brush that have been set by the user.  Uses gdi32->Ellipse
        /// </summary>
        /// <param name="g">Graphics object.  You can use CreateGraphics().</param>
        /// <param name="p1">First corner of ellipse (if you imagine its size as a rectangle).</param>
        /// <param name="p2">Second corner of ellipse (if you imagine its size as a rectangle).</param>
        public void DrawEllipse(Graphics g, Point p1, Point p2)
        {
            InitPenAndBrush(g);
            NativeMethods.Ellipse(_hdc, p1.X, p1.Y, p2.X, p2.Y);
            Dispose(g);
        }

        public void Dispose()
        {
        }

        private int GetRGBFromColor(Color fromColor)
        {
            return fromColor.ToArgb() & 0xFFFFFF;
        }

        /// <summary>
        /// Initializes the pen and brush objects.  Stores the old pen and brush so they can be recovered later.
        /// </summary>
        /// <param name="g"></param>
        private void InitPenAndBrush(Graphics g)
        {
            _hdc = g.GetHdc();
            _gdiPen = NativeMethods.CreatePen(_penStyle, _lineWidth, GetRGBFromColor(PenColor));
            _gdiBrush = NativeMethods.GetStockObject(5); // CreateSolidBrush(GetRGBFromColor(fillColor));
            if (PenColor == Color.Transparent)
                NativeMethods.SetROP2(_hdc, (int)RasterOps.R2_XORPEN);
            _oldPen = NativeMethods.SelectObject(_hdc, _gdiPen);
            _oldBrush = NativeMethods.SelectObject(_hdc, _gdiBrush);
        }

        /// <summary>
        /// Reloads the old pen and brush.
        /// Deletes the pen that was created by InitPenAndBrush(g).
        /// Releases the handle to the device context and then disposes of the Graphics object.
        /// </summary>
        /// <param name="g"></param>
        private void Dispose(Graphics g)
        {
            NativeMethods.SelectObject(_hdc, _oldBrush);
            NativeMethods.SelectObject(_hdc, _oldPen);
            NativeMethods.DeleteObject(_gdiPen);
            NativeMethods.DeleteObject(_gdiBrush);
            g.ReleaseHdc(_hdc);
        }

        #endregion
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern bool Ellipse(IntPtr hdc, int x1, int y1, int x2, int y2);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern bool Rectangle(IntPtr hdc, int X1, int Y1, int X2, int Y2);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern IntPtr MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern bool LineTo(IntPtr hdc, int x, int y);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern IntPtr CreatePen(PenStyles enPenStyle, int nWidth, int crColor);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern IntPtr CreateSolidBrush(int crColor);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern bool DeleteObject(IntPtr hObject);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern IntPtr GetStockObject(int brStyle);
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            internal static extern int SetROP2(IntPtr hdc, int enDrawMode);
        }
    }
}
