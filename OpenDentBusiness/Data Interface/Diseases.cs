using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace OpenDentBusiness {
	///<summary></summary>
	public class Diseases {
		public static Disease GetSpecificDiseaseForPatient(long patNum,long diseaseDefNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Disease>(MethodBase.GetCurrentMethod(),patNum,diseaseDefNum);
			}
			string command="SELECT * FROM disease WHERE PatNum="+POut.Long(patNum)
				+" AND DiseaseDefNum="+POut.Long(diseaseDefNum);
			return Crud.DiseaseCrud.SelectOne(command);
		}

		///<summary>Gets a list of all Diseases for a given patient.  Includes hidden. Sorted by diseasedef.ItemOrder.</summary>
		public static Disease[] Refresh(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Disease[]>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT disease.* FROM disease,diseasedef "
				+"WHERE disease.DiseaseDefNum=diseasedef.DiseaseDefNum "
				+"AND PatNum="+POut.Long(patNum)
				+" ORDER BY diseasedef.ItemOrder";
			return Crud.DiseaseCrud.SelectMany(command).ToArray();
		}

		///<summary></summary>
		public static void Update(Disease disease) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),disease);
				return;
			}
			Crud.DiseaseCrud.Update(disease);
		}

		///<summary></summary>
		public static long Insert(Disease disease) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				disease.DiseaseNum=Meth.GetLong(MethodBase.GetCurrentMethod(),disease);
				return disease.DiseaseNum;
			}
			return Crud.DiseaseCrud.Insert(disease);
		}

		///<summary></summary>
		public static void Delete(Disease disease) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),disease);
				return;
			}
			string command="DELETE FROM disease WHERE DiseaseNum ="+POut.Long(disease.DiseaseNum);
			Db.NonQ(command);
		}

		///<summary>Deletes all diseases for one patient.</summary>
		public static void DeleteAllForPt(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum);
				return;
			}
			string command="DELETE FROM disease WHERE PatNum ="+POut.Long(patNum);
			Db.NonQ(command);
		}

		
		
		
		
	}

		



		
	

	

	


}










