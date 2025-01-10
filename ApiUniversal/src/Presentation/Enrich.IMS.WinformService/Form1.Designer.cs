namespace RunAtTime
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.chk_Stop = new System.Windows.Forms.CheckBox();
            this.btn_Exit = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.notiTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Chck = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "Recurring Winform Running";
            this.notifyIcon1.BalloonTipTitle = "Recurring App";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Hangfire Pos";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // chk_Stop
            // 
            this.chk_Stop.AutoSize = true;
            this.chk_Stop.Location = new System.Drawing.Point(12, 12);
            this.chk_Stop.Name = "chk_Stop";
            this.chk_Stop.Size = new System.Drawing.Size(48, 17);
            this.chk_Stop.TabIndex = 0;
            this.chk_Stop.Text = "Stop";
            this.chk_Stop.UseVisualStyleBackColor = true;
            this.chk_Stop.CheckedChanged += new System.EventHandler(this.chk_Stop_CheckedChanged);
            // 
            // btn_Exit
            // 
            this.btn_Exit.Location = new System.Drawing.Point(61, 8);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(75, 23);
            this.btn_Exit.TabIndex = 1;
            this.btn_Exit.Text = "Exit";
            this.btn_Exit.UseVisualStyleBackColor = true;
            this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.notiTextBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(102, 37);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(150, 81);
            this.panel1.TabIndex = 2;
            // 
            // notiTextBox
            // 
            this.notiTextBox.Location = new System.Drawing.Point(0, 0);
            this.notiTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.notiTextBox.Name = "notiTextBox";
            this.notiTextBox.Size = new System.Drawing.Size(151, 82);
            this.notiTextBox.TabIndex = 2;
            this.notiTextBox.Text = "";
            this.notiTextBox.TextChanged += new System.EventHandler(this.notiTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // btn_Chck
            // 
            this.btn_Chck.Location = new System.Drawing.Point(12, 37);
            this.btn_Chck.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Chck.Name = "btn_Chck";
            this.btn_Chck.Size = new System.Drawing.Size(56, 19);
            this.btn_Chck.TabIndex = 3;
            this.btn_Chck.Text = "Check";
            this.btn_Chck.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btn_Chck.UseVisualStyleBackColor = true;
            this.btn_Chck.Click += new System.EventHandler(this.btn_Chck_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 125);
            this.Controls.Add(this.btn_Chck);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btn_Exit);
            this.Controls.Add(this.chk_Stop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Recurring App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.CheckBox chk_Stop;
        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Chck;
        private System.Windows.Forms.RichTextBox notiTextBox;
    }
}

