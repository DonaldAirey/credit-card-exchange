namespace FluidTrade.Guardian
{
	partial class FormSlice
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSlice));
			this.groupBoxQuantity = new System.Windows.Forms.GroupBox();
			this.comboBoxUnit = new System.Windows.Forms.ComboBox();
			this.textBoxValue = new System.Windows.Forms.TextBox();
			this.buttonHelp = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.listBoxDestination1 = new ListBoxDestination();
			this.radioButtonUseDefaultDestination = new System.Windows.Forms.RadioButton();
			this.groupBoxQuantity.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxQuantity
			// 
			this.groupBoxQuantity.Controls.Add(this.comboBoxUnit);
			this.groupBoxQuantity.Controls.Add(this.textBoxValue);
			this.groupBoxQuantity.Location = new System.Drawing.Point(12, 12);
			this.groupBoxQuantity.Name = "groupBoxQuantity";
			this.groupBoxQuantity.Size = new System.Drawing.Size(366, 60);
			this.groupBoxQuantity.TabIndex = 0;
			this.groupBoxQuantity.TabStop = false;
			this.groupBoxQuantity.Text = "Quantity";
			// 
			// comboBoxUnit
			// 
			this.comboBoxUnit.FormattingEnabled = true;
			this.comboBoxUnit.Location = new System.Drawing.Point(148, 20);
			this.comboBoxUnit.Name = "comboBoxUnit";
			this.comboBoxUnit.Size = new System.Drawing.Size(121, 21);
			this.comboBoxUnit.TabIndex = 1;
			// 
			// textBoxValue
			// 
			this.textBoxValue.Location = new System.Drawing.Point(17, 19);
			this.textBoxValue.Name = "textBoxValue";
			this.textBoxValue.Size = new System.Drawing.Size(124, 20);
			this.textBoxValue.TabIndex = 0;
			// 
			// buttonHelp
			// 
			this.buttonHelp.Enabled = false;
			this.buttonHelp.Location = new System.Drawing.Point(304, 225);
			this.buttonHelp.Name = "buttonHelp";
			this.buttonHelp.Size = new System.Drawing.Size(75, 23);
			this.buttonHelp.TabIndex = 1;
			this.buttonHelp.Text = "Help";
			this.buttonHelp.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(223, 225);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(142, 225);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.listBoxDestination1);
			this.groupBox1.Controls.Add(this.radioButtonUseDefaultDestination);
			this.groupBox1.Location = new System.Drawing.Point(12, 78);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(366, 129);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Destination";
			// 
			// listBoxDestination1
			// 
			this.listBoxDestination1.DataSource = ((object)(resources.GetObject("listBoxDestination1.DataSource")));
			this.listBoxDestination1.DisplayMember = "ShortName";
			this.listBoxDestination1.FormattingEnabled = true;
			this.listBoxDestination1.Location = new System.Drawing.Point(6, 43);
			this.listBoxDestination1.Name = "listBoxDestination1";
			this.listBoxDestination1.Size = new System.Drawing.Size(354, 69);
			this.listBoxDestination1.TabIndex = 1;
			this.listBoxDestination1.ValueMember = "DestinationId";
			// 
			// radioButtonUseDefaultDestination
			// 
			this.radioButtonUseDefaultDestination.AutoSize = true;
			this.radioButtonUseDefaultDestination.Location = new System.Drawing.Point(7, 20);
			this.radioButtonUseDefaultDestination.Name = "radioButtonUseDefaultDestination";
			this.radioButtonUseDefaultDestination.Size = new System.Drawing.Size(81, 17);
			this.radioButtonUseDefaultDestination.TabIndex = 0;
			this.radioButtonUseDefaultDestination.TabStop = true;
			this.radioButtonUseDefaultDestination.Text = "Use Default";
			this.radioButtonUseDefaultDestination.UseVisualStyleBackColor = true;
			// 
			// FormSlice
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(397, 260);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonHelp);
			this.Controls.Add(this.groupBoxQuantity);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormSlice";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Slice Orders";
			this.groupBoxQuantity.ResumeLayout(false);
			this.groupBoxQuantity.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxQuantity;
		private System.Windows.Forms.Button buttonHelp;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ComboBox comboBoxUnit;
		private System.Windows.Forms.TextBox textBoxValue;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonUseDefaultDestination;
		private ListBoxDestination listBoxDestination1;
	}
}