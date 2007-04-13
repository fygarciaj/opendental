using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental{
	/// <summary>
	/// Summary description for FormBasicTemplate.
	/// </summary>
	public class FormLetterEdit : System.Windows.Forms.Form{
		private OpenDental.UI.Button butCancel;
		private OpenDental.UI.Button butOK;
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textDescription;
		private System.Windows.Forms.Label label7;
		private OpenDental.ODtextBox textBody;
		///<summary></summary>
		public bool IsNew;
		public Letter LetterCur;

		///<summary></summary>
		public FormLetterEdit()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Lan.F(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLetterEdit));
			this.butCancel = new OpenDental.UI.Button();
			this.butOK = new OpenDental.UI.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textBody = new OpenDental.ODtextBox();
			this.SuspendLayout();
			// 
			// butCancel
			// 
			this.butCancel.AdjustImageLocation = new System.Drawing.Point(0,0);
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Autosize = true;
			this.butCancel.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCancel.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCancel.CornerRadius = 4F;
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(737,496);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75,26);
			this.butCancel.TabIndex = 3;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0,0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(737,455);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75,26);
			this.butOK.TabIndex = 2;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(29,35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100,14);
			this.label2.TabIndex = 3;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(132,30);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(346,20);
			this.textDescription.TabIndex = 0;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(43,56);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(87,79);
			this.label7.TabIndex = 7;
			this.label7.Text = "Body of Letter (do not include the address, greeting, or closing)";
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textBody
			// 
			this.textBody.AcceptsReturn = true;
			this.textBody.Location = new System.Drawing.Point(132,56);
			this.textBody.Multiline = true;
			this.textBody.Name = "textBody";
			this.textBody.QuickPasteType = OpenDentBusiness.QuickPasteType.Letter;
			this.textBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBody.Size = new System.Drawing.Size(680,383);
			this.textBody.TabIndex = 8;
			// 
			// FormLetterEdit
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5,13);
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(844,544);
			this.Controls.Add(this.textBody);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormLetterEdit";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Edit Letter";
			this.Load += new System.EventHandler(this.FormLetterEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void FormLetterEdit_Load(object sender, System.EventArgs e) {
			textDescription.Text=LetterCur.Description;
			textBody.Text=LetterCur.BodyText;
		}

		private void butOK_Click(object sender, System.EventArgs e) {
			LetterCur.Description=textDescription.Text;
			LetterCur.BodyText=textBody.Text;
			if(IsNew){
				Letters.Insert(LetterCur);
			}
			else{
				Letters.Update(LetterCur);
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender, System.EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		


	}
}





















