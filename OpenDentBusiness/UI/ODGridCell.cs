using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.UI{ 

	///<summary></summary>
	public class ODGridCell{		
		private string text;
		private Color colorText;
		private YN bold;
		//private Color colorBackG;
		
		///<summary>Creates a new ODGridCell.</summary>
		public ODGridCell(){
			text="";
			colorText=Color.Empty;
			bold=YN.Unknown;
			//colorBackG=Color.Empty;
		}

		///<summary>Creates a new ODGridCell.</summary>
		public ODGridCell(string myText){
			text=myText;
			colorText=Color.Empty;
			bold=YN.Unknown;
		}

		///<summary></summary>
		public string Text{
			get{
				return text;
			}
			set{
				text=value;
			}
		}

		///<summary>Default is Color.Empty.  If any color is set, it will override the row color.</summary>
		public Color ColorText{
			get{
				return colorText;
			}
			set{
				colorText=value;
			}
		}

		///<summary>If YN.Unknown, then the row state is used for bold.  Otherwise, this overrides the row.</summary>
		public YN Bold{
			get{
				return bold;
			}
			set{
				bold=value;
			}
		}
	        
	        

	}

	









}






