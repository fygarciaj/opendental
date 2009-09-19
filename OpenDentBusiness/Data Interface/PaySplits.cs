using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness{
	///<summary></summary>
	public class PaySplits {
		///<summary>Returns all paySplits for the given patNum, organized by procDate.  WARNING! Also includes related paysplits that aren't actually attached to patient.  Includes any split where payment is for this patient.</summary>
		public static PaySplit[] Refresh(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<PaySplit[]>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command=
				"SELECT DISTINCT paysplit.* FROM paysplit,payment "
				+"WHERE paysplit.PayNum=payment.PayNum "
				+"AND (paysplit.PatNum = '"+POut.PLong(patNum)+"' OR payment.PatNum = '"+POut.PLong(patNum)+"') "
				+"ORDER BY ProcDate";
			return RefreshAndFill(Db.GetTable(command)).ToArray();
		}

		private static List<PaySplit> RefreshAndFill(DataTable table) {
			//No need to check RemotingRole; no call to db.
			List<PaySplit> retVal=new List<PaySplit>();
			PaySplit split;
			for(int i=0;i<table.Rows.Count;i++) {
				split=new PaySplit();
				split.SplitNum    = PIn.PLong(table.Rows[i][0].ToString());
				split.SplitAmt    = PIn.PDouble(table.Rows[i][1].ToString());
				split.PatNum      = PIn.PLong(table.Rows[i][2].ToString());
				split.ProcDate    = PIn.PDate(table.Rows[i][3].ToString());
				split.PayNum      = PIn.PLong(table.Rows[i][4].ToString());
				//List[i].IsDiscount  = PIn.PBool  (table.Rows[i][5].ToString());
				//List[i].DiscountType= PIn.PInt   (table.Rows[i][6].ToString());
				split.ProvNum     = PIn.PLong(table.Rows[i][7].ToString());
				split.PayPlanNum  = PIn.PLong(table.Rows[i][8].ToString());
				split.DatePay     = PIn.PDate(table.Rows[i][9].ToString());
				split.ProcNum     = PIn.PLong(table.Rows[i][10].ToString());
				split.DateEntry   = PIn.PDate(table.Rows[i][11].ToString());
				split.UnearnedType= PIn.PLong(table.Rows[i][12].ToString());
				retVal.Add(split);
			}
			return retVal;
		}

		///<summary>Used from payment window to get all paysplits for the payment.</summary>
		public static List<PaySplit> GetForPayment(long payNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<PaySplit>>(MethodBase.GetCurrentMethod(),payNum);
			}
			string command=
				"SELECT * FROM paysplit "
				+"WHERE PayNum="+POut.PLong(payNum);
			return RefreshAndFill(Db.GetTable(command));
		}

		///<summary></summary>
		public static void Update(PaySplit split){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),split);
				return;
			}
			string command="UPDATE paysplit SET " 
				+ "SplitAmt = '"     +POut.PDouble(split.SplitAmt)+"'"
				+ ",PatNum = '"      +POut.PLong   (split.PatNum)+"'"
				+ ",ProcDate = "    +POut.PDate  (split.ProcDate)
				+ ",PayNum = '"      +POut.PLong   (split.PayNum)+"'"
				+ ",ProvNum = '"     +POut.PLong   (split.ProvNum)+"'"
				+ ",PayPlanNum = '"  +POut.PLong   (split.PayPlanNum)+"'"
				+ ",DatePay = "     +POut.PDate  (split.DatePay)
				+ ",ProcNum = '"     +POut.PLong   (split.ProcNum)+"'"
				//+ ",DateEntry = '"   +POut.PDate  (DateEntry)+"'"//not allowed to change
				+ ",UnearnedType = '" +POut.PLong(split.UnearnedType)+"'"
				+" WHERE splitNum = '"+POut.PLong (split.SplitNum)+"'";
 			Db.NonQ(command);
		}

		///<summary></summary>
		public static long Insert(PaySplit split) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				split.SplitNum=Meth.GetInt(MethodBase.GetCurrentMethod(),split);
				return split.SplitNum;
			}
			if(PrefC.RandomKeys){
				split.SplitNum=ReplicationServers.GetKey("paysplit","SplitNum");
			}
			string command= "INSERT INTO paysplit (";
			if(PrefC.RandomKeys){
				command+="SplitNum,";
			}
			command+="SplitAmt,PatNum,ProcDate, "
				+"PayNum,IsDiscount,DiscountType,ProvNum,PayPlanNum,DatePay,ProcNum,DateEntry,UnearnedType) VALUES(";
			if(PrefC.RandomKeys){
				command+="'"+POut.PLong(split.SplitNum)+"', ";
			}
			command+=
				 "'"+POut.PDouble(split.SplitAmt)+"', "
				+"'"+POut.PLong(split.PatNum)+"', "
				+POut.PDate(split.ProcDate)+", "
				+"'"+POut.PLong(split.PayNum)+"', "
				+"'0', "//IsDiscount
				+"'0', "//DiscountType
				+"'"+POut.PLong(split.ProvNum)+"', "
				+"'"+POut.PLong(split.PayPlanNum)+"', "
				+POut.PDate(split.DatePay)+", "
				+"'"+POut.PLong(split.ProcNum)+"', "
				+"NOW(), "//DateEntry: date of server
				+"'"+POut.PLong(split.UnearnedType)+"')";
 			if(PrefC.RandomKeys){
				Db.NonQ(command);
			}
			else{
 				split.SplitNum=Db.NonQ(command,true);
			}
			return split.SplitNum;
		}

		///<summary>Deletes the paysplit.</summary>
		public static void Delete(PaySplit split){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),split);
				return;
			}
			string command= "DELETE from paysplit WHERE splitNum = "+POut.PLong(split.SplitNum);
 			Db.NonQ(command);
		}

		///<summary>Returns all paySplits for the given procNum. Must supply a list of all paysplits for the patient.</summary>
		public static ArrayList GetForProc(long procNum,PaySplit[] List) {
			//No need to check RemotingRole; no call to db.
			ArrayList retVal=new ArrayList();
			for(int i=0;i<List.Length;i++){
				if(List[i].ProcNum==procNum){
					retVal.Add(List[i]);
				}
			}
			return retVal;
		}

		///<summary>Used from ContrAccount and ProcEdit to display and calculate payments attached to procs. Used once in FormProcEdit</summary>
		public static double GetTotForProc(long procNum,PaySplit[] List) {
			//No need to check RemotingRole; no call to db.
			double retVal=0;
			for(int i=0;i<List.Length;i++){
				if(List[i].ProcNum==procNum){
					retVal+=List[i].SplitAmt;
				}
			}
			return retVal;
		}

		///<summary>Used from FormPaySplitEdit.  Returns total payments for a procedure for all paysplits other than the supplied excluded paysplit.</summary>
		public static double GetTotForProc(long procNum,PaySplit[] List,long excludeSplitNum) {
			//No need to check RemotingRole; no call to db.
			double retVal=0;
			for(int i=0;i<List.Length;i++){
				if(List[i].SplitNum==excludeSplitNum){
					continue;
				}
				if(List[i].ProcNum==procNum){
					retVal+=List[i].SplitAmt;
				}
			}
			return retVal;
		}

		///<summary>Used once in ContrAccount.  WARNING!  The returned list of 'paysplits' are not real paysplits.  They are actually grouped by patient and date.  Only the ProcDate, SplitAmt, PatNum, and ProcNum(one of many) are filled. Must supply a list which would include all paysplits for this payment.</summary>
		public static ArrayList GetGroupedForPayment(long payNum,PaySplit[] List) {
			//No need to check RemotingRole; no call to db.
			ArrayList retVal=new ArrayList();
			int matchI;
			for(int i=0;i<List.Length;i++){
				if(List[i].PayNum==payNum){
					//find a 'paysplit' with matching procdate and patnum
					matchI=-1;
					for(int j=0;j<retVal.Count;j++){
						if(((PaySplit)retVal[j]).ProcDate==List[i].ProcDate && ((PaySplit)retVal[j]).PatNum==List[i].PatNum){
							matchI=j;
							break;
						}
					}
					if(matchI==-1){
						retVal.Add(new PaySplit());
						matchI=retVal.Count-1;
						((PaySplit)retVal[matchI]).ProcDate=List[i].ProcDate;
						((PaySplit)retVal[matchI]).PatNum=List[i].PatNum;
					}
					if(((PaySplit)retVal[matchI]).ProcNum==0 && List[i].ProcNum!=0){
						((PaySplit)retVal[matchI]).ProcNum=List[i].ProcNum;
					}
					((PaySplit)retVal[matchI]).SplitAmt+=List[i].SplitAmt;
				}
			}
			return retVal;
		}

		///<summary>Only those amounts that have the same paynum, procDate, and patNum as the payment, and are not attached to procedures.</summary>
		public static double GetAmountForPayment(long payNum,DateTime payDate,long patNum,PaySplit[] paySplitList) {
			//No need to check RemotingRole; no call to db.
			double retVal=0;
			for(int i=0;i<paySplitList.Length;i++){
				if(paySplitList[i].PayNum!=payNum) {
					continue;
				}
				if(paySplitList[i].PatNum!=patNum){
					continue;
				}
				if(paySplitList[i].ProcDate!=payDate){
					continue;
				}
				if(paySplitList[i].ProcNum!=0){
					continue;
				}
				retVal+=paySplitList[i].SplitAmt;
			}
			return retVal;
		}

		///<summary>Used once in ContrAccount to just get the splits for a single patient.  The supplied list also contains splits that are not necessarily for this one patient.</summary>
		public static PaySplit[] GetForPatient(long patNum,PaySplit[] List) {
			//No need to check RemotingRole; no call to db.
			ArrayList retVal=new ArrayList();
			for(int i=0;i<List.Length;i++){
				if(List[i].PatNum==patNum){
					retVal.Add(List[i]);
				}
			}
			PaySplit[] retList=new PaySplit[retVal.Count];
			retVal.CopyTo(retList);
			return retList;
		}

		///<summary>Used once in ContrAccount.  Usually returns 0 unless there is a payplan for this payment and patient.</summary>
		public static long GetPayPlanNum(long payNum,long patNum,PaySplit[] List) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<List.Length;i++){
				if(List[i].PayNum==payNum && List[i].PatNum==patNum && List[i].PayPlanNum!=0){
					return List[i].PayPlanNum;
				}
			}
			return 0;
		}

		/*
		///<summary>Used in ComputeBalances to compute balance for a single patient. Supply a list of all paysplits for the patient.</summary>
		public static double ComputeBal(PaySplit[] list){//
			double retVal=0;
			for(int i=0;i<list.Length;i++){
				retVal+=list[i].SplitAmt;
			}
			return retVal;
		}*/

		///<summary>Used in FormPayment to sych database with changes user made to the paySplit list for a payment.  Must supply an old list for comparison.  Only the differences are saved.</summary>
		public static void UpdateList(List<PaySplit> oldSplitList,List<PaySplit> newSplitList) {
			//No need to check RemotingRole; no call to db.
			PaySplit newPaySplit;
			for(int i=0;i<oldSplitList.Count;i++) {//loop through the old list
				newPaySplit=null;
				for(int j=0;j<newSplitList.Count;j++) {
					if(newSplitList[j]==null || newSplitList[j].SplitNum==0) {
						continue;
					}
					if(((PaySplit)oldSplitList[i]).SplitNum==((PaySplit)newSplitList[j]).SplitNum) {
						newPaySplit=newSplitList[j];
						break;
					}
				}
				if(newPaySplit==null) {
					//PaySplit with matching SplitNum was not found, so it must have been deleted
					PaySplits.Delete(oldSplitList[i]);
					continue;
				}
				//PaySplit was found with matching SplitNum, so check for changes
				if(newPaySplit.DateEntry != oldSplitList[i].DateEntry
					|| newPaySplit.DatePay != oldSplitList[i].DatePay
					|| newPaySplit.PatNum != oldSplitList[i].PatNum
					|| newPaySplit.PayNum != oldSplitList[i].PayNum
					|| newPaySplit.PayPlanNum != oldSplitList[i].PayPlanNum
					|| newPaySplit.ProcDate != oldSplitList[i].ProcDate
					|| newPaySplit.ProcNum != oldSplitList[i].ProcNum
					|| newPaySplit.ProvNum != oldSplitList[i].ProvNum
					|| newPaySplit.SplitAmt != oldSplitList[i].SplitAmt
					|| newPaySplit.UnearnedType != oldSplitList[i].UnearnedType) 
				{
					PaySplits.Update(newPaySplit);
				}
			}
			for(int i=0;i<newSplitList.Count;i++) {//loop through the new list
				if(newSplitList[i]==null) {
					continue;
				}
				if(newSplitList[i].SplitNum!=0) {
					continue;
				}
				//entry with SplitNum=0, so it's new
				PaySplits.Insert(newSplitList[i]);
			}
		}

		

	}

	

	


}










