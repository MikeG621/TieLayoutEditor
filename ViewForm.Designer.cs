namespace Idmr.TieLayoutEditor
{
	partial class ViewForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewForm));
			this.lstBlocks = new System.Windows.Forms.ListBox();
			this.pctView = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.hsbTime = new System.Windows.Forms.HScrollBar();
			this.cmdStart = new System.Windows.Forms.Button();
			this.cmdBack = new System.Windows.Forms.Button();
			this.cmdPlayPause = new System.Windows.Forms.Button();
			this.cmdForward = new System.Windows.Forms.Button();
			this.cmdEnd = new System.Windows.Forms.Button();
			this.tmrPlayback = new System.Windows.Forms.Timer(this.components);
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
			// hsbTime
			// 
			this.hsbTime.Location = new System.Drawing.Point(130, 541);
			this.hsbTime.Name = "hsbTime";
			this.hsbTime.Size = new System.Drawing.Size(320, 20);
			this.hsbTime.TabIndex = 4;
			this.hsbTime.ValueChanged += new System.EventHandler(this.hsbTime_ValueChanged);
			// 
			// cmdStart
			// 
			this.cmdStart.Location = new System.Drawing.Point(502, 539);
			this.cmdStart.Name = "cmdStart";
			this.cmdStart.Size = new System.Drawing.Size(27, 23);
			this.cmdStart.TabIndex = 5;
			this.cmdStart.Text = "|<";
			this.cmdStart.UseVisualStyleBackColor = true;
			this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
			// 
			// cmdBack
			// 
			this.cmdBack.Location = new System.Drawing.Point(535, 539);
			this.cmdBack.Name = "cmdBack";
			this.cmdBack.Size = new System.Drawing.Size(27, 23);
			this.cmdBack.TabIndex = 5;
			this.cmdBack.Text = "<<";
			this.cmdBack.UseVisualStyleBackColor = true;
			// 
			// cmdPlayPause
			// 
			this.cmdPlayPause.Location = new System.Drawing.Point(568, 539);
			this.cmdPlayPause.Name = "cmdPlayPause";
			this.cmdPlayPause.Size = new System.Drawing.Size(27, 23);
			this.cmdPlayPause.TabIndex = 5;
			this.cmdPlayPause.Text = ">";
			this.cmdPlayPause.UseVisualStyleBackColor = true;
			this.cmdPlayPause.Click += new System.EventHandler(this.cmdPlayPause_Click);
			// 
			// cmdForward
			// 
			this.cmdForward.Location = new System.Drawing.Point(601, 539);
			this.cmdForward.Name = "cmdForward";
			this.cmdForward.Size = new System.Drawing.Size(27, 23);
			this.cmdForward.TabIndex = 5;
			this.cmdForward.Text = ">>";
			this.cmdForward.UseVisualStyleBackColor = true;
			this.cmdForward.Click += new System.EventHandler(this.cmdForward_Click);
			// 
			// cmdEnd
			// 
			this.cmdEnd.Location = new System.Drawing.Point(634, 539);
			this.cmdEnd.Name = "cmdEnd";
			this.cmdEnd.Size = new System.Drawing.Size(27, 23);
			this.cmdEnd.TabIndex = 5;
			this.cmdEnd.Text = ">|";
			this.cmdEnd.UseVisualStyleBackColor = true;
			// 
			// tmrPlayback
			// 
			this.tmrPlayback.Interval = 83;
			this.tmrPlayback.Tick += new System.EventHandler(this.tmrPlayback_Tick);
			// 
			// ViewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(774, 570);
			this.Controls.Add(this.cmdEnd);
			this.Controls.Add(this.cmdForward);
			this.Controls.Add(this.cmdPlayPause);
			this.Controls.Add(this.cmdBack);
			this.Controls.Add(this.cmdStart);
			this.Controls.Add(this.hsbTime);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pctView);
			this.Controls.Add(this.lstBlocks);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "ViewForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "frmTLE_View";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pctView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstBlocks;
		private System.Windows.Forms.PictureBox pctView;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.HScrollBar hsbTime;
		private System.Windows.Forms.Button cmdStart;
		private System.Windows.Forms.Button cmdBack;
		private System.Windows.Forms.Button cmdPlayPause;
		private System.Windows.Forms.Button cmdForward;
		private System.Windows.Forms.Button cmdEnd;
		private System.Windows.Forms.Timer tmrPlayback;
	}
}