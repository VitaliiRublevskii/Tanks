namespace FieldForTanks
{
    partial class fieldForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            fieldPanel = new Panel();
            SuspendLayout();
            // 
            // fieldPanel
            // 
            fieldPanel.BackColor = SystemColors.ControlDark;
            fieldPanel.BackgroundImage = Properties.Resources.Field;
            fieldPanel.BackgroundImageLayout = ImageLayout.Stretch;
            fieldPanel.Location = new Point(12, 12);
            fieldPanel.Name = "fieldPanel";
            fieldPanel.Size = new Size(860, 437);
            fieldPanel.TabIndex = 0;
            // 
            // fieldForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 461);
            Controls.Add(fieldPanel);
            Name = "fieldForm";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Panel fieldPanel;
    }
}
