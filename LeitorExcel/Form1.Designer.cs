namespace LeitorExcel
{
    partial class frmGeradorScript
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.lblProgresso = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblTimerProtocolo = new System.Windows.Forms.Label();
            this.lblProgressoProtocolo = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Gerar Petição";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 91);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(118, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Extrair Arquivos";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblProgresso
            // 
            this.lblProgresso.AutoSize = true;
            this.lblProgresso.Location = new System.Drawing.Point(168, 22);
            this.lblProgresso.Name = "lblProgresso";
            this.lblProgresso.Size = new System.Drawing.Size(35, 13);
            this.lblProgresso.TabIndex = 2;
            this.lblProgresso.Text = "label1";
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Location = new System.Drawing.Point(119, 22);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(43, 13);
            this.lblTimer.TabIndex = 3;
            this.lblTimer.Text = "lblTimer";
            // 
            // lblTimerProtocolo
            // 
            this.lblTimerProtocolo.AutoSize = true;
            this.lblTimerProtocolo.Location = new System.Drawing.Point(119, 51);
            this.lblTimerProtocolo.Name = "lblTimerProtocolo";
            this.lblTimerProtocolo.Size = new System.Drawing.Size(35, 13);
            this.lblTimerProtocolo.TabIndex = 6;
            this.lblTimerProtocolo.Text = "label1";
            // 
            // lblProgressoProtocolo
            // 
            this.lblProgressoProtocolo.AutoSize = true;
            this.lblProgressoProtocolo.Location = new System.Drawing.Point(168, 51);
            this.lblProgressoProtocolo.Name = "lblProgressoProtocolo";
            this.lblProgressoProtocolo.Size = new System.Drawing.Size(35, 13);
            this.lblProgressoProtocolo.TabIndex = 5;
            this.lblProgressoProtocolo.Text = "label1";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 41);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(101, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Gerar Protocolo";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // frmGeradorScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 117);
            this.Controls.Add(this.lblTimerProtocolo);
            this.Controls.Add(this.lblProgressoProtocolo);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblProgresso);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "frmGeradorScript";
            this.Text = "Gerar Carga";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lblProgresso;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblTimerProtocolo;
        private System.Windows.Forms.Label lblProgressoProtocolo;
        private System.Windows.Forms.Button button3;
    }
}

