using System;
using System.Collections;

namespace OpenDentBusiness {

	/// <summary>Each row is one disease that one patient has.  A disease is a medical condition or allergy.  Diseases are defined in the DiseaseDef table.</summary>
	[Serializable]
	public class Disease:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long DiseaseNum;
		///<summary>FK to patient.PatNum</summary>
		public long PatNum;
		///<summary>FK to diseasedef.DiseaseDefNum.  The disease description is in that table.</summary>
		public long DiseaseDefNum;
		///<summary>Any note about this disease that is specific to this patient.</summary>
		public string PatNote;///<summary>The last date and time this row was altered.  Not user editable.  Will be set to NOW by OD if this patient gets an OnlinePassword assigned.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.TimeStamp)]
		public DateTime DateTStamp;

		///<summary></summary>
		public Disease Copy() {
			return (Disease)this.MemberwiseClone();
		}

		
		
		
		
	}

		



		
	

	

	


}










