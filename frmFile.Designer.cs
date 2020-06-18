namespace Idmr.TieLayoutEditor
{
	partial class frmFile
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFile));
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.cmdFileOpen = new System.Windows.Forms.Button();
			this.opnFile = new System.Windows.Forms.OpenFileDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lstFILM = new System.Windows.Forms.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cmdLoad = new System.Windows.Forms.Button();
			this.grpFILM = new System.Windows.Forms.GroupBox();
			this.txtFILM = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.numBlocks = new System.Windows.Forms.NumericUpDown();
			this.numFrames = new System.Windows.Forms.NumericUpDown();
			this.cmdSave = new System.Windows.Forms.Button();
			this.grpFILM.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBlocks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numFrames)).BeginInit();
			this.SuspendLayout();
			// 
			// txtFilename
			// 
			this.txtFilename.Location = new System.Drawing.Point(5, 58);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(192, 20);
			this.txtFilename.TabIndex = 0;
			this.txtFilename.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilename_KeyPress);
			// 
			// cmdFileOpen
			// 
			this.cmdFileOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdFileOpen.Location = new System.Drawing.Point(203, 58);
			this.cmdFileOpen.Name = "cmdFileOpen";
			this.cmdFileOpen.Size = new System.Drawing.Size(23, 19);
			this.cmdFileOpen.TabIndex = 1;
			this.cmdFileOpen.Text = "...";
			this.cmdFileOpen.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.cmdFileOpen.UseVisualStyleBackColor = true;
			this.cmdFileOpen.Click += new System.EventHandler(this.cmdFileOpen_Click);
			// 
			// opnFile
			// 
			this.opnFile.DefaultExt = "lfd";
			this.opnFile.Filter = "LFD Files|*.lfd";
			this.opnFile.FileOk += new System.ComponentModel.CancelEventHandler(this.opnFile_FileOk);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(2, 42);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "LFD Filename";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(61, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(102, 20);
			this.label2.TabIndex = 3;
			this.label2.Text = "File Interface";
			// 
			// lstFILM
			// 
			this.lstFILM.FormattingEnabled = true;
			this.lstFILM.Location = new System.Drawing.Point(5, 89);
			this.lstFILM.Name = "lstFILM";
			this.lstFILM.Size = new System.Drawing.Size(100, 121);
			this.lstFILM.TabIndex = 4;
			this.lstFILM.DoubleClick += new System.EventHandler(this.cmdLoad_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(123, 89);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 81);
			this.label3.TabIndex = 5;
			this.label3.Text = "Available FILMs will be shown in the box to the left. Select a FILM and click the" +
    " \'Load\' button to continue.";
			// 
			// cmdLoad
			// 
			this.cmdLoad.Location = new System.Drawing.Point(123, 182);
			this.cmdLoad.Name = "cmdLoad";
			this.cmdLoad.Size = new System.Drawing.Size(89, 25);
			this.cmdLoad.TabIndex = 6;
			this.cmdLoad.Text = "&Load";
			this.cmdLoad.UseVisualStyleBackColor = true;
			this.cmdLoad.Click += new System.EventHandler(this.cmdLoad_Click);
			// 
			// grpFILM
			// 
			this.grpFILM.Controls.Add(this.txtFILM);
			this.grpFILM.Controls.Add(this.label7);
			this.grpFILM.Controls.Add(this.label6);
			this.grpFILM.Controls.Add(this.label5);
			this.grpFILM.Controls.Add(this.label4);
			this.grpFILM.Controls.Add(this.numBlocks);
			this.grpFILM.Controls.Add(this.numFrames);
			this.grpFILM.Enabled = false;
			this.grpFILM.Location = new System.Drawing.Point(5, 239);
			this.grpFILM.Name = "grpFILM";
			this.grpFILM.Size = new System.Drawing.Size(220, 102);
			this.grpFILM.TabIndex = 7;
			this.grpFILM.TabStop = false;
			this.grpFILM.Text = "FILM options";
			// 
			// txtFILM
			// 
			this.txtFILM.Location = new System.Drawing.Point(115, 21);
			this.txtFILM.MaxLength = 8;
			this.txtFILM.Name = "txtFILM";
			this.txtFILM.Size = new System.Drawing.Size(92, 20);
			this.txtFILM.TabIndex = 3;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(16, 24);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(35, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Name";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(16, 76);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(91, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Number of Blocks";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(16, 76);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(91, 13);
			this.label5.TabIndex = 1;
			this.label5.Text = "Number of Blocks";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(16, 49);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(93, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "Number of Frames";
			// 
			// numBlocks
			// 
			this.numBlocks.Location = new System.Drawing.Point(115, 73);
			this.numBlocks.Maximum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
			this.numBlocks.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numBlocks.Name = "numBlocks";
			this.numBlocks.Size = new System.Drawing.Size(92, 20);
			this.numBlocks.TabIndex = 0;
			this.numBlocks.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// numFrames
			// 
			this.numFrames.Location = new System.Drawing.Point(115, 47);
			this.numFrames.Maximum = new decimal(new int[] {
            32678,
            0,
            0,
            0});
			this.numFrames.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numFrames.Name = "numFrames";
			this.numFrames.Size = new System.Drawing.Size(92, 20);
			this.numFrames.TabIndex = 0;
			this.numFrames.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// cmdSave
			// 
			this.cmdSave.Location = new System.Drawing.Point(124, 215);
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Size = new System.Drawing.Size(87, 24);
			this.cmdSave.TabIndex = 8;
			this.cmdSave.Text = "&Save";
			this.cmdSave.UseVisualStyleBackColor = true;
			this.cmdSave.Click += new System.EventHandler(this.cmdSave_Click);
			// 
			// frmTLE_File
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(235, 340);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.grpFILM);
			this.Controls.Add(this.cmdLoad);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lstFILM);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdFileOpen);
			this.Controls.Add(this.txtFilename);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmTLE_File";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "TIE Layout Editor";
			this.grpFILM.ResumeLayout(false);
			this.grpFILM.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBlocks)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numFrames)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.Button cmdFileOpen;
		private System.Windows.Forms.OpenFileDialog opnFile;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox lstFILM;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button cmdLoad;
		private System.Windows.Forms.GroupBox grpFILM;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numBlocks;
		private System.Windows.Forms.NumericUpDown numFrames;
		private System.Windows.Forms.TextBox txtFILM;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button cmdSave;
	}
}

