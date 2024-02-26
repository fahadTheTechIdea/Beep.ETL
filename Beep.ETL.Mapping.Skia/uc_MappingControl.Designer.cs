namespace Beep.ETL.Mapping.Skia
{
    partial class uc_MappingControl
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
            splitContainer1 = new SplitContainer();
            skControl1 = new SkiaSharp.Views.Desktop.SKGLControl();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.AllowDrop = true;
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(skControl1);
            splitContainer1.Size = new Size(974, 755);
            splitContainer1.SplitterDistance = 173;
            splitContainer1.TabIndex = 1;
            // 
            // skControl1
            // 
            skControl1.BackColor = Color.Black;
            skControl1.Dock = DockStyle.Fill;
            skControl1.Location = new Point(0, 0);
            skControl1.Margin = new Padding(4, 3, 4, 3);
            skControl1.Name = "skControl1";
            skControl1.Size = new Size(795, 753);
            skControl1.TabIndex = 0;
            skControl1.VSync = true;
            // 
            // uc_MappingControl
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "uc_MappingControl";
            Size = new Size(974, 755);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainer1;
        private SkiaSharp.Views.Desktop.SKGLControl skControl1;
    }
}
