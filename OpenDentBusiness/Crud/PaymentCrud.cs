//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class PaymentCrud {
		///<summary>Gets one Payment object from the database using the primary key.  Returns null if not found.</summary>
		public static Payment SelectOne(long payNum){
			string command="SELECT * FROM payment "
				+"WHERE PayNum = "+POut.Long(payNum);
			List<Payment> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one Payment object from the database using a query.</summary>
		public static Payment SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<Payment> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of Payment objects from the database using a query.</summary>
		public static List<Payment> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<Payment> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<Payment> TableToList(DataTable table){
			List<Payment> retVal=new List<Payment>();
			Payment payment;
			for(int i=0;i<table.Rows.Count;i++) {
				payment=new Payment();
				payment.PayNum       = PIn.Long  (table.Rows[i]["PayNum"].ToString());
				payment.PayType      = PIn.Long  (table.Rows[i]["PayType"].ToString());
				payment.PayDate      = PIn.Date  (table.Rows[i]["PayDate"].ToString());
				payment.PayAmt       = PIn.Double(table.Rows[i]["PayAmt"].ToString());
				payment.CheckNum     = PIn.String(table.Rows[i]["CheckNum"].ToString());
				payment.BankBranch   = PIn.String(table.Rows[i]["BankBranch"].ToString());
				payment.PayNote      = PIn.String(table.Rows[i]["PayNote"].ToString());
				payment.IsSplit      = PIn.Bool  (table.Rows[i]["IsSplit"].ToString());
				payment.PatNum       = PIn.Long  (table.Rows[i]["PatNum"].ToString());
				payment.ClinicNum    = PIn.Long  (table.Rows[i]["ClinicNum"].ToString());
				payment.DateEntry    = PIn.Date  (table.Rows[i]["DateEntry"].ToString());
				payment.DepositNum   = PIn.Long  (table.Rows[i]["DepositNum"].ToString());
				payment.Receipt      = PIn.String(table.Rows[i]["Receipt"].ToString());
				payment.IsRecurringCC= PIn.Bool  (table.Rows[i]["IsRecurringCC"].ToString());
				retVal.Add(payment);
			}
			return retVal;
		}

		///<summary>Inserts one Payment into the database.  Returns the new priKey.</summary>
		public static long Insert(Payment payment){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				payment.PayNum=DbHelper.GetNextOracleKey("payment","PayNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(payment,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							payment.PayNum++;
							loopcount++;
						}
						else{
							throw ex;
						}
					}
				}
				throw new ApplicationException("Insert failed.  Could not generate primary key.");
			}
			else {
				return Insert(payment,false);
			}
		}

		///<summary>Inserts one Payment into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(Payment payment,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				payment.PayNum=ReplicationServers.GetKey("payment","PayNum");
			}
			string command="INSERT INTO payment (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="PayNum,";
			}
			command+="PayType,PayDate,PayAmt,CheckNum,BankBranch,PayNote,IsSplit,PatNum,ClinicNum,DateEntry,DepositNum,Receipt,IsRecurringCC) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(payment.PayNum)+",";
			}
			command+=
				     POut.Long  (payment.PayType)+","
				+    POut.Date  (payment.PayDate)+","
				+"'"+POut.Double(payment.PayAmt)+"',"
				+"'"+POut.String(payment.CheckNum)+"',"
				+"'"+POut.String(payment.BankBranch)+"',"
				+"'"+POut.String(payment.PayNote)+"',"
				+    POut.Bool  (payment.IsSplit)+","
				+    POut.Long  (payment.PatNum)+","
				+    POut.Long  (payment.ClinicNum)+","
				+    DbHelper.Now()+","
				+    POut.Long  (payment.DepositNum)+","
				+DbHelper.ParamChar+"paramReceipt,"
				+    POut.Bool  (payment.IsRecurringCC)+")";
			if(payment.Receipt==null) {
				payment.Receipt="";
			}
			OdSqlParameter paramReceipt=new OdSqlParameter("paramReceipt",OdDbType.Text,payment.Receipt);
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramReceipt);
			}
			else {
				payment.PayNum=Db.NonQ(command,true,paramReceipt);
			}
			return payment.PayNum;
		}

		///<summary>Updates one Payment in the database.</summary>
		public static void Update(Payment payment){
			string command="UPDATE payment SET "
				+"PayType      =  "+POut.Long  (payment.PayType)+", "
				+"PayDate      =  "+POut.Date  (payment.PayDate)+", "
				+"PayAmt       = '"+POut.Double(payment.PayAmt)+"', "
				+"CheckNum     = '"+POut.String(payment.CheckNum)+"', "
				+"BankBranch   = '"+POut.String(payment.BankBranch)+"', "
				+"PayNote      = '"+POut.String(payment.PayNote)+"', "
				+"IsSplit      =  "+POut.Bool  (payment.IsSplit)+", "
				+"PatNum       =  "+POut.Long  (payment.PatNum)+", "
				+"ClinicNum    =  "+POut.Long  (payment.ClinicNum)+", "
				//DateEntry not allowed to change
				//DepositNum excluded from update
				+"Receipt      =  "+DbHelper.ParamChar+"paramReceipt, "
				+"IsRecurringCC=  "+POut.Bool  (payment.IsRecurringCC)+" "
				+"WHERE PayNum = "+POut.Long(payment.PayNum);
			if(payment.Receipt==null) {
				payment.Receipt="";
			}
			OdSqlParameter paramReceipt=new OdSqlParameter("paramReceipt",OdDbType.Text,payment.Receipt);
			Db.NonQ(command,paramReceipt);
		}

		///<summary>Updates one Payment in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.</summary>
		public static void Update(Payment payment,Payment oldPayment){
			string command="";
			if(payment.PayType != oldPayment.PayType) {
				if(command!=""){ command+=",";}
				command+="PayType = "+POut.Long(payment.PayType)+"";
			}
			if(payment.PayDate != oldPayment.PayDate) {
				if(command!=""){ command+=",";}
				command+="PayDate = "+POut.Date(payment.PayDate)+"";
			}
			if(payment.PayAmt != oldPayment.PayAmt) {
				if(command!=""){ command+=",";}
				command+="PayAmt = '"+POut.Double(payment.PayAmt)+"'";
			}
			if(payment.CheckNum != oldPayment.CheckNum) {
				if(command!=""){ command+=",";}
				command+="CheckNum = '"+POut.String(payment.CheckNum)+"'";
			}
			if(payment.BankBranch != oldPayment.BankBranch) {
				if(command!=""){ command+=",";}
				command+="BankBranch = '"+POut.String(payment.BankBranch)+"'";
			}
			if(payment.PayNote != oldPayment.PayNote) {
				if(command!=""){ command+=",";}
				command+="PayNote = '"+POut.String(payment.PayNote)+"'";
			}
			if(payment.IsSplit != oldPayment.IsSplit) {
				if(command!=""){ command+=",";}
				command+="IsSplit = "+POut.Bool(payment.IsSplit)+"";
			}
			if(payment.PatNum != oldPayment.PatNum) {
				if(command!=""){ command+=",";}
				command+="PatNum = "+POut.Long(payment.PatNum)+"";
			}
			if(payment.ClinicNum != oldPayment.ClinicNum) {
				if(command!=""){ command+=",";}
				command+="ClinicNum = "+POut.Long(payment.ClinicNum)+"";
			}
			//DateEntry not allowed to change
			//DepositNum excluded from update
			if(payment.Receipt != oldPayment.Receipt) {
				if(command!=""){ command+=",";}
				command+="Receipt = "+DbHelper.ParamChar+"paramReceipt";
			}
			if(payment.IsRecurringCC != oldPayment.IsRecurringCC) {
				if(command!=""){ command+=",";}
				command+="IsRecurringCC = "+POut.Bool(payment.IsRecurringCC)+"";
			}
			if(command==""){
				return;
			}
			if(payment.Receipt==null) {
				payment.Receipt="";
			}
			OdSqlParameter paramReceipt=new OdSqlParameter("paramReceipt",OdDbType.Text,payment.Receipt);
			command="UPDATE payment SET "+command
				+" WHERE PayNum = "+POut.Long(payment.PayNum);
			Db.NonQ(command,paramReceipt);
		}

		///<summary>Deletes one Payment from the database.</summary>
		public static void Delete(long payNum){
			string command="DELETE FROM payment "
				+"WHERE PayNum = "+POut.Long(payNum);
			Db.NonQ(command);
		}

	}
}