namespace ChatClientGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtIP = new TextBox();
            txtName = new TextBox();
            txtChat = new TextBox();
            txtMsg = new TextBox();
            btnConnect = new Button();
            btnSend = new Button();
            btnSendFile = new Button();
            lstUsers = new ListBox();
            label1 = new Label();
            label2 = new Label();
            lable3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // txtIP
            // 
            txtIP.Location = new Point(365, 34);
            txtIP.Name = "txtIP";
            txtIP.Size = new Size(125, 27);
            txtIP.TabIndex = 0;
            // 
            // txtName
            // 
            txtName.Location = new Point(596, 34);
            txtName.Name = "txtName";
            txtName.Size = new Size(125, 27);
            txtName.TabIndex = 1;
            // 
            // txtChat
            // 
            txtChat.Location = new Point(57, 142);
            txtChat.Multiline = true;
            txtChat.Name = "txtChat";
            txtChat.Size = new Size(387, 164);
            txtChat.TabIndex = 2;
            // 
            // txtMsg
            // 
            txtMsg.Location = new Point(596, 188);
            txtMsg.Name = "txtMsg";
            txtMsg.Size = new Size(125, 27);
            txtMsg.TabIndex = 3;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(54, 350);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(94, 29);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(297, 350);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(94, 29);
            btnSend.TabIndex = 5;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(596, 350);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(94, 29);
            btnSendFile.TabIndex = 6;
            btnSendFile.Text = "SendFile";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // lstUsers
            // 
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(24, 12);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(150, 104);
            lstUsers.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(315, 37);
            label1.Name = "label1";
            label1.Size = new Size(21, 20);
            label1.TabIndex = 8;
            label1.Text = "IP";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(540, 41);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 9;
            label2.Text = "Name";
            // 
            // lable3
            // 
            lable3.AutoSize = true;
            lable3.Location = new Point(12, 142);
            lable3.Name = "lable3";
            lable3.Size = new Size(39, 20);
            lable3.TabIndex = 10;
            lable3.Text = "Chat";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(529, 195);
            label4.Name = "label4";
            label4.Size = new Size(61, 20);
            label4.TabIndex = 11;
            label4.Text = "Mesage";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label4);
            Controls.Add(lable3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lstUsers);
            Controls.Add(btnSendFile);
            Controls.Add(btnSend);
            Controls.Add(btnConnect);
            Controls.Add(txtMsg);
            Controls.Add(txtChat);
            Controls.Add(txtName);
            Controls.Add(txtIP);
            Name = "Form1";
            Text = "Client";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtIP;
        private TextBox txtName;
        private TextBox txtChat;
        private TextBox txtMsg;
        private Button btnConnect;
        private Button btnSend;
        private Button btnSendFile;
        private ListBox lstUsers;
        private Label label1;
        private Label label2;
        private Label lable3;
        private Label label4;
    }
}
