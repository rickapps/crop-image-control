using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestBed
{
    public partial class TestForm : Form
    {
        private Image _curImage;

        public TestForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            _curImage = Properties.Resources.sample;
            displayImage.Image = _curImage;
            displayImage.Disabled = false;
            displayImage.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void OnStatusUpdate(object sender, RickApps.CropImage.StatusEventArg e)
        {
            croppingStatus.Text = String.Format(
                "Pos: ({0}, {1}) Area: {2} X {3}",
                e.CursorPos.X.ToString(),
                e.CursorPos.Y.ToString(),
                e.CroppingRect.Width.ToString(),
                e.CroppingRect.Height.ToString());
        }

        private void OnImageCropped(object sender, EventArgs e)
        {
            // Get the cropped portion of our image
            Image croppedImage = displayImage.SelectedImage;
            Clipboard.SetImage(croppedImage);
            MessageBox.Show("Image copied to your clipboard");
        }
    }
}
