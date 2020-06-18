namespace Idmr.TieLayoutEditor
{
	partial class frmView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmView));
			this.lstBlocks = new System.Windows.Forms.ListBox();
			this.pctView = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pctView)).BeginInit();
			this.SuspendLayout();
			// 
			// lstBlocks
			// 
			this.lstBlocks.FormattingEnabled = true;
			this.lstBlocks.Location = new System.Drawing.Point(12, 51);
			this.lstBlocks.Name = "lstBlocks";
			this.lstBlocks.Size = new System.Drawing.Size(112, 511);
			this.lstBlocks.TabIndex = 0;
			this.lstBlocks.SelectedIndexChanged += new System.EventHandler(this.lstBlocks_SelectedIndexChanged);
			// 
			// pctView
			// 
			this.pctView.Location = new System.Drawing.Point(130, 35);
			this.pctView.Name = "pctView";
			this.pctView.Size = new System.Drawing.Size(640, 480);
			this.pctView.TabIndex = 1;
			this.pctView.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(339, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(111, 20);
			this.label1.TabIndex = 2;
			this.label1.Text = "View Interface";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(39, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Blocks";
			// 
			// frmTLE_View
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(774, 570);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pctView);
			this.Controls.Add(this.lstBlocks);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmTLE_View";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "frmTLE_View";
			((System.ComponentModel.ISupportInitialize)(this.pctView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstBlocks;
		private System.Windows.Forms.PictureBox pctView;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}