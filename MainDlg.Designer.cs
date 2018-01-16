namespace AutoJumper
{
	partial class AutoJumper
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoJumper));
			this.btnStart = new System.Windows.Forms.Button();
			this.btnTestADB = new System.Windows.Forms.Button();
			this.btnGetScreenWin = new System.Windows.Forms.Button();
			this.picBoard = new System.Windows.Forms.PictureBox();
			this.txtInfo = new System.Windows.Forms.TextBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.numWait = new System.Windows.Forms.NumericUpDown();
			this.numStepRatio = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numWait)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numStepRatio)).BeginInit();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(381, 69);
			this.btnStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(85, 44);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "开始";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnTestADB
			// 
			this.btnTestADB.Location = new System.Drawing.Point(381, 13);
			this.btnTestADB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnTestADB.Name = "btnTestADB";
			this.btnTestADB.Size = new System.Drawing.Size(85, 47);
			this.btnTestADB.TabIndex = 1;
			this.btnTestADB.Text = "测试ADB";
			this.btnTestADB.UseVisualStyleBackColor = true;
			this.btnTestADB.Click += new System.EventHandler(this.btnTestADB_Click);
			// 
			// btnGetScreenWin
			// 
			this.btnGetScreenWin.Location = new System.Drawing.Point(382, 123);
			this.btnGetScreenWin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnGetScreenWin.Name = "btnGetScreenWin";
			this.btnGetScreenWin.Size = new System.Drawing.Size(85, 44);
			this.btnGetScreenWin.TabIndex = 0;
			this.btnGetScreenWin.Text = "截屏";
			this.btnGetScreenWin.UseVisualStyleBackColor = true;
			this.btnGetScreenWin.Click += new System.EventHandler(this.btnGetScreenWin_Click);
			// 
			// picBoard
			// 
			this.picBoard.ImageLocation = "";
			this.picBoard.Location = new System.Drawing.Point(12, 12);
			this.picBoard.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.picBoard.Name = "picBoard";
			this.picBoard.Size = new System.Drawing.Size(360, 640);
			this.picBoard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.picBoard.TabIndex = 14;
			this.picBoard.TabStop = false;
			// 
			// txtInfo
			// 
			this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtInfo.Location = new System.Drawing.Point(14, 659);
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ReadOnly = true;
			this.txtInfo.Size = new System.Drawing.Size(451, 28);
			this.txtInfo.TabIndex = 16;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(384, 225);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 19);
			this.label1.TabIndex = 17;
			this.label1.Text = "等待  s";
			// 
			// numWait
			// 
			this.numWait.DecimalPlaces = 1;
			this.numWait.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.numWait.InterceptArrowKeys = false;
			this.numWait.Location = new System.Drawing.Point(382, 247);
			this.numWait.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numWait.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numWait.Name = "numWait";
			this.numWait.Size = new System.Drawing.Size(84, 28);
			this.numWait.TabIndex = 18;
			this.numWait.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			// 
			// numStepRatio
			// 
			this.numStepRatio.DecimalPlaces = 2;
			this.numStepRatio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numStepRatio.InterceptArrowKeys = false;
			this.numStepRatio.Location = new System.Drawing.Point(381, 194);
			this.numStepRatio.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numStepRatio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numStepRatio.Name = "numStepRatio";
			this.numStepRatio.Size = new System.Drawing.Size(84, 28);
			this.numStepRatio.TabIndex = 18;
			this.numStepRatio.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(384, 172);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 19);
			this.label2.TabIndex = 17;
			this.label2.Text = "步长系数";
			// 
			// AutoJumper
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(477, 703);
			this.Controls.Add(this.numStepRatio);
			this.Controls.Add(this.numWait);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtInfo);
			this.Controls.Add(this.picBoard);
			this.Controls.Add(this.btnTestADB);
			this.Controls.Add(this.btnGetScreenWin);
			this.Controls.Add(this.btnStart);
			this.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "AutoJumper";
			this.Text = "AutoJumper";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.JumpHelper_FormClosed);
			this.Load += new System.EventHandler(this.JumpHelper_Load);
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numWait)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numStepRatio)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnTestADB;
		private System.Windows.Forms.Button btnGetScreenWin;
		private System.Windows.Forms.PictureBox picBoard;
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numWait;
		private System.Windows.Forms.NumericUpDown numStepRatio;
		private System.Windows.Forms.Label label2;
	}
}

