namespace Retroloader
{
	partial class CoreSelector
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CoreSelector));
			this.message = new System.Windows.Forms.Label();
			this.outerPanel = new System.Windows.Forms.Panel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.outerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// message
			// 
			this.message.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.message.Dock = System.Windows.Forms.DockStyle.Top;
			this.message.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.message.Location = new System.Drawing.Point(0, 0);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(492, 81);
			this.message.TabIndex = 1;
			this.message.Text = "Multiple cores reported the ability to run files with this extension. Choose one:" +
    "";
			this.message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// outerPanel
			// 
			this.outerPanel.Controls.Add(this.buttonPanel);
			this.outerPanel.Controls.Add(this.message);
			this.outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outerPanel.Location = new System.Drawing.Point(0, 0);
			this.outerPanel.Name = "outerPanel";
			this.outerPanel.Size = new System.Drawing.Size(492, 82);
			this.outerPanel.TabIndex = 1;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonPanel.Location = new System.Drawing.Point(0, 81);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(8);
			this.buttonPanel.Size = new System.Drawing.Size(492, 1);
			this.buttonPanel.TabIndex = 2;
			// 
			// CoreSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(492, 82);
			this.Controls.Add(this.outerPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "CoreSelector";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Retroloader";
			this.Load += new System.EventHandler(this.CoreSelector_Load);
			this.outerPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label message;
		private System.Windows.Forms.Panel outerPanel;
		private System.Windows.Forms.Panel buttonPanel;
	}
}

