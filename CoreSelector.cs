using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Retroloader
{
	public partial class CoreSelector : Form
	{
		private RetroarchCoreInfo[] Cores;
		private string RetroarchPath;
		private string Target;

		public CoreSelector(string retroarchPath, string target, RetroarchCoreInfo[] cores)
		{
			this.RetroarchPath = retroarchPath;
			this.Target = target;
			this.Cores = cores;
			InitializeComponent();
		}

		private void CoreSelector_Load(object loadSender, EventArgs loadEvent)
		{
			// Set window title
			this.Text = Path.GetFileName(this.Target) + " - Retroloader";

			// Create buttons that launch each supported core
			for (var i = 0; i < Cores.Length; i++)
			{
				var core = Cores[i];
				var button = new Button
				{
					AutoSize = true,
					Dock = System.Windows.Forms.DockStyle.Top,
					Location = new System.Drawing.Point(8, 8),
					Name = core.PrettyName,
					Size = new System.Drawing.Size(476, 32),
					TabIndex = 3 + i,
					Text = core.PrettyName,
					UseVisualStyleBackColor = true
				};
				button.Click += new EventHandler((Object clickSender, EventArgs clickEvent) =>
				{
					Close();
					var psi = new ProcessStartInfo(Path.Combine(RetroarchPath, "retroarch.exe"), $" -L \"{core.Path}\" \"{Target}\"");
					psi.WorkingDirectory = RetroarchPath;
					Process.Start(psi);
				});
				this.buttonPanel.Controls.Add(button);
			}

			this.buttonPanel.PerformLayout();
			this.Height = this.Height + (32 * Cores.Length) + 16; // button height and 8px padding on top/bottom
		}
	}
}
