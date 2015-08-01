// ----------------------------------------------------
// <copyright file="CropperBox.cs" company="StellerJay Enterprises, LLC." >
// Copyright (C) 2010 Rick A. Eichhorn. 
// </copyright>
// <description>
// CropperBox is a user control to manage the cropping rectangle. The control consists of a picturebox within
// a panel. Use the Image property to get or set the picturebox image.
// </description>
// ----------------------------------------------------

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RickApps.CropImage
{
    public enum Operation
    {
        none,
        draw,
        move,
        left,
        top,
        right,
        bottom,
        topLeft,
        topRight,
        bottomRight,
        bottomLeft
    }

    /// <summary>
    /// Manages the cropping rectangle - allows drawing, resizing, and moving. _theRectangle and _backupRectangle are stored 
    /// in units of pixels.
    /// </summary>
    public partial class RubberBand : UserControl
    {
        #region Member Variables;

        private Operation _curOp;                               // Indicates the current operation
        private bool _isDragging = false;                       // Indicates if the left mouse button is down
        private Point _startPoint;                              // Set in the mouse down event. The anchor for our rectangle
        private bool _disabled = false;                         // Indicates cropping is not allowed 
        private Rectangle _theRectDrawing = new Rectangle(      // We draw this rectangle on the screen as our cropping rectangle
            new Point(0, 0), new Size(0, 0));                   // It is populated in the MouseUp event

        private Rectangle _backupRectangle = new Rectangle(     // Helps us handle double click event - we need to restore the rectangle
            new Point(0, 0), new Size(0, 0));                   // because double click also triggers a single click event.

        private Rectangle _picRectangle = new Rectangle(         // The area of our image - not the same as area of PictureBox control
            new Point(0, 0), new Size(0, 0));

        private GDIWrap _myGDI = new GDIWrap();                 // DrawReversibleFrame caused a lot of flickering - so we use GDI methods
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CropperBox class
        /// </summary>
        public RubberBand()
        {
            InitializeComponent();
        }
        #endregion

        public event EventHandler ImageCropped;

        public event EventHandler<StatusEventArg> CursorMove;

        #region Public Properties and Methods
        /// <summary>
        /// Gets or sets how images will be sized in the control - true sized or sized to fit the screen
        /// </summary>
        [Category("Custom"), Description("Size mode of the picturebox control")]
        public PictureBoxSizeMode SizeMode
        {
            get
            {
                return thePicture.SizeMode;
            }

            set
            {
                Rectangle origSize = ImageRect();              // This defines the current image rectangle
                // Set docking style before we set size mode
                // Otherwise we have repaint problems if pic was scrolled.
                if (value == PictureBoxSizeMode.AutoSize)
                    thePicture.Dock = DockStyle.None;
                else
                    thePicture.Dock = DockStyle.Fill;
                thePicture.SizeMode = value;
                _picRectangle = ImageRect();

                // Now we can transform our cropping rectangle
                _theRectDrawing.Height = (int)Math.Round(_theRectDrawing.Height * (float)_picRectangle.Height / origSize.Height);
                _theRectDrawing.Width = (int)Math.Round(_theRectDrawing.Width * (float)_picRectangle.Width / origSize.Width);

                // Get the offset for the cropping rectangle
                Point offset = new Point(_theRectDrawing.X - origSize.X, _theRectDrawing.Y - origSize.Y);

                // Scale the offset
                offset.X = (int)Math.Round(offset.X * (float)_picRectangle.Width / origSize.Width);
                offset.Y = (int)Math.Round(offset.Y * (float)_picRectangle.Height / origSize.Height);

                // Set our new position
                _theRectDrawing.X = offset.X + ImageRect().X;
                _theRectDrawing.Y = offset.Y + ImageRect().Y;
            }
        }

        /// <summary>
        /// Gets or sets the image that will be displayed. When we set a new image, we check if the current
        /// cropping rectangle fits within the image. If not, we reset it to zero.
        /// </summary>
        [Category("Custom"), Description("Gets or sets the image from the picturebox")]
        public Image Image
        {
            get
            {
                return thePicture.Image;
            }

            set
            {
                // Change the image
                thePicture.Image = value;

                // Is the cropping rectangle completely within the new image?
                if (!ImageRect().Contains(_theRectDrawing))
                    _theRectDrawing = new Rectangle(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Gets the image selected by the cropping rectangle. Returns null if no image is selected.
        /// Return the entire image if no cropping rectangle is selected.
        /// </summary>
        [Category("Custom"), Description("Gets the image selected by the cropping rectangle")]
        public Image SelectedImage
        {
            get
            {
                bool tempRect = false;
                // Create a bitmap from our image
                Bitmap bmpImage = new Bitmap(thePicture.Image);

                // If we dont have a cropping rectangle, set the cropping rectangle to the 
                // dimensions of the entire image.
                if (_theRectDrawing.Width <= 0 || _theRectDrawing.Height <= 0)
                {
                    _theRectDrawing = ImageRect();
                    tempRect = true;
                }

                // Scale the cropping rectangle
                Rectangle cropRect = CroppingArea;

                // Clone the image defined by cropRect. Use the same pixel format
                // as our original image.
                Bitmap bmpCrop = bmpImage.Clone(cropRect, bmpImage.PixelFormat);
                bmpImage.Dispose();
                if (tempRect)
                    _theRectDrawing = new Rectangle(0, 0, 0, 0);

                return (Image)bmpCrop;
            }
        }

        /// <summary>
        /// Gets or sets the Disabled property. We disable the control when we display the home page.
        /// </summary>
        [Category("Custom"), Description("Gets or sets the cropping capability of the control")]
        public bool Disabled
        {
            get
            {
                return _disabled;
            }

            set
            {
                _disabled = value;
                if (_disabled)
                    _theRectDrawing = new Rectangle(0, 0, 0, 0);
            }
        }
        #endregion

        #region Private Properties And Methods

        /// <summary>
        /// Gets the cropping area scaled to the specified size
        /// </summary>
        /// <param name="basis"></param>
        /// <returns></returns>
        [Category("Custom"), Description("Obtain the current cropping rectangle scaled to the size of the specified image")]
        private Rectangle CroppingArea
        {
            get
            {
                Rectangle cropRect = _theRectDrawing;
                double ratio = GetScaleFactor(SizeMode == PictureBoxSizeMode.AutoSize);
                if (ratio != 0.0)
                {
                    cropRect.Offset(-_picRectangle.Left, -_picRectangle.Top);
                    cropRect.Height = (int)Math.Round(cropRect.Height / ratio);
                    cropRect.X = (int)Math.Round(cropRect.X / ratio);
                    cropRect.Width = (int)Math.Round(cropRect.Width / ratio);
                    cropRect.Y = (int)Math.Round(cropRect.Y / ratio);
                }

                return cropRect;
            }
        }

        /// <summary>
        /// Update the point based on the image size
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Point CroppingPoint(Point pt)
        {
            Point adjustedPt = pt;
            double ratio = GetScaleFactor(SizeMode == PictureBoxSizeMode.AutoSize);
            if (ratio != 0.0)
            {
                adjustedPt.Offset(-_picRectangle.Left, -_picRectangle.Top);
                adjustedPt.X = (int)Math.Round(adjustedPt.X / ratio);
                adjustedPt.Y = (int)Math.Round(adjustedPt.Y / ratio);
            }

            return adjustedPt;
        }

        /// <summary>
        /// If the cursor is outside a cropping area, start a new rubberband. If the cursor is within the
        /// bounds of a rubber band, trigger a move operation. If the cursor is on the border of a rubber
        /// band, trigger a resize operation. This event is triggered by the PictureBox control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            // Set the isDrag variable to true and get the starting point 
            // by using the PointToScreen method to convert form 
            // coordinates to screen coordinates.
            Point myPoint = new Point(e.X, e.Y);    // PictureBox coordinates are passed in
            if (_disabled)
                _curOp = Operation.none;
            else if (e.Button == MouseButtons.Left)
            {
                this._isDragging = true;
                this._picRectangle = ImageRect();
                this._backupRectangle = _theRectDrawing;           // Helps with undo if this is part of a double click event
                this._curOp = DetermineMouseOperation(myPoint);
            }
            else
            {
                _curOp = Operation.none;
            }

            switch (_curOp)
            {
                case Operation.draw:
                    _startPoint = myPoint;

                    // Reset the rectangle.
                    _theRectDrawing = new Rectangle(0, 0, 0, 0);
                    thePicture.Invalidate();
                    break;
                case Operation.none:
                    _isDragging = false;

                    // Reset the rectangle.
                    _theRectDrawing = new Rectangle(0, 0, 0, 0);
                    thePicture.Invalidate();
                    break;
                default:
                    _startPoint = myPoint;

                    // Erase the inked rectangle
                    thePicture.Refresh();

                    // Draw the rectangle in reverse ink
                    Graphics g = thePicture.CreateGraphics();
                    _myGDI.DrawRectangle(g, _theRectDrawing);
                    g.Dispose();
                    break;
            }
        }

        /// <summary>
        /// If curOp is none, we will set the cursor depending on the current position. Otherwise, 
        /// we will modify or move the rubberand. This event is triggered by the PictureBox control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point myPoint = new Point(e.X, e.Y);     // Picture box coordinates
            Graphics g = thePicture.CreateGraphics();

            // Determine what our current operation is. This depends on if we are dragging the mouse or not
            if (_isDragging)
            {
                _curOp = CheckOperationChange(_curOp, myPoint); // The operation can change during a mouse drag
                // Erase the old rectangle created in the mouse down event
                _myGDI.DrawRectangle(g, _theRectDrawing);
            }
            else
            {
                _curOp = DetermineMouseOperation(myPoint);     // If not dragging, see which cursor to use
            }

            bool isActive = IsActivePoint(ref myPoint);
            switch (_curOp)
            {
                case Operation.draw:
                    // Calculate the endpoint and dimensions for the new 
                    _theRectDrawing = NormalizeRectangle(_startPoint, myPoint);
                    break;
                case Operation.move:
                    Cursor = Cursors.SizeAll;
                    if (_isDragging)
                    {
                        // Compute how much the cursor has moved since the previous call
                        Point myDiff = new Point(myPoint.X - _startPoint.X, myPoint.Y - _startPoint.Y);
                        myDiff = CalcMaxOffset(myDiff);
                        _theRectDrawing.Offset(myDiff);
                        _startPoint = myPoint;
                    }

                    break;
                case Operation.left:
                    Cursor = Cursors.SizeWE;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(myPoint.X, _theRectDrawing.Top, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.top:
                    Cursor = Cursors.SizeNS;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, myPoint.Y, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.right:
                    Cursor = Cursors.SizeWE;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, myPoint.X, _theRectDrawing.Bottom);
                    break;
                case Operation.bottom:
                    Cursor = Cursors.SizeNS;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, _theRectDrawing.Right, myPoint.Y);
                    break;
                case Operation.topLeft:
                    Cursor = Cursors.SizeNWSE;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(myPoint.X, myPoint.Y, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.topRight:
                    Cursor = Cursors.SizeNESW;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, myPoint.Y, myPoint.X, _theRectDrawing.Bottom);
                    break;
                case Operation.bottomRight:
                    Cursor = Cursors.SizeNWSE;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, myPoint.X, myPoint.Y);
                    break;
                case Operation.bottomLeft:
                    Cursor = Cursors.SizeNESW;
                    if (_isDragging)
                        _theRectDrawing = NormalizeRectangle(myPoint.X, _theRectDrawing.Top, _theRectDrawing.Right, myPoint.Y);
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
            }

            // Draw our new rectangle
            if (_isDragging)
                _myGDI.DrawRectangle(g, _theRectDrawing);
            g.Dispose();

            // Notify the host that the mouse is moved. If we have a cropping rectangle,
            // size of the cropping rectangle. Otherwise, report the mouse position
            if (CursorMove != null)
            {
                Rectangle crop = CroppingArea;
                if (crop.Height <= 0 || crop.Width <= 0)
                {
                    crop.Width = thePicture.Image.Width;
                    crop.Height = thePicture.Image.Height;
                    crop.X = 0;
                    crop.Y = 0;
                    // If we are within the picture, report the current mouse position
                    if (isActive)
                        CursorMove(this, new StatusEventArg(CroppingPoint(myPoint), crop));
                    else
                        CursorMove(this, new StatusEventArg(crop.Location, crop));
                }
                else
                    CursorMove(this, new StatusEventArg(crop.Location, crop));
            }
        }

        /// <summary>
        /// We are done with our mouse operation. This event is triggered by
        /// the PictureBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Point myPoint = new Point(e.X, e.Y);
            IsActivePoint(ref myPoint);

            // Erase the current rectangle
            Graphics g = thePicture.CreateGraphics();
            if (_curOp != Operation.none)
                _myGDI.DrawRectangle(g, _theRectDrawing);
            g.Dispose();

            // If the MouseUp event occurs, the user has stopped dragging
            _isDragging = false;

            // Note that some operations can change mid stride depending on where the user moves the mouse
            _curOp = CheckOperationChange(_curOp, myPoint);

            switch (_curOp)
            {
                case Operation.move:
                    myPoint = new Point(myPoint.X - _startPoint.X, myPoint.Y - _startPoint.Y);
                    myPoint = CalcMaxOffset(myPoint);
                    _theRectDrawing.Offset(myPoint);
                    break;
                case Operation.left:
                    _theRectDrawing = NormalizeRectangle(myPoint.X, _theRectDrawing.Top, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.right:
                    _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, myPoint.X, _theRectDrawing.Bottom);
                    break;
                case Operation.top:
                    _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, myPoint.Y, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.bottom:
                    _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, _theRectDrawing.Right, myPoint.Y);
                    break;
                case Operation.topLeft:
                    _theRectDrawing = NormalizeRectangle(myPoint.X, myPoint.Y, _theRectDrawing.Right, _theRectDrawing.Bottom);
                    break;
                case Operation.topRight:
                    _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, myPoint.Y, myPoint.X, _theRectDrawing.Bottom);
                    break;
                case Operation.bottomLeft:
                    _theRectDrawing = NormalizeRectangle(myPoint.X, _theRectDrawing.Top, _theRectDrawing.Right, myPoint.Y);
                    break;
                case Operation.bottomRight:
                    _theRectDrawing = NormalizeRectangle(_theRectDrawing.Left, _theRectDrawing.Top, myPoint.X, myPoint.Y);
                    break;
            }

            if (_curOp != Operation.none)
            {
                // Create our new cropping rectangle - scale it in terms of the current image size
                // The rectangle is currently in pictureBox coordinates. We need to scale it in terms
                // of the current image.
                myPoint.X = _theRectDrawing.Right;
                myPoint.Y = _theRectDrawing.Bottom;
                _theRectDrawing = NormalizeRectangle(_theRectDrawing.Location, myPoint);
                thePicture.Invalidate();
                _curOp = Operation.none;
            }
        }

        /// <summary>
        /// Notify our host that someone double clicked the pictureBox. Keep in mind that a
        /// double click event also causes two mouse down events and one mouse up event prior to
        /// triggering this event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            _theRectDrawing = _backupRectangle;
            _curOp = Operation.none;
            _isDragging = false;
            if (ImageCropped != null)
                ImageCropped(this, e);
        }

        /// <summary>
        /// Paint event of the picturebox control. Draw the rectangle only if not being drawn by
        /// mouse events. The rectangle is drawn using client coordinates of the PictureBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (!_isDragging)
                _myGDI.DrawRectangle(e.Graphics, _theRectDrawing);
        }

        /// <summary>
        /// Fired when the picturebox control is resized. We may also need to resize our cropping rectangle
        /// and refresh the dimensions of picRectangle (thePicture is the PictureBox control). 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResize(object sender, EventArgs e)
        {
            _picRectangle = ImageRect();
        }

        /// <summary>
        /// Create a normalized rectangle from two points. There does not seem to be
        /// any built in method to do this.  The two points represent the top left and bottom
        /// right corners.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Rectangle NormalizeRectangle(Point p1, Point p2)
        {
            Rectangle rc = new Rectangle();

            // Normalize the rectangle.
            if (p1.X < p2.X)
            {
                rc.X = p1.X;
                rc.Width = p2.X - p1.X;
            }
            else
            {
                rc.X = p2.X;
                rc.Width = p1.X - p2.X;
            }

            if (p1.Y < p2.Y)
            {
                rc.Y = p1.Y;
                rc.Height = p2.Y - p1.Y;
            }
            else
            {
                rc.Y = p2.Y;
                rc.Height = p1.Y - p2.Y;
            }

            return rc;
        }

        private Rectangle NormalizeRectangle(int x1, int y1, int x2, int y2)
        {
            Point p1 = new Point(x1, y1);
            Point p2 = new Point(x2, y2);
            return NormalizeRectangle(p1, p2);
        }

        /// <summary>
        /// The amount to change picture size to fit within our panel. If the
        /// image is true sized, return 1. Otherwise return the factor we will
        /// increase/reduce the actual image size.
        /// </summary>
        /// <param name="isTrueSize"></param>
        /// <returns></returns>
        private double GetScaleFactor(bool isTrueSize)
        {
            double ratio = 0.0;
            double ratio1 = 0.0;

            if (isTrueSize)
                ratio = 1.0;   // Image is true sized
            else
            {
                // Get the dimensions of the picture so we can compute our sizing ratio
                if (thePicture.Image != null)
                {
                    ratio = ((float)panel1.Size.Width) / thePicture.Image.Width;
                    ratio1 = ((float)panel1.Size.Height) / thePicture.Image.Height;
                }

                if (ratio1 < ratio)
                    ratio = ratio1;
            }

            return ratio;
        }

        /// <summary>
        /// When the picture is sized to fit, it is also centered inside the panel. 
        /// We need to determine how much it is offset from the 0,0 position.
        /// </summary>
        /// <param name="isTrueSize"></param>
        /// <returns></returns>
        private Point GetScaleOffset(bool isTrueSize)
        {
            Point topLeft = new Point();
            if (thePicture.Image == null)
                return topLeft;

            double ratio = GetScaleFactor(isTrueSize);
            int width = (int)(thePicture.Image.Width * ratio);
            int height = (int)(thePicture.Image.Height * ratio);

            // Get the location of the picture. If not trueSize, it is centered in the panel
            // smaller than the panel.
            if (isTrueSize || width > panel1.Width)
                topLeft.X = 0;
            else
                topLeft.X = (panel1.Width - width) / 2;
            if (isTrueSize || height > panel1.Height)
                topLeft.Y = 0;
            else
                topLeft.Y = (panel1.Height - height) / 2;

            return topLeft;
        }

        /// <summary>
        /// Return the rectangle that defines the current displayed image
        /// in the picture box. Seems there should already be a method to
        /// do this, but I could not find one. thePicture is our PictureBox
        /// control. theImage is the size of the image contained therein.  
        /// The size of the image depends on the current SizeMode - the 
        /// image might be scaled to fit the picturebox.
        /// </summary>
        /// <returns></returns>
        private Rectangle ImageRect()
        {
            double ratio;
            Point topLeft = new Point();
            Rectangle imageRect = new Rectangle();

            if (thePicture.Image == null)
                return imageRect;

            ratio = GetScaleFactor(thePicture.SizeMode == PictureBoxSizeMode.AutoSize);
            topLeft = GetScaleOffset(thePicture.SizeMode == PictureBoxSizeMode.AutoSize);
            imageRect.Width = (int)(thePicture.Image.Width * ratio);
            imageRect.Height = (int)(thePicture.Image.Height * ratio);
            imageRect.Location = topLeft;
            return imageRect;
        }

        /// <summary>
        /// Return true if the point is within the boundaries of the current
        /// picturebox image. If not, modify pt so it is on the offending boundary.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private bool IsActivePoint(ref Point pt)
        {
            bool isActive = false;
            if (_picRectangle.Size.IsEmpty)
                _picRectangle = ImageRect();

            // Is the point within the picture area? The point must be in client coords
            isActive = _picRectangle.Contains(pt);
            if (!isActive)
            {
                // We will adjust the point to make it active
                Rectangle myRect = _picRectangle;
                if (pt.X < myRect.Left)
                    pt.X = myRect.Left;
                if (pt.X > myRect.Right)
                    pt.X = myRect.Right;
                if (pt.Y < myRect.Top)
                    pt.Y = myRect.Top;
                if (pt.Y > myRect.Bottom)
                    pt.Y = myRect.Bottom;
            }

            return isActive;
        }

        /// <summary>
        /// Return the appropriate operation type based on the point passed in
        /// </summary>
        /// <param name="pt">Client coordinates are passed in</param>
        /// <returns></returns>
        private Operation DetermineMouseOperation(Point pt)
        {
            Operation retVal = Operation.none;
            Rectangle outerFrame = _theRectDrawing;
            Rectangle innerFrame = outerFrame;
            Size sz = new Size(2, 2);                // The amount of leeway to give our user moving the mouse
            outerFrame.Inflate(sz);                      // Create an outer rectangle around our cropping area
            innerFrame.Inflate(-sz.Width, -sz.Height);   // Create an inner rectangle within our cropping area

            // Check if we are near any of the rectangle corners. We have to offset these four rectangles because the
            // hotspot on the stock cursor for SizeNESW and SizeNWSE is not in the center of the cursor where it should be.
            Rectangle picCorner;

            // Check for top left corner
            picCorner = new Rectangle(outerFrame.Left, outerFrame.Top, sz.Width, sz.Height);
            picCorner.Inflate(8, 8);
            picCorner.Offset(-8, -8);
            if (picCorner.Contains(pt))
                retVal = Operation.topLeft;

            // Check for top right corner
            picCorner = new Rectangle(innerFrame.Right, outerFrame.Top, sz.Width, sz.Height);
            picCorner.Inflate(8, 8);
            picCorner.Offset(-8, -8);
            if (picCorner.Contains(pt))
                retVal = Operation.topRight;

            // Check for bottom right corner
            picCorner = new Rectangle(innerFrame.Right, innerFrame.Bottom, sz.Width, sz.Height);
            picCorner.Inflate(8, 8);
            picCorner.Offset(-8, -8);
            if (picCorner.Contains(pt))
                retVal = Operation.bottomRight;

            // Check for bottom left corner
            picCorner = new Rectangle(outerFrame.Left, innerFrame.Bottom, sz.Width, sz.Height);
            picCorner.Inflate(8, 8);
            picCorner.Offset(-8, -8);
            if (picCorner.Contains(pt))
                retVal = Operation.bottomLeft;

            // If we are not within any corners, do a little more checking
            if (retVal == Operation.none)
            {
                // Are we inside the outer rectangle?
                if (outerFrame.Contains(pt))
                {
                    // Are we inside the inner rectangle?
                    if (innerFrame.Contains(pt))
                        retVal = Operation.move;
                    else if (pt.Y >= outerFrame.Top && pt.Y <= innerFrame.Top)
                        retVal = Operation.top;
                    else if (pt.Y >= innerFrame.Bottom && pt.Y <= outerFrame.Bottom)
                    {
                        System.Diagnostics.Debug.Print("Outer: " + outerFrame.ToString());
                        System.Diagnostics.Debug.Print("Inner: " + innerFrame.ToString());
                        System.Diagnostics.Debug.Print("Point: " + pt.ToString());
                        retVal = Operation.bottom;
                    }
                    else if (pt.X >= innerFrame.Right && pt.X <= outerFrame.Right)
                        retVal = Operation.right;
                    else if (pt.X >= outerFrame.Left && pt.X <= innerFrame.Left)
                        retVal = Operation.left;
                }
            }

            // Are we inside the picture area?
            if (retVal == Operation.none)
            {
                if (_picRectangle.Contains(pt) && _isDragging)
                    retVal = Operation.draw;
            }

            System.Diagnostics.Debug.Print(retVal.ToString());
            return retVal;
        }

        /// <summary>
        /// The operation can change depending on where the user moves the mouse
        /// </summary>
        /// <param name="curOp"></param>
        /// <param name="myPoint"></param>
        /// <returns></returns>
        private Operation CheckOperationChange(Operation curOp, Point myPoint)
        {
            switch (curOp)
            {
                case Operation.left:
                    if (myPoint.X > _theRectDrawing.Right)
                        curOp = Operation.right;
                    break;
                case Operation.right:
                    if (myPoint.X < _theRectDrawing.Left)
                        curOp = Operation.left;
                    break;
                case Operation.top:
                    if (myPoint.Y > _theRectDrawing.Bottom)
                        curOp = Operation.bottom;
                    break;
                case Operation.bottom:
                    if (myPoint.Y < _theRectDrawing.Top)
                        curOp = Operation.top;
                    break;
                case Operation.topLeft:
                    if ((myPoint.X > _theRectDrawing.Right) && (myPoint.Y > _theRectDrawing.Bottom))
                        curOp = Operation.bottomRight;
                    else if (myPoint.X > _theRectDrawing.Right)
                        curOp = Operation.topRight;
                    else if (myPoint.Y > _theRectDrawing.Bottom)
                        curOp = Operation.bottomLeft;
                    break;
                case Operation.topRight:
                    if ((myPoint.X < _theRectDrawing.Left) && (myPoint.Y > _theRectDrawing.Bottom))
                        curOp = Operation.bottomLeft;
                    else if (myPoint.X < _theRectDrawing.Left)
                        curOp = Operation.topLeft;
                    else if (myPoint.Y > _theRectDrawing.Bottom)
                        curOp = Operation.bottomRight;
                    break;
                case Operation.bottomLeft:
                    if ((myPoint.X > _theRectDrawing.Right) && (myPoint.Y < _theRectDrawing.Top))
                        curOp = Operation.topRight;
                    else if (myPoint.X > _theRectDrawing.Right)
                        curOp = Operation.bottomRight;
                    else if (myPoint.Y < _theRectDrawing.Top)
                        curOp = Operation.topLeft;
                    break;
                case Operation.bottomRight:
                    if ((myPoint.X < _theRectDrawing.Left) && (myPoint.Y < _theRectDrawing.Top))
                        curOp = Operation.topLeft;
                    else if (myPoint.X < _theRectDrawing.Left)
                        curOp = Operation.bottomLeft;
                    else if (myPoint.Y < _theRectDrawing.Top)
                        curOp = Operation.topRight;
                    break;
            }

            return curOp;
        }

        /// <summary>
        /// Move the cropping rectangle - but not out of the picture boundaries
        /// </summary>
        /// <param name="offSet"></param>
        /// <returns></returns>
        private Point CalcMaxOffset(Point offSet)
        {
            Rectangle testRect = _theRectDrawing;

            // Move the rectangle the specified amount and see if Kosher
            testRect.Offset(offSet);

            // Do we need to adjust our offset?
            if (!_picRectangle.Contains(testRect))
            {
                if (testRect.Top < _picRectangle.Top)
                    offSet.Y = offSet.Y + (_picRectangle.Top - testRect.Top);
                if (testRect.Right > _picRectangle.Right)
                    offSet.X = offSet.X + (_picRectangle.Right - testRect.Right);
                if (testRect.Bottom > _picRectangle.Bottom)
                    offSet.Y = offSet.Y + (_picRectangle.Bottom - testRect.Bottom);
                if (testRect.Left < _picRectangle.Left)
                    offSet.X = offSet.X + (_picRectangle.Left - testRect.Left);
            }

            return offSet;
        }

        #endregion
    }
}
