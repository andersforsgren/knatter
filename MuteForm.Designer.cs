
namespace KeyMute
{
   partial class MuteForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.deviceCombo = new System.Windows.Forms.ComboBox();
         this.label1 = new System.Windows.Forms.Label();
         this.pausedCheckbox = new System.Windows.Forms.CheckBox();
         this.SuspendLayout();
         // 
         // deviceCombo
         // 
         this.deviceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.deviceCombo.FormattingEnabled = true;
         this.deviceCombo.Location = new System.Drawing.Point(94, 12);
         this.deviceCombo.Name = "deviceCombo";
         this.deviceCombo.Size = new System.Drawing.Size(319, 21);
         this.deviceCombo.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 15);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(41, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Device";
         // 
         // pausedCheckbox
         // 
         this.pausedCheckbox.AutoSize = true;
         this.pausedCheckbox.Location = new System.Drawing.Point(94, 39);
         this.pausedCheckbox.Name = "pausedCheckbox";
         this.pausedCheckbox.Size = new System.Drawing.Size(62, 17);
         this.pausedCheckbox.TabIndex = 5;
         this.pausedCheckbox.Text = "Paused";
         this.pausedCheckbox.UseVisualStyleBackColor = true;
         // 
         // MuteForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(421, 92);
         this.Controls.Add(this.pausedCheckbox);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.deviceCombo);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "MuteForm";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ComboBox deviceCombo;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.CheckBox pausedCheckbox;
   }
}

