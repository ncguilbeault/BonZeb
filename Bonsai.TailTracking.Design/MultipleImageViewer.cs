using System;
using System.Windows.Forms;
using OpenCV.Net;
using System.Globalization;
using Bonsai.Vision.Design;

namespace Bonsai.TailTracking.Design
{
    public partial class ThresholdImageViewer : UserControl
    {
        CheckBox checkBox;
        ToolStripControlHost host;
        ToolStripStatusLabel statusLabel;

        public bool ShowThresholdImage { get => checkBox.Checked; set => checkBox.CheckState = value ? CheckState.Checked : CheckState.Unchecked; }

        //public event EventHandler ShowThresholdImageChanged
        //{
        //    add { checkBox.CheckStateChanged += value; }
        //    remove { checkBox.CheckStateChanged -= value; }
        //}

        public ThresholdImageViewer()
        {
            InitializeComponent();
            checkBox = new CheckBox();
            checkBox.Text = "ShowThresholdImage";
            checkBox.CheckState = CheckState.Unchecked;
            host = new ToolStripControlHost(checkBox);
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(host);
            statusStrip.Items.Add(statusLabel);
            imageControl.Canvas.MouseClick += new MouseEventHandler(imageControl_MouseClick);
            imageControl.Canvas.MouseMove += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    var cursorPosition = imageControl.Canvas.PointToClient(Form.MousePosition);
                    if (imageControl.ClientRectangle.Contains(cursorPosition))
                    {
                        var imageX = (int)(cursorPosition.X * ((float)image.Width / imageControl.Width));
                        var imageY = (int)(cursorPosition.Y * ((float)image.Height / imageControl.Height));
                        var cursorColor = image[imageY, imageX];
                        statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                    }
                }
            };

            imageControl.Canvas.DoubleClick += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    Parent.ClientSize = new System.Drawing.Size(image.Width, image.Height);
                }
            };
        }
        void imageControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }
        public VisualizerCanvas Canvas
        {
            get { return imageControl; }
        }
        public void Update(IplImage image)
        {
            imageControl.Image = image;
            if (image == null) statusLabel.Text = string.Empty;
        }
    }
}