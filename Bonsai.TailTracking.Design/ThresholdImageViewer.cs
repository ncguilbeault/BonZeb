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

        public ThresholdImageViewer()
        {
            InitializeComponent();
            checkBox = new CheckBox();
            checkBox.Text = "ShowThresholdImage";
            checkBox.CheckState = CheckState.Unchecked;
            host = new ToolStripControlHost(checkBox);
            statusStrip.Items.Add(host);
        }

        public bool ShowThresholdImage { get => checkBox.Checked; set => checkBox.CheckState = value ? CheckState.Checked : CheckState.Unchecked; }

        public event EventHandler ShowThresholdImageChanged
        {
            add { checkBox.CheckStateChanged += value; }
            remove { checkBox.CheckStateChanged -= value; }
        }
        public VisualizerCanvas Canvas
        {
            get { return imageControl; }
        }
    }
}