using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using CodeBase;

namespace OpenDental{
	/// <summary> </summary>
	public class FormProgramLinkEdit : System.Windows.Forms.Form{
		private OpenDental.UI.Button butCancel;
		private OpenDental.UI.Button butOK;
		private System.Windows.Forms.CheckBox checkEnabled;
		private System.ComponentModel.Container components = null;
		private OpenDental.UI.Button butDelete;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textProgName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textProgDesc;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textPath;
		private System.Windows.Forms.TextBox textCommandLine;
		private System.Windows.Forms.ListBox listToolBars;
		private System.Windows.Forms.TextBox textButtonText;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textNote;
		private Label label9;// Required designer variable.
		/// <summary>This Program link is new.</summary>
		public bool IsNew;
		public Program ProgramCur;
		private OpenDental.UI.ODGrid gridMain;
		private TextBox textPluginDllName;
		private Label label5;
		private TextBox textOverride;
		private Label labelOverride;
		private ArrayList ProgramPropertiesForProgram;
		private string pathOverrideOld;

		///<summary></summary>
		public FormProgramLinkEdit(){
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgramLinkEdit));
			this.butCancel = new OpenDental.UI.Button();
			this.butOK = new OpenDental.UI.Button();
			this.checkEnabled = new System.Windows.Forms.CheckBox();
			this.butDelete = new OpenDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textProgName = new System.Windows.Forms.TextBox();
			this.textProgDesc = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPath = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textCommandLine = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.listToolBars = new System.Windows.Forms.ListBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textButtonText = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textNote = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.gridMain = new OpenDental.UI.ODGrid();
			this.textPluginDllName = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textOverride = new System.Windows.Forms.TextBox();
			this.labelOverride = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butCancel
			// 
			this.butCancel.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Autosize = true;
			this.butCancel.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCancel.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCancel.CornerRadius = 4F;
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(702, 514);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 26);
			this.butCancel.TabIndex = 0;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(702, 473);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 1;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// checkEnabled
			// 
			this.checkEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkEnabled.Location = new System.Drawing.Point(161, 60);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(98, 18);
			this.checkEnabled.TabIndex = 41;
			this.checkEnabled.Text = "Enabled";
			this.checkEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butDelete
			// 
			this.butDelete.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Autosize = true;
			this.butDelete.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butDelete.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butDelete.CornerRadius = 4F;
			this.butDelete.Image = global::OpenDental.Properties.Resources.deleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(17, 514);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 26);
			this.butDelete.TabIndex = 43;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(58, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(187, 18);
			this.label1.TabIndex = 44;
			this.label1.Text = "Internal Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textProgName
			// 
			this.textProgName.Location = new System.Drawing.Point(246, 9);
			this.textProgName.Name = "textProgName";
			this.textProgName.ReadOnly = true;
			this.textProgName.Size = new System.Drawing.Size(275, 20);
			this.textProgName.TabIndex = 45;
			// 
			// textProgDesc
			// 
			this.textProgDesc.Location = new System.Drawing.Point(246, 34);
			this.textProgDesc.Name = "textProgDesc";
			this.textProgDesc.Size = new System.Drawing.Size(275, 20);
			this.textProgDesc.TabIndex = 47;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(57, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(187, 18);
			this.label2.TabIndex = 46;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPath
			// 
			this.textPath.Location = new System.Drawing.Point(246, 81);
			this.textPath.Name = "textPath";
			this.textPath.Size = new System.Drawing.Size(410, 20);
			this.textPath.TabIndex = 49;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(13, 83);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(231, 18);
			this.label3.TabIndex = 48;
			this.label3.Text = "Path of file to open";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCommandLine
			// 
			this.textCommandLine.Location = new System.Drawing.Point(246, 131);
			this.textCommandLine.Name = "textCommandLine";
			this.textCommandLine.Size = new System.Drawing.Size(410, 20);
			this.textCommandLine.TabIndex = 52;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(3, 131);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(241, 52);
			this.label4.TabIndex = 51;
			this.label4.Text = "Optional command line arguments.  Leave this blank for most bridges.";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// listToolBars
			// 
			this.listToolBars.Location = new System.Drawing.Point(15, 281);
			this.listToolBars.Name = "listToolBars";
			this.listToolBars.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listToolBars.Size = new System.Drawing.Size(147, 108);
			this.listToolBars.TabIndex = 53;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(14, 247);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(223, 30);
			this.label6.TabIndex = 56;
			this.label6.Text = "Add a button to these toolbars";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textButtonText
			// 
			this.textButtonText.Location = new System.Drawing.Point(246, 196);
			this.textButtonText.Name = "textButtonText";
			this.textButtonText.Size = new System.Drawing.Size(195, 20);
			this.textButtonText.TabIndex = 58;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(13, 197);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(232, 18);
			this.label7.TabIndex = 57;
			this.label7.Text = "Text on button";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textNote
			// 
			this.textNote.Location = new System.Drawing.Point(246, 403);
			this.textNote.MaxLength = 4000;
			this.textNote.Multiline = true;
			this.textNote.Name = "textNote";
			this.textNote.Size = new System.Drawing.Size(323, 80);
			this.textNote.TabIndex = 59;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(246, 383);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(221, 17);
			this.label8.TabIndex = 60;
			this.label8.Text = "Notes";
			this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(244, 154);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(410, 39);
			this.label9.TabIndex = 61;
			this.label9.Text = "For custom bridges, NOT for regular bridges, you can also include [LName], [FName" +
    "], [LNameLetter], [PatNum], [ChartNumber], [WirelessPhone], [HmPhone], or [WkPho" +
    "ne] in either of the three boxes above.";
			// 
			// gridMain
			// 
			this.gridMain.HScrollVisible = false;
			this.gridMain.Location = new System.Drawing.Point(246, 247);
			this.gridMain.Name = "gridMain";
			this.gridMain.ScrollValue = 0;
			this.gridMain.Size = new System.Drawing.Size(410, 133);
			this.gridMain.TabIndex = 62;
			this.gridMain.Title = "Additional Properties";
			this.gridMain.TranslationName = null;
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// textPluginDllName
			// 
			this.textPluginDllName.Location = new System.Drawing.Point(246, 221);
			this.textPluginDllName.Name = "textPluginDllName";
			this.textPluginDllName.Size = new System.Drawing.Size(195, 20);
			this.textPluginDllName.TabIndex = 64;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(13, 222);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(232, 18);
			this.label5.TabIndex = 63;
			this.label5.Text = "Plug-in dll name";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textOverride
			// 
			this.textOverride.Location = new System.Drawing.Point(246, 106);
			this.textOverride.Name = "textOverride";
			this.textOverride.Size = new System.Drawing.Size(410, 20);
			this.textOverride.TabIndex = 66;
			// 
			// labelOverride
			// 
			this.labelOverride.Location = new System.Drawing.Point(13, 108);
			this.labelOverride.Name = "labelOverride";
			this.labelOverride.Size = new System.Drawing.Size(231, 18);
			this.labelOverride.TabIndex = 65;
			this.labelOverride.Text = "Local path override.  Usually left blank.";
			this.labelOverride.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormProgramLinkEdit
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(797, 560);
			this.Controls.Add(this.textOverride);
			this.Controls.Add(this.labelOverride);
			this.Controls.Add(this.textPluginDllName);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.textButtonText);
			this.Controls.Add(this.textCommandLine);
			this.Controls.Add(this.textPath);
			this.Controls.Add(this.textProgDesc);
			this.Controls.Add(this.textProgName);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.listToolBars);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.checkEnabled);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProgramLinkEdit";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Program Link";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormProgramLinkEdit_Closing);
			this.Load += new System.EventHandler(this.FormProgramLinkEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void FormProgramLinkEdit_Load(object sender, System.EventArgs e) {
			if(ProgramCur.ProgName!=""){
				//user not allowed to delete program links that we include, only their own.
				butDelete.Enabled=false;
			}
			pathOverrideOld=ProgramProperties.GetLocalPathOverrideForProgram(ProgramCur.ProgramNum);
			textOverride.Text=pathOverrideOld;
			FillForm();
		}

		private void FillForm(){
			//this is not refined enough to be called more than once on the form because it will not
			//remember the toolbars that were selected.
			ToolButItems.RefreshCache();
			ProgramProperties.RefreshCache();
			textProgName.Text=ProgramCur.ProgName;
			textProgDesc.Text=ProgramCur.ProgDesc;
			checkEnabled.Checked=ProgramCur.Enabled;
			textPath.Text=ProgramCur.Path;
			textCommandLine.Text=ProgramCur.CommandLine;
			textPluginDllName.Text=ProgramCur.PluginDllName;
			textNote.Text=ProgramCur.Note;
			List<ToolButItem> itemsForProgram=ToolButItems.GetForProgram(ProgramCur.ProgramNum);
			listToolBars.Items.Clear();
			for(int i=0;i<Enum.GetNames(typeof(ToolBarsAvail)).Length;i++){
				listToolBars.Items.Add(Enum.GetNames(typeof(ToolBarsAvail))[i]);
			}
			for(int i=0;i<itemsForProgram.Count;i++) {
				listToolBars.SetSelected((int)itemsForProgram[i].ToolBar,true);
			}
			if(itemsForProgram.Count>0){//the text on all buttons will be the same for now
				textButtonText.Text=itemsForProgram[0].ButtonText;
			}
			FillGrid();
		}

		private void FillGrid(){
			ProgramPropertiesForProgram=ProgramProperties.GetForProgram(ProgramCur.ProgramNum);
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g(this,"Property"),260);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Value"),130);
			gridMain.Columns.Add(col);
			gridMain.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<ProgramPropertiesForProgram.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(((ProgramProperty)ProgramPropertiesForProgram[i]).PropertyDesc);
				row.Cells.Add(((ProgramProperty)ProgramPropertiesForProgram[i]).PropertyValue);
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();
		}


		private void gridMain_CellDoubleClick(object sender,OpenDental.UI.ODGridClickEventArgs e) {
			FormProgramProperty FormPP=new FormProgramProperty();
			FormPP.ProgramPropertyCur=(ProgramProperty)ProgramPropertiesForProgram[e.Row];
			FormPP.ShowDialog();
			if(FormPP.DialogResult!=DialogResult.OK) {
				return;
			}
			ProgramProperties.RefreshCache();
			ProgramPropertiesForProgram=ProgramProperties.GetForProgram(ProgramCur.ProgramNum);
			FillGrid();
		}

		private void butDelete_Click(object sender, System.EventArgs e) {
			if(ProgramCur.ProgName!=""){//prevent users from deleting program links that we included.
				MsgBox.Show(this,"Not allowed to delete a program link with an internal name.");
				return;
			}
			if(MessageBox.Show(Lan.g(this,"Delete this program link?"),"",MessageBoxButtons.OKCancel)
				!=DialogResult.OK){
				return;
			}
			if(!IsNew){
				Programs.Delete(ProgramCur);
			}
			DialogResult=DialogResult.OK;
		}

		private void butOK_Click(object sender, System.EventArgs e) {
			if(checkEnabled.Checked && textPluginDllName.Text!=""){
				string dllPath=ODFileUtils.CombinePaths(Application.StartupPath,textPluginDllName.Text);
				if(dllPath.Contains("[VersionMajMin]")) {
					Version vers = new Version(Application.ProductVersion);
					dllPath = dllPath.Replace("[VersionMajMin]","");//now stripped clean
				}
				if(!File.Exists(dllPath)) {
					MessageBox.Show(Lan.g(this,"Dll file not found:")+" "+dllPath);
					return;
				}
			}
			if(textPluginDllName.Text!="" && textPath.Text!="") {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"If both a path and a plug-in are specified, the path will be ignored.  Continue anyway?")) {
					return;
				}
			}
			ProgramCur.ProgName=textProgName.Text;
			ProgramCur.ProgDesc=textProgDesc.Text;
			ProgramCur.Enabled=checkEnabled.Checked;
			ProgramCur.Path=textPath.Text;
			if(pathOverrideOld!=textOverride.Text) {
				ProgramProperties.InsertOrUpdateLocalOverridePath(ProgramCur.ProgramNum,textOverride.Text);
				ProgramProperties.RefreshCache();
			}
			ProgramCur.CommandLine=textCommandLine.Text;
			ProgramCur.PluginDllName=textPluginDllName.Text;
			ProgramCur.Note=textNote.Text;
			if(IsNew){
				Programs.Insert(ProgramCur);
			}
			else{
				Programs.Update(ProgramCur);
			}
			ToolButItems.DeleteAllForProgram(ProgramCur.ProgramNum);
			//then add one toolButItem for each highlighted row in listbox
			ToolButItem ToolButItemCur;
			for(int i=0;i<listToolBars.SelectedIndices.Count;i++){
				ToolButItemCur=new ToolButItem();
				ToolButItemCur.ProgramNum=ProgramCur.ProgramNum;
				ToolButItemCur.ButtonText=textButtonText.Text;
				ToolButItemCur.ToolBar=(ToolBarsAvail)listToolBars.SelectedIndices[i];
				ToolButItems.Insert(ToolButItemCur);
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender, System.EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void FormProgramLinkEdit_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if(DialogResult==DialogResult.OK)
				return;
			if(IsNew){
				Programs.Delete(ProgramCur);
			}
		}

		

		

		

		
		


	}
}





















