using System;
using System.Reflection;
using System.Windows.Forms;
using OpenCV.Net;
using Bonsai.Vision.Design;

namespace BonZeb.Design
{
    public partial class MultipleImageViewer : UserControl
    {
        ToolStripComboBox selectImage;
        ToolStripStatusLabel statusLabel;

        public MultipleImageViewer()
        {
            InitializeComponent();
            selectImage = new ToolStripComboBox();
            selectImage.DropDownStyle = ComboBoxStyle.DropDownList;
            selectImage.FlatStyle = FlatStyle.Flat;
            selectImage.BackColor = statusStrip.BackColor;
            selectImage.Width = 200;
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(selectImage);
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

            selectImage.SelectedIndexChanged += (sender, e) =>
            {
                SelectedImageViewer = (string)selectImage.SelectedItem;
                SelectedImageIndex = selectImage.SelectedIndex;
            };
        }

        public void SelectImageIndex(int index)
        {
            selectImage.SelectedIndex = index;
        }

        public int SelectedImageIndex { get; private set; }

        public string SelectedImageViewer { get; private set; }

        public VisualizerCanvas Canvas { get => imageControl; }

        void imageControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }
        public void Update(IplImage image)
        {
            imageControl.Image = image;
            if (image == null)
            {
                statusLabel.Text = string.Empty;
            }
        }
        public void PopulateMultipleImageViewer<T>()
        {
            Type type = typeof(T);
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType.Equals(typeof(IplImage)))
                {
                    selectImage.Items.Add(property.Name);
                    selectImage.SelectedItem = property.Name;
                    SelectedImageViewer = (string)selectImage.SelectedItem;
                }
            }
        }

        public void AddItemToMultipleImageViewer(string item)
        {
            selectImage.Items.Add(item);
            selectImage.SelectedItem = item;
            SelectedImageViewer = (string)selectImage.SelectedItem;
        }
        
        public void MaximizeDropDownMenuWidth()
        {
            int maxWidth = 0;
            foreach (var obj in selectImage.Items)
            {
                maxWidth = TextRenderer.MeasureText(obj.ToString(), selectImage.Font).Width > maxWidth ? TextRenderer.MeasureText(obj.ToString(), selectImage.Font).Width : maxWidth;
            }
            selectImage.DropDownWidth = maxWidth;
        }
    }
}
