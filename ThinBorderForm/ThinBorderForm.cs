using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rrSoft
{
    //[System.ComponentModel.DesignerCategory("")]
    public class ThinBorderForm : Form
    {
        public ThinBorderForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // set some initial parameters
            //
            this.Name = "ThinBorderForm";
            this.BorderWidth = 4;
            this.CaptionHeight = 24;
            this.CaptionColor = Color.AliceBlue;
            this.CaptionTextWidth = 0;
            //
            this.ResumeLayout(false);

            this.Layout += PropertyChangedHandler;

            // reset any properties incompatible with thin border model
            forceFormProperties();
        }

        private void PropertyChangedHandler(object sender, EventArgs e)
        {
            // reset any properties incompatible with thin border model
            forceFormProperties();
        }

        public int BorderWidth { get; set; }                // Border size
        public int CaptionHeight { get; set; }              // Caption height
        public Color CaptionColor { get; set; }             // Caption Colour
        public int CaptionTextWidth { get; set; }           // ccaption text width -- 0 is auto

        private int actualCaptionTextWidth = 0;

        //
        // force various form properties to be consistent with thinbiorder model, regardless
        // what derived or base classes set
        //
        private void forceFormProperties()
        {
            // prevent recursion
            if (forceFormPropertiesCount > 0)
                return;
            forceFormPropertiesCount++;
            try
            {
                forceFormProperties_();
            }
            finally
            {
                forceFormPropertiesCount--;
            }
        }
        private int forceFormPropertiesCount = 0;

        private void forceFormProperties_()
        {
            bool performLayout = false;
            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            //
            // enforce form padding to be minimum for our properties...creates reduced "client" area within actual client 
            // so docked child controls will work ok
            bool setPadding = false;
            var padding = this.Padding;
            if (padding.Top < CaptionHeight)
            {
                padding.Top = CaptionHeight;
                setPadding = true;
            }
            if (padding.Left < BorderWidth)
            {
                padding.Left = BorderWidth;
                setPadding = true;
            }
            if (padding.Bottom < BorderWidth)
            {
                padding.Bottom = BorderWidth;
                setPadding = true;
            }
            if (padding.Right < BorderWidth)
            {
                padding.Right = BorderWidth;
                setPadding = true;
            }
            if (setPadding)
            {
                this.Padding = padding;
                performLayout = true;
            }
            //
            // and re-layout caption incase anythign changed
            //
            if (paintCaption(null))
                performLayout = true;
            //
            this.ResumeLayout(performLayout);
            //
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // if includes redrawing top of window
            if (e.ClipRectangle.Y < CaptionHeight)
            {
                paintCaption(e.Graphics);
            }
        }

        private int paintCaptionCount = 0;
        private bool paintCaption(Graphics g)
        {
            if (paintCaptionCount > 0)
                return false;
            paintCaptionCount++;
            try
            {
                return paintCaption_(g);
            }
            finally
            {
                paintCaptionCount--;
            }
        }

        private bool paintCaption_(Graphics g)
        {
            int X = 0;
            int R = this.ClientSize.Width;
            bool ret = false;

            // paint caption bar
            // - icon
            // - text
            // - mainmenustrip
            // - min/max/close
            var rc = new Rectangle(X, 0, R, CaptionHeight);
            if (g != null)
            {
                var sb = new SolidBrush(CaptionColor);
                g.FillRectangle(sb, rc);
            }

            if (this.ShowIcon)
            {
                if (g != null)
                {
                    var rcIcon = new Rectangle(X, 0, CaptionHeight, CaptionHeight);
                    g.DrawIcon(this.Icon, rcIcon);
                    X += CaptionHeight;
                }
            }
            if (!string.IsNullOrEmpty(Text))
            {
                var captionFont = SystemFonts.CaptionFont;
                var captionBrush = SystemBrushes.ActiveCaptionText;

                if (g != null)
                {
                    var m = g.MeasureString(Text, captionFont);
                    int W = CaptionTextWidth;
                    if (W == 0)
                    {
                        W = (int)m.Width + 2;// runf up plus extra pixel space
                    }
                    actualCaptionTextWidth = W;
                    var topOffset = (CaptionHeight - m.Height) / 2;
                    var captionRect = new RectangleF(X, topOffset < 0 ? 0 : topOffset, W, CaptionHeight);
                    g.DrawString(Text, captionFont, captionBrush, captionRect);
                    X += W;
                }
                else
                {
                    X += actualCaptionTextWidth;// use width from last time we drew it (are we one step behind?)
                }
            }
            //
            // seperator after text
            X += CaptionHeight;
            //
            // control button far right
            if (this.ControlBox)
            {
                R -= CaptionHeight;
                if (g != null)
                {
#if SYSTEM_CLOSE_BUTTON
                    var r = new Rectangle(R, 0, CaptionHeight, CaptionHeight);
                    ControlPaint.DrawCaptionButton(g, r, CaptionButton.Close, ButtonState.Normal | ButtonState.Flat);
#else
                    // just draw a simple cross
                    Pen p = Pens.Black;
                    int o = CaptionHeight / 3; // offset to cross ends inside the box
                    int w = CaptionHeight - o - o;// width/height of the cross
                    g.DrawLine(p, R + o, o, R + o + w, o + w);
                    g.DrawLine(p, R + o, o + w, R + o + w, o);
#endif
                }
            }
            //
            // seperator before control button
            R -= CaptionHeight;
            //
            // whatever isleft is for mmenu strip
            // from X to R...
            if (this.MainMenuStrip != null)
            {
                // no room for the menu!
                if (R < X)
                    R = X;

                //
                // if mainmenu is not in calculated place then move it
                // - note width check is ">" rather than "!=" becuase we may set toolstrip size and it
                //   may set somethign lower depending what it has
                if (this.MainMenuStrip.Left != X || this.MainMenuStrip.Right > R)
                {
                    // never dock the main menu from outer form...we "own" its position
                    if (this.MainMenuStrip.Dock != DockStyle.None)
                        this.MainMenuStrip.Dock = DockStyle.None;

                    this.MainMenuStrip.SetBounds(X, 0, R - X, CaptionHeight);
                    ret = true;
                }
                if (g != null)
                {
                    // menu same back color as us
                    if (MainMenuStrip.BackColor != this.CaptionColor)
                        MainMenuStrip.BackColor = this.CaptionColor;

                    MainMenuStrip.Refresh();
                }
            }
            return ret;
        }

        protected override void OnResize(EventArgs e)
        {
            paintCaption(null);// graphics is null so no paint just layout
            base.OnResize(e);
        }

        protected override void WndProc(ref Message m)
        {
            //
            // Look for NC messages and sort of pretend our caption is a new non-clicnet area...
            //
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    {
                        Point pos = new Point(m.LParam.ToInt32());
                        pos = this.PointToClient(pos);

                        //
                        // over right edge
                        if (pos.X >= this.ClientSize.Width - BorderWidth)
                        {
                            if (pos.Y >= this.ClientSize.Height - BorderWidth)
                            {
                                m.Result = (IntPtr)HitTestValues.HTBOTTOMRIGHT;
                                return;
                            }
                            else if (pos.Y < BorderWidth)
                            {
                                m.Result = (IntPtr)HitTestValues.HTTOPRIGHT;
                                return;
                            }
                            else
                            {
                                m.Result = (IntPtr)HitTestValues.HTRIGHT;
                                return;
                            }
                        }
                        //
                        // over left edge
                        if (pos.X < BorderWidth)
                        {
                            if (pos.Y >= this.ClientSize.Height - BorderWidth)
                            {
                                m.Result = (IntPtr)HitTestValues.HTBOTTOMLEFT;
                                return;
                            }
                            else if (pos.Y < BorderWidth)
                            {
                                m.Result = (IntPtr)HitTestValues.HTTOPLEFT;
                                return;
                            }
                            else
                            {
                                m.Result = (IntPtr)HitTestValues.HTLEFT;
                                return;
                            }
                        }
                        //
                        // between left/right edges and over bottom
                        if (pos.Y >= this.ClientSize.Height - BorderWidth)
                        {
                            m.Result = (IntPtr)HitTestValues.HTBOTTOM;
                            return;
                        }
                        //
                        // over top edge
                        else if (pos.Y < BorderWidth)
                        {
                            m.Result = (IntPtr)HitTestValues.HTTOP;
                            return;
                        }
                        //
                        // over top caption (but not right on edge)
                        else if (pos.Y < CaptionHeight)
                        {
                            //
                            // close box in top right...
                            if (this.ControlBox && pos.X > this.ClientSize.Width - CaptionHeight)
                                m.Result = (IntPtr)HitTestValues.HTCLOSE;
                            //
                            // system menu/icon in top left...
                            else if (this.ShowIcon && pos.X < CaptionHeight)
                                m.Result = (IntPtr)HitTestValues.HTSYSMENU;
                            //
                            // else just in the middle of caption for moving window around
                            else
                                m.Result = (IntPtr)HitTestValues.HTCAPTION;
                            return;
                        }
                        break;
                    }

                case WM_NCLBUTTONDOWN:
                    {
                        // handle mouse down on HTCLOSE in order to stop base starting a mouse capture
                        if (m.WParam == (IntPtr)HitTestValues.HTCLOSE
                         || m.WParam == (IntPtr)HitTestValues.HTSYSMENU
                            )
                        {
                            m.Result = (IntPtr)0;
                            return;
                        }
                        break;
                    }
                case WM_NCLBUTTONUP:
                    {
                        // handle mouse down on HTCLOSE and close (should we actually close or just post a close?)
                        if (m.WParam == (IntPtr)HitTestValues.HTCLOSE)
                        {
                            this.Close();
                            m.Result = (IntPtr)0;
                            return;
                        }
                        else if (m.WParam == (IntPtr)HitTestValues.HTSYSMENU)
                        {
                            Debug.WriteLine("WM_NCLBUTTONUP HTSYSMENU");
                        }
                        break;
                    }
            }
            base.WndProc(ref m);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WM_NCLBUTTONUP = 0xA2;

        enum HitTestValues
        {
            /// <summary>
            /// In the border of a window that does not have a sizing border.
            /// </summary>
            HTBORDER = 18,

            /// <summary>
            /// In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).
            /// </summary>
            HTBOTTOM = 15,

            /// <summary>
            /// In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            /// </summary>
            HTBOTTOMLEFT = 16,

            /// <summary>
            /// In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            /// </summary>
            HTBOTTOMRIGHT = 17,

            /// <summary>
            /// In a title bar.
            /// </summary>
            HTCAPTION = 2,

            /// <summary>
            /// In a client area.
            /// </summary>
            HTCLIENT = 1,

            /// <summary>
            /// In a Close button.
            /// </summary>
            HTCLOSE = 20,

            /// <summary>
            /// On the screen background or on a dividing line between windows (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).
            /// </summary>
            HTERROR = -2,

            /// <summary>
            /// In a size box (same as HTSIZE).
            /// </summary>
            HTGROWBOX = 4,

            /// <summary>
            /// In a Help button.
            /// </summary>
            HTHELP = 21,

            /// <summary>
            /// In a horizontal scroll bar.
            /// </summary>
            HTHSCROLL = 6,

            /// <summary>
            /// In the left border of a resizable window (the user can click the mouse to resize the window horizontally).
            /// </summary>
            HTLEFT = 10,

            /// <summary>
            /// In a menu.
            /// </summary>
            HTMENU = 5,

            /// <summary>
            /// In a Maximize button.
            /// </summary>
            HTMAXBUTTON = 9,

            /// <summary>
            /// In a Minimize button.
            /// </summary>
            HTMINBUTTON = 8,

            /// <summary>
            /// On the screen background or on a dividing line between windows.
            /// </summary>
            HTNOWHERE = 0,

            /// <summary>
            /// Not implemented.
            /// </summary>
            /* HTOBJECT = 19, */

            /// <summary>
            /// In a Minimize button.
            /// </summary>
            HTREDUCE = HTMINBUTTON,

            /// <summary>
            /// In the right border of a resizable window (the user can click the mouse to resize the window horizontally).
            /// </summary>
            HTRIGHT = 11,

            /// <summary>
            /// In a size box (same as HTGROWBOX).
            /// </summary>
            HTSIZE = HTGROWBOX,

            /// <summary>
            /// In a window menu or in a Close button in a child window.
            /// </summary>
            HTSYSMENU = 3,

            /// <summary>
            /// In the upper-horizontal border of a window.
            /// </summary>
            HTTOP = 12,

            /// <summary>
            /// In the upper-left corner of a window border.
            /// </summary>
            HTTOPLEFT = 13,

            /// <summary>
            /// In the upper-right corner of a window border.
            /// </summary>
            HTTOPRIGHT = 14,

            /// <summary>
            /// In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
            /// </summary>
            HTTRANSPARENT = -1,

            /// <summary>
            /// In the vertical scroll bar.
            /// </summary>
            HTVSCROLL = 7,

            /// <summary>
            /// In a Maximize button.
            /// </summary>
            HTZOOM = HTMAXBUTTON,
        }

        private void ThinBorderForm_Load(object sender, EventArgs e)
        {
        }

        private void ThinBorderForm_Shown(object sender, EventArgs e)
        {
        }
    }
}
