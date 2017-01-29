namespace TripToPrint.Views
{
    partial class MainFormView
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
            this.label1 = new System.Windows.Forms.Label();
            this.linkKmzFile = new System.Windows.Forms.LinkLabel();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCopyReportPath = new System.Windows.Forms.Button();
            this.buttonOpenReport = new System.Windows.Forms.Button();
            this.buttonOpenContainingFolder = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.listLog = new TripToPrint.Views.LogListBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select a KMZ/KML file:";
            // 
            // linkKmzFile
            // 
            this.linkKmzFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkKmzFile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.linkKmzFile.Location = new System.Drawing.Point(168, 9);
            this.linkKmzFile.Name = "linkKmzFile";
            this.linkKmzFile.Size = new System.Drawing.Size(744, 23);
            this.linkKmzFile.TabIndex = 1;
            this.linkKmzFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkKmzFile_LinkClicked);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(168, 38);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(129, 29);
            this.buttonGenerate.TabIndex = 2;
            this.buttonGenerate.Text = "Generate report";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 74);
            this.label2.MaximumSize = new System.Drawing.Size(880, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Process Log";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 614);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(900, 23);
            this.progressBar.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(12, 82);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(900, 4);
            this.panel1.TabIndex = 7;
            // 
            // buttonCopyReportPath
            // 
            this.buttonCopyReportPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyReportPath.Location = new System.Drawing.Point(699, 643);
            this.buttonCopyReportPath.Name = "buttonCopyReportPath";
            this.buttonCopyReportPath.Size = new System.Drawing.Size(213, 28);
            this.buttonCopyReportPath.TabIndex = 6;
            this.buttonCopyReportPath.Text = "Copy report path to clipboard";
            this.buttonCopyReportPath.UseVisualStyleBackColor = true;
            this.buttonCopyReportPath.Click += new System.EventHandler(this.buttonCopyReportPath_Click);
            // 
            // buttonOpenReport
            // 
            this.buttonOpenReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenReport.Location = new System.Drawing.Point(326, 643);
            this.buttonOpenReport.Name = "buttonOpenReport";
            this.buttonOpenReport.Size = new System.Drawing.Size(139, 28);
            this.buttonOpenReport.TabIndex = 4;
            this.buttonOpenReport.Text = "Open report file";
            this.buttonOpenReport.UseVisualStyleBackColor = true;
            this.buttonOpenReport.Click += new System.EventHandler(this.buttonOpenReport_Click);
            // 
            // buttonOpenContainingFolder
            // 
            this.buttonOpenContainingFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenContainingFolder.Location = new System.Drawing.Point(471, 643);
            this.buttonOpenContainingFolder.Name = "buttonOpenContainingFolder";
            this.buttonOpenContainingFolder.Size = new System.Drawing.Size(222, 28);
            this.buttonOpenContainingFolder.TabIndex = 5;
            this.buttonOpenContainingFolder.Text = "Open report containing folder";
            this.buttonOpenContainingFolder.UseVisualStyleBackColor = true;
            this.buttonOpenContainingFolder.Click += new System.EventHandler(this.buttonOpenContainingFolder_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.DarkSlateGray;
            this.linkLabel1.Location = new System.Drawing.Point(244, 649);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(75, 17);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Github link";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(9, 649);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(229, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Made by Dmytro Kravtsov \"Genius\"";
            // 
            // listLog
            // 
            this.listLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listLog.IntegralHeight = false;
            this.listLog.ItemHeight = 18;
            this.listLog.Location = new System.Drawing.Point(12, 94);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(900, 514);
            this.listLog.TabIndex = 3;
            // 
            // MainFormView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 683);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.buttonOpenContainingFolder);
            this.Controls.Add(this.buttonOpenReport);
            this.Controls.Add(this.buttonCopyReportPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.linkKmzFile);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(942, 400);
            this.Name = "MainFormView";
            this.Text = "Trip-to-Print";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkKmzFile;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar;
        private LogListBox listLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCopyReportPath;
        private System.Windows.Forms.Button buttonOpenReport;
        private System.Windows.Forms.Button buttonOpenContainingFolder;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
    }
}

