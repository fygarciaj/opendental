using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness {
	public class ClaimValCodeLogs {

		public static double GetValAmountTotal(Claim Cur, string Code){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<double>(MethodBase.GetCurrentMethod(),Cur,Code);
			}
			double total = 0;
			string command="SELECT * FROM claimvalcodelog WHERE ClaimNum='" + POut.Long(Cur.ClaimNum) + "' AND ValCode='" + POut.String(Code) + "'";
			DataTable table=Db.GetTable(command);
			for(int i=0;i<table.Rows.Count;i++){
				total+=PIn.Double(table.Rows[i][4].ToString());
			}
			return total;
		}

		public static List<ClaimValCodeLog> GetValCodes(long claimNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ClaimValCodeLog>>(MethodBase.GetCurrentMethod(),claimNum);
			}
			string command="SELECT * FROM claimvalcodelog WHERE ClaimNum="+POut.Long(claimNum);
			return Crud.ClaimValCodeLogCrud.SelectMany(command);
		}

		public static void UpdateList(List<ClaimValCodeLog> vCodes) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),vCodes);
				return;
			}
			for(int i=0;i<vCodes.Count;i++){
				ClaimValCodeLog vc = vCodes[i];
				if(vc.ClaimValCodeLogNum==0){
					Crud.ClaimValCodeLogCrud.Insert(vc);
				} 
				else {
					Crud.ClaimValCodeLogCrud.Update(vc);
				}
			}
		}
	}
} 