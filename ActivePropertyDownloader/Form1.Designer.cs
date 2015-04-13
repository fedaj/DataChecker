namespace DataChecker
{
    partial class Form1
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
            this.downloadbutton = new System.Windows.Forms.Button();
            this.SiteList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.fileSaveLink = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.ClientID = new System.Windows.Forms.NumericUpDown();
            this.emailBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.countryProgress = new System.Windows.Forms.ProgressBar();
            this.label5 = new System.Windows.Forms.Label();
            this.countryProgressText = new System.Windows.Forms.Label();
            this.cityProgressText = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cityProgress = new System.Windows.Forms.ProgressBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.statusText = new System.Windows.Forms.Label();
            this.vNumber = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ClientID)).BeginInit();
            this.SuspendLayout();
            // 
            // downloadbutton
            // 
            this.downloadbutton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.downloadbutton.Location = new System.Drawing.Point(312, 341);
            this.downloadbutton.Name = "downloadbutton";
            this.downloadbutton.Size = new System.Drawing.Size(145, 23);
            this.downloadbutton.TabIndex = 0;
            this.downloadbutton.Text = "Download Properties";
            this.downloadbutton.UseVisualStyleBackColor = true;
            this.downloadbutton.Click += new System.EventHandler(this.button1_Click);
            // 
            // SiteList
            // 
            this.SiteList.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SiteList.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SiteList.FormattingEnabled = true;
            this.SiteList.Location = new System.Drawing.Point(111, 80);
            this.SiteList.Name = "SiteList";
            this.SiteList.Size = new System.Drawing.Size(230, 21);
            this.SiteList.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Site";
            // 
            // fileSaveLink
            // 
            this.fileSaveLink.AutoSize = true;
            this.fileSaveLink.Location = new System.Drawing.Point(59, 312);
            this.fileSaveLink.Name = "fileSaveLink";
            this.fileSaveLink.Size = new System.Drawing.Size(95, 13);
            this.fileSaveLink.TabIndex = 3;
            this.fileSaveLink.TabStop = true;
            this.fileSaveLink.Text = "File Save Location";
            this.fileSaveLink.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.fileSaveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Client ID";
            // 
            // ClientID
            // 
            this.ClientID.Location = new System.Drawing.Point(111, 111);
            this.ClientID.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.ClientID.Name = "ClientID";
            this.ClientID.Size = new System.Drawing.Size(230, 20);
            this.ClientID.TabIndex = 6;
            // 
            // emailBox
            // 
            this.emailBox.Location = new System.Drawing.Point(111, 138);
            this.emailBox.Name = "emailBox";
            this.emailBox.Size = new System.Drawing.Size(230, 20);
            this.emailBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Email";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(49, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Password";
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(111, 165);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.Size = new System.Drawing.Size(230, 20);
            this.passwordBox.TabIndex = 9;
            // 
            // countryProgress
            // 
            this.countryProgress.Location = new System.Drawing.Point(52, 230);
            this.countryProgress.Name = "countryProgress";
            this.countryProgress.Size = new System.Drawing.Size(100, 23);
            this.countryProgress.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(49, 214);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Countries";
            // 
            // countryProgressText
            // 
            this.countryProgressText.AutoSize = true;
            this.countryProgressText.BackColor = System.Drawing.Color.Transparent;
            this.countryProgressText.Location = new System.Drawing.Point(158, 230);
            this.countryProgressText.Name = "countryProgressText";
            this.countryProgressText.Size = new System.Drawing.Size(30, 13);
            this.countryProgressText.TabIndex = 15;
            this.countryProgressText.Text = "0 / 0";
            // 
            // cityProgressText
            // 
            this.cityProgressText.AutoSize = true;
            this.cityProgressText.Location = new System.Drawing.Point(158, 272);
            this.cityProgressText.Name = "cityProgressText";
            this.cityProgressText.Size = new System.Drawing.Size(30, 13);
            this.cityProgressText.TabIndex = 18;
            this.cityProgressText.Text = "0 / 0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(51, 256);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Cities";
            // 
            // cityProgress
            // 
            this.cityProgress.Location = new System.Drawing.Point(54, 272);
            this.cityProgress.Name = "cityProgress";
            this.cityProgress.Size = new System.Drawing.Size(100, 23);
            this.cityProgress.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(178, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(279, 39);
            this.label6.TabIndex = 22;
            this.label6.Text = "Hotel List Downloader\r\nCreated by Zach McNaney\r\nhttps://github.com/zmcnaney/Activ" +
    "ePropertyDownloader\r\n";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(207, 214);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "Current Action";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // statusText
            // 
            this.statusText.AutoSize = true;
            this.statusText.Location = new System.Drawing.Point(270, 231);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(0, 13);
            this.statusText.TabIndex = 24;
            // 
            // vNumber
            // 
            this.vNumber.AutoSize = true;
            this.vNumber.Cursor = System.Windows.Forms.Cursors.Default;
            this.vNumber.Location = new System.Drawing.Point(13, 9);
            this.vNumber.Name = "vNumber";
            this.vNumber.Size = new System.Drawing.Size(50, 13);
            this.vNumber.TabIndex = 25;
            this.vNumber.Text = "V 0.0.0.0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 376);
            this.Controls.Add(this.vNumber);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cityProgressText);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cityProgress);
            this.Controls.Add(this.countryProgressText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.countryProgress);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.passwordBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.emailBox);
            this.Controls.Add(this.ClientID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fileSaveLink);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SiteList);
            this.Controls.Add(this.downloadbutton);
            this.Name = "Form1";
            this.Text = "Property Downloader";
            ((System.ComponentModel.ISupportInitialize)(this.ClientID)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button downloadbutton;
        private System.Windows.Forms.ComboBox SiteList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel fileSaveLink;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown ClientID;
        private System.Windows.Forms.TextBox emailBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.ProgressBar countryProgress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label countryProgressText;
        private System.Windows.Forms.Label cityProgressText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ProgressBar cityProgress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label statusText;
        private System.Windows.Forms.Label vNumber;
    }
}

