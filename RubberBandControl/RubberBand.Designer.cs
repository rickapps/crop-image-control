namespace StellerJay.FitToList
{
    partial class RubberBand
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.thePicture = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.thePicture)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // thePicture
            // 
            this.thePicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.thePicture.Location = new System.Drawing.Point(3, 3);
            this.thePicture.Name = "thePicture";
            this.thePicture.Size = new System.Drawing.Size(151, 134);
            this.thePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.thePicture.TabIndex = 0;
            this.thePicture.TabStop = false;
            this.thePicture.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.thePicture.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseDoubleClick);
            this.thePicture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.thePicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.thePicture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this.thePicture.Resize += new System.EventHandler(this.OnResize);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.thePicture);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(150, 150);
            this.panel1.TabIndex = 1;
            // 
            // RubberBand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "RubberBand";
            ((System.ComponentModel.ISupportInitialize)(this.thePicture)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox thePicture;
        private System.Windows.Forms.Panel panel1;
    }
}
