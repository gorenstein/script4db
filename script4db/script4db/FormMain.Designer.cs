namespace script4db
{
    partial class FormMain
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("empty");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonOpen = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonPauseContinue = new System.Windows.Forms.Button();
            this.buttonBreak = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.textBoxScriptFile = new System.Windows.Forms.TextBox();
            this.buttonAbout = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.treeViewScriptBlocks = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageRaw = new System.Windows.Forms.TabPage();
            this.richTextBoxRaw = new System.Windows.Forms.RichTextBox();
            this.tabPageTree = new System.Windows.Forms.TabPage();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.richTextBoxLogs = new System.Windows.Forms.RichTextBox();
            this.statusStrip.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageRaw.SuspendLayout();
            this.tabPageTree.SuspendLayout();
            this.tabPageLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpen
            // 
            this.buttonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpen.Location = new System.Drawing.Point(778, 12);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(96, 31);
            this.buttonOpen.TabIndex = 0;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(778, 62);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(96, 31);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonPauseContinue
            // 
            this.buttonPauseContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPauseContinue.Enabled = false;
            this.buttonPauseContinue.Location = new System.Drawing.Point(778, 99);
            this.buttonPauseContinue.Name = "buttonPauseContinue";
            this.buttonPauseContinue.Size = new System.Drawing.Size(96, 31);
            this.buttonPauseContinue.TabIndex = 2;
            this.buttonPauseContinue.Text = "Pause";
            this.buttonPauseContinue.UseVisualStyleBackColor = true;
            this.buttonPauseContinue.Visible = false;
            this.buttonPauseContinue.Click += new System.EventHandler(this.buttonPauseContinue_Click);
            // 
            // buttonBreak
            // 
            this.buttonBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBreak.Enabled = false;
            this.buttonBreak.Location = new System.Drawing.Point(778, 147);
            this.buttonBreak.Name = "buttonBreak";
            this.buttonBreak.Size = new System.Drawing.Size(96, 31);
            this.buttonBreak.TabIndex = 3;
            this.buttonBreak.Text = "Break";
            this.buttonBreak.UseVisualStyleBackColor = true;
            this.buttonBreak.Click += new System.EventHandler(this.buttonBreak_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(778, 316);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(96, 31);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // textBoxScriptFile
            // 
            this.textBoxScriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxScriptFile.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxScriptFile.Location = new System.Drawing.Point(12, 14);
            this.textBoxScriptFile.Name = "textBoxScriptFile";
            this.textBoxScriptFile.ReadOnly = true;
            this.textBoxScriptFile.Size = new System.Drawing.Size(760, 25);
            this.textBoxScriptFile.TabIndex = 5;
            // 
            // buttonAbout
            // 
            this.buttonAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbout.Location = new System.Drawing.Point(778, 279);
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Size = new System.Drawing.Size(96, 31);
            this.buttonAbout.TabIndex = 6;
            this.buttonAbout.Text = "About";
            this.buttonAbout.UseVisualStyleBackColor = true;
            this.buttonAbout.Click += new System.EventHandler(this.buttonAbout_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip.Location = new System.Drawing.Point(0, 348);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(886, 24);
            this.statusStrip.TabIndex = 7;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 19);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(122, 19);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // treeViewScriptBlocks
            // 
            this.treeViewScriptBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewScriptBlocks.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewScriptBlocks.Location = new System.Drawing.Point(3, 3);
            this.treeViewScriptBlocks.Name = "treeViewScriptBlocks";
            treeNode1.ForeColor = System.Drawing.Color.Gray;
            treeNode1.Name = "Node0";
            treeNode1.NodeFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            treeNode1.Text = "empty";
            this.treeViewScriptBlocks.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewScriptBlocks.Size = new System.Drawing.Size(746, 265);
            this.treeViewScriptBlocks.TabIndex = 8;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageRaw);
            this.tabControl1.Controls.Add(this.tabPageTree);
            this.tabControl1.Controls.Add(this.tabPageLogs);
            this.tabControl1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(12, 46);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(760, 301);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPageRaw
            // 
            this.tabPageRaw.Controls.Add(this.richTextBoxRaw);
            this.tabPageRaw.Location = new System.Drawing.Point(4, 26);
            this.tabPageRaw.Name = "tabPageRaw";
            this.tabPageRaw.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRaw.Size = new System.Drawing.Size(752, 271);
            this.tabPageRaw.TabIndex = 1;
            this.tabPageRaw.Text = "Raw";
            this.tabPageRaw.UseVisualStyleBackColor = true;
            // 
            // richTextBoxRaw
            // 
            this.richTextBoxRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRaw.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRaw.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxRaw.Name = "richTextBoxRaw";
            this.richTextBoxRaw.ReadOnly = true;
            this.richTextBoxRaw.Size = new System.Drawing.Size(746, 265);
            this.richTextBoxRaw.TabIndex = 0;
            this.richTextBoxRaw.Text = "";
            this.richTextBoxRaw.WordWrap = false;
            // 
            // tabPageTree
            // 
            this.tabPageTree.Controls.Add(this.treeViewScriptBlocks);
            this.tabPageTree.Location = new System.Drawing.Point(4, 26);
            this.tabPageTree.Name = "tabPageTree";
            this.tabPageTree.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTree.Size = new System.Drawing.Size(752, 271);
            this.tabPageTree.TabIndex = 0;
            this.tabPageTree.Text = "Tree";
            this.tabPageTree.UseVisualStyleBackColor = true;
            // 
            // tabPageLogs
            // 
            this.tabPageLogs.Controls.Add(this.richTextBoxLogs);
            this.tabPageLogs.Location = new System.Drawing.Point(4, 26);
            this.tabPageLogs.Name = "tabPageLogs";
            this.tabPageLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogs.Size = new System.Drawing.Size(752, 271);
            this.tabPageLogs.TabIndex = 2;
            this.tabPageLogs.Text = "Logs";
            this.tabPageLogs.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLogs
            // 
            this.richTextBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxLogs.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxLogs.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxLogs.Name = "richTextBoxLogs";
            this.richTextBoxLogs.ReadOnly = true;
            this.richTextBoxLogs.Size = new System.Drawing.Size(746, 265);
            this.richTextBoxLogs.TabIndex = 0;
            this.richTextBoxLogs.Text = "";
            this.richTextBoxLogs.WordWrap = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 372);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.buttonAbout);
            this.Controls.Add(this.textBoxScriptFile);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonBreak);
            this.Controls.Add(this.buttonPauseContinue);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonOpen);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "script4db";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageRaw.ResumeLayout(false);
            this.tabPageTree.ResumeLayout(false);
            this.tabPageLogs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonPauseContinue;
        private System.Windows.Forms.Button buttonBreak;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.TextBox textBoxScriptFile;
        private System.Windows.Forms.Button buttonAbout;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TreeView treeViewScriptBlocks;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageTree;
        private System.Windows.Forms.TabPage tabPageRaw;
        private System.Windows.Forms.RichTextBox richTextBoxRaw;
        private System.Windows.Forms.TabPage tabPageLogs;
        private System.Windows.Forms.RichTextBox richTextBoxLogs;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.StatusStrip statusStrip;
    }
}

