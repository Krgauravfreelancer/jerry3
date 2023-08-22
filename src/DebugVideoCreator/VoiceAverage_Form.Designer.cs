namespace DebugVideoCreator
{
    partial class VoiceAverage_Form
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
            this.lblHeading = new System.Windows.Forms.Label();
            this.btnPlayRecording = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnCalcAverage = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblHeading
            // 
            this.lblHeading.AutoSize = true;
            this.lblHeading.Font = new System.Drawing.Font("Calibri", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeading.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(129)))), ((int)(((byte)(204)))));
            this.lblHeading.Location = new System.Drawing.Point(12, 9);
            this.lblHeading.MaximumSize = new System.Drawing.Size(780, 0);
            this.lblHeading.Name = "lblHeading";
            this.lblHeading.Size = new System.Drawing.Size(767, 75);
            this.lblHeading.TabIndex = 0;
            this.lblHeading.Text = "Click on Record, Recording will start. Read out loud all the text and press stop." +
    "  Click Play Recording to listen your recording. Press Calculate Average to calc" +
    " and Save voice avergae";
            this.lblHeading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPlayRecording
            // 
            this.btnPlayRecording.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnPlayRecording.Enabled = false;
            this.btnPlayRecording.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlayRecording.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlayRecording.ForeColor = System.Drawing.Color.Black;
            this.btnPlayRecording.Location = new System.Drawing.Point(11, 435);
            this.btnPlayRecording.Margin = new System.Windows.Forms.Padding(10);
            this.btnPlayRecording.Name = "btnPlayRecording";
            this.btnPlayRecording.Size = new System.Drawing.Size(150, 30);
            this.btnPlayRecording.TabIndex = 1;
            this.btnPlayRecording.Text = "Play Recording";
            this.btnPlayRecording.UseVisualStyleBackColor = false;
            this.btnPlayRecording.Click += new System.EventHandler(this.btnPlayRecording_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecord.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecord.ForeColor = System.Drawing.Color.Black;
            this.btnRecord.Location = new System.Drawing.Point(166, 435);
            this.btnRecord.Margin = new System.Windows.Forms.Padding(10);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(150, 30);
            this.btnRecord.TabIndex = 2;
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.Location = new System.Drawing.Point(320, 435);
            this.btnStop.Margin = new System.Windows.Forms.Padding(10);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(150, 30);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnCalcAverage
            // 
            this.btnCalcAverage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCalcAverage.Enabled = false;
            this.btnCalcAverage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalcAverage.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalcAverage.ForeColor = System.Drawing.Color.Black;
            this.btnCalcAverage.Location = new System.Drawing.Point(475, 435);
            this.btnCalcAverage.Margin = new System.Windows.Forms.Padding(10);
            this.btnCalcAverage.Name = "btnCalcAverage";
            this.btnCalcAverage.Size = new System.Drawing.Size(150, 30);
            this.btnCalcAverage.TabIndex = 5;
            this.btnCalcAverage.Text = "Calc Average";
            this.btnCalcAverage.UseVisualStyleBackColor = false;
            this.btnCalcAverage.Click += new System.EventHandler(this.btnCalcAverage_Click);
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResult.Location = new System.Drawing.Point(10, 400);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(122, 26);
            this.lblResult.TabIndex = 6;
            this.lblResult.Text = "Average - ";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSave.Enabled = false;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.Location = new System.Drawing.Point(629, 435);
            this.btnSave.Margin = new System.Windows.Forms.Padding(10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 30);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // VoiceAverage_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(794, 481);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnCalcAverage);
            this.Controls.Add(this.lblHeading);
            this.Controls.Add(this.btnPlayRecording);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnRecord);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(129)))), ((int)(((byte)(204)))));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VoiceAverage_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Voice Avergae Form Calculator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeading;
        private System.Windows.Forms.Button btnPlayRecording;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnCalcAverage;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btnSave;
    }
}