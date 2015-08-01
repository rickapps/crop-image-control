namespace TestBed
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.croppingStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.displayImage = new RickApps.CropImage.RubberBand();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.croppingStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(556, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // croppingStatus
            // 
            this.croppingStatus.Name = "croppingStatus";
            this.croppingStatus.Size = new System.Drawing.Size(39, 17);
            this.croppingStatus.Text = "Ready";
            // 
            // displayImage
            // 
            this.displayImage.Disabled = false;
            this.displayImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayImage.Image = null;
            this.displayImage.Location = new System.Drawing.Point(0, 39);
            this.displayImage.Name = "displayImage";
            this.displayImage.Size = new System.Drawing.Size(556, 374);
            this.displayImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.displayImage.TabIndex = 0;
            this.displayImage.ImageCropped += new System.EventHandler(this.OnImageCropped);
            this.displayImage.CursorMove += new System.EventHandler<RickApps.CropImage.StatusEventArg>(this.OnStatusUpdate);
            this.displayImage.Load += new System.EventHandler(this.OnLoad);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(556, 39);
            this.label1.TabIndex = 2;
            this.label1.Text = "Drag the mouse over the image to create a cropping rectangle. Double click inside" +
    " the rectangle to crop. Click outside the image to cancel the rectangle.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(556, 435);
            this.panel1.TabIndex = 3;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 435);
            this.Controls.Add(this.displayImage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Name = "TestForm";
            this.Text = "Image Cropping User Control Demo";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RickApps.CropImage.RubberBand displayImage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel croppingStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
    }
}

