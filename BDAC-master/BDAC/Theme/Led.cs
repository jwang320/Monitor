﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BDAC.Theme
{
    public class Led : Control
    {
        #region Public and Private Members

        private Color _color;
        private bool _on = true;
        private readonly Color _reflectionColor = Color.FromArgb(180, 255, 255, 255);
        private readonly Color[] _surroundColor = { Color.FromArgb(0, 255, 255, 255) };
        private readonly Timer _timer = new Timer();

        /// <summary>
        /// Gets or Sets the color of the LED light
        /// </summary>
        [DefaultValue(typeof(Color), "153, 255, 54")]
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                DarkColor = ControlPaint.Dark(_color);
                DarkDarkColor = ControlPaint.DarkDark(_color);
                Invalidate();  // Redraw the control
            }
        }

        /// <summary>
        /// Dark shade of the LED color used for gradient
        /// </summary>
        public Color DarkColor { get; protected set; }

        /// <summary>
        /// Very dark shade of the LED color used for gradient
        /// </summary>
        public Color DarkDarkColor { get; protected set; }

        /// <summary>
        /// Gets or Sets whether the light is turned on
        /// </summary>
        public bool On
        {
            get { return _on; }
            set { _on = value; Invalidate(); }
        }

        #endregion

        #region Constructor

        public Led()
        {
            SetStyle(ControlStyles.DoubleBuffer
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint
            | ControlStyles.SupportsTransparentBackColor, true);

            Color = Color.FromArgb(255, 153, 255, 54);
            _timer.Tick += (sender, e) => { On = !On; };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the Paint event for this UserControl
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Create an off screen graphics object for double buffering
            var offScreenBmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            using (var g = Graphics.FromImage(offScreenBmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                // Draw the control
                DrawControl(g, On);
                // Draw the image to the screen
                e.Graphics.DrawImageUnscaled(offScreenBmp, 0, 0);
            }
        }

        /// <summary>
        /// Renders the control to an image
        /// </summary>
        private void DrawControl(Graphics g, bool on)
        {
            // Is the bulb on or off
            var lightColor = (on) ? Color : Color.FromArgb(150, DarkColor);
            var darkColor = (on) ? DarkColor : DarkDarkColor;

            // Calculate the dimensions of the bulb
            var width = Width - (Padding.Left + Padding.Right);
            var height = Height - (Padding.Top + Padding.Bottom);
            // Diameter is the lesser of width and height
            var diameter = Math.Min(width, height);
            // Subtract 1 pixel so ellipse doesn't get cut off
            diameter = Math.Max(diameter - 1, 1);

            // Draw the background ellipse
            var rectangle = new Rectangle(Padding.Left, Padding.Top, diameter, diameter);
            g.FillEllipse(new SolidBrush(darkColor), rectangle);

            // Draw the glow gradient
            var path = new GraphicsPath();
            path.AddEllipse(rectangle);
            var pathBrush = new PathGradientBrush(path)
            {
                CenterColor = lightColor,
                SurroundColors = new[] { Color.FromArgb(0, lightColor) }
            };
            g.FillEllipse(pathBrush, rectangle);

            // Draw the white reflection gradient
            var offset = Convert.ToInt32(diameter * .15F);
            var diameter1 = Convert.ToInt32(rectangle.Width * .8F);
            var whiteRect = new Rectangle(rectangle.X - offset, rectangle.Y - offset, diameter1, diameter1);
            var path1 = new GraphicsPath();
            path1.AddEllipse(whiteRect);
            var pathBrush1 = new PathGradientBrush(path)
            {
                CenterColor = _reflectionColor,
                SurroundColors = _surroundColor
            };
            g.FillEllipse(pathBrush1, whiteRect);

            // Draw the border
            g.SetClip(ClientRectangle);
            if (On) g.DrawEllipse(new Pen(Color.FromArgb(85, Color.Black), 1F), rectangle);
        }

        /// <summary>
        /// Causes the Led to start blinking
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to blink for. 0 stops blinking</param>
        public void Blink(int milliseconds)
        {
            if (milliseconds > 0)
            {
                On = true;
                _timer.Interval = milliseconds;
                _timer.Enabled = true;
            }
            else
            {
                _timer.Enabled = false;
                On = false;
            }
        }

        #endregion
    }
}
