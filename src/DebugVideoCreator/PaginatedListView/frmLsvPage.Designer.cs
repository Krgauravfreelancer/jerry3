namespace VideoCreator.PaginatedListView
{
    partial class frmLsvPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLsvPage));
            this.lsvData = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pnlNRPP = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.nudNRPP = new System.Windows.Forms.NumericUpDown();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnLast = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnFirst = new System.Windows.Forms.Button();
            this.pnlNavigate = new System.Windows.Forms.Panel();
            this.pnlNRPP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNRPP)).BeginInit();
            this.pnlNavigate.SuspendLayout();
            this.SuspendLayout();
            // 
            // lsvData
            // 
            this.lsvData.FullRowSelect = true;
            this.lsvData.GridLines = true;
            this.lsvData.HideSelection = false;
            this.lsvData.Location = new System.Drawing.Point(8, 50);
            this.lsvData.Name = "lsvData";
            this.lsvData.Size = new System.Drawing.Size(554, 262);
            this.lsvData.SmallImageList = this.imageList1;
            this.lsvData.TabIndex = 0;
            this.lsvData.UseCompatibleStateImageBehavior = false;
            this.lsvData.View = System.Windows.Forms.View.Details;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "plus_32.png");
            // 
            // pnlNRPP
            // 
            this.pnlNRPP.Controls.Add(this.label1);
            this.pnlNRPP.Controls.Add(this.nudNRPP);
            this.pnlNRPP.Location = new System.Drawing.Point(8, 10);
            this.pnlNRPP.Name = "pnlNRPP";
            this.pnlNRPP.Size = new System.Drawing.Size(192, 32);
            this.pnlNRPP.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "No. of Recrods Per Page";
            // 
            // nudNRPP
            // 
            this.nudNRPP.Location = new System.Drawing.Point(136, 6);
            this.nudNRPP.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.nudNRPP.Name = "nudNRPP";
            this.nudNRPP.Size = new System.Drawing.Size(48, 20);
            this.nudNRPP.TabIndex = 24;
            this.nudNRPP.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudNRPP.ValueChanged += new System.EventHandler(this.nudNRPP_ValueChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblInfo.Location = new System.Drawing.Point(13, 13);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(47, 15);
            this.lblInfo.TabIndex = 24;
            this.lblInfo.Text = "lblInfo";
            // 
            // btnLast
            // 
            this.btnLast.BackgroundImage = global::DebugVideoCreator.Properties.Resources.last;
            this.btnLast.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLast.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLast.Location = new System.Drawing.Point(537, 5);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(32, 35);
            this.btnLast.TabIndex = 23;
            this.btnLast.UseVisualStyleBackColor = true;
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // btnNext
            // 
            this.btnNext.BackgroundImage = global::DebugVideoCreator.Properties.Resources.next;
            this.btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNext.Location = new System.Drawing.Point(505, 5);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(32, 35);
            this.btnNext.TabIndex = 22;
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.BackgroundImage = global::DebugVideoCreator.Properties.Resources.previous;
            this.btnPrev.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPrev.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPrev.Location = new System.Drawing.Point(473, 5);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(32, 35);
            this.btnPrev.TabIndex = 21;
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnFirst
            // 
            this.btnFirst.BackgroundImage = global::DebugVideoCreator.Properties.Resources.first;
            this.btnFirst.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFirst.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnFirst.Location = new System.Drawing.Point(441, 5);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(32, 35);
            this.btnFirst.TabIndex = 20;
            this.btnFirst.UseVisualStyleBackColor = true;
            this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
            // 
            // pnlNavigate
            // 
            this.pnlNavigate.BackColor = System.Drawing.Color.White;
            this.pnlNavigate.Controls.Add(this.btnFirst);
            this.pnlNavigate.Controls.Add(this.btnPrev);
            this.pnlNavigate.Controls.Add(this.btnNext);
            this.pnlNavigate.Controls.Add(this.btnLast);
            this.pnlNavigate.Controls.Add(this.lblInfo);
            this.pnlNavigate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNavigate.Location = new System.Drawing.Point(0, 314);
            this.pnlNavigate.Name = "pnlNavigate";
            this.pnlNavigate.Padding = new System.Windows.Forms.Padding(5);
            this.pnlNavigate.Size = new System.Drawing.Size(574, 45);
            this.pnlNavigate.TabIndex = 22;
            // 
            // frmLsvPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 359);
            this.Controls.Add(this.pnlNRPP);
            this.Controls.Add(this.pnlNavigate);
            this.Controls.Add(this.lsvData);
            this.Name = "frmLsvPage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Media Library";
            this.Load += new System.EventHandler(this.frmLsvPage_Load);
            this.Resize += new System.EventHandler(this.frmLsvPage_Resize);
            this.pnlNRPP.ResumeLayout(false);
            this.pnlNRPP.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNRPP)).EndInit();
            this.pnlNavigate.ResumeLayout(false);
            this.pnlNavigate.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvData;
        private System.Windows.Forms.Panel pnlNRPP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudNRPP;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnLast;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnFirst;
        private System.Windows.Forms.Panel pnlNavigate;
    }
}

