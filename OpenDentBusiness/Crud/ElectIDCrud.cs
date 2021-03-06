//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class ElectIDCrud {
		///<summary>Gets one ElectID object from the database using the primary key.  Returns null if not found.</summary>
		public static ElectID SelectOne(long electIDNum){
			string command="SELECT * FROM electid "
				+"WHERE ElectIDNum = "+POut.Long(electIDNum);
			List<ElectID> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one ElectID object from the database using a query.</summary>
		public static ElectID SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<ElectID> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of ElectID objects from the database using a query.</summary>
		public static List<ElectID> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<ElectID> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<ElectID> TableToList(DataTable table){
			List<ElectID> retVal=new List<ElectID>();
			ElectID electID;
			for(int i=0;i<table.Rows.Count;i++) {
				electID=new ElectID();
				electID.ElectIDNum   = PIn.Long  (table.Rows[i]["ElectIDNum"].ToString());
				electID.PayorID      = PIn.String(table.Rows[i]["PayorID"].ToString());
				electID.CarrierName  = PIn.String(table.Rows[i]["CarrierName"].ToString());
				electID.IsMedicaid   = PIn.Bool  (table.Rows[i]["IsMedicaid"].ToString());
				electID.ProviderTypes= PIn.String(table.Rows[i]["ProviderTypes"].ToString());
				electID.Comments     = PIn.String(table.Rows[i]["Comments"].ToString());
				retVal.Add(electID);
			}
			return retVal;
		}

		///<summary>Inserts one ElectID into the database.  Returns the new priKey.</summary>
		public static long Insert(ElectID electID){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				electID.ElectIDNum=DbHelper.GetNextOracleKey("electid","ElectIDNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(electID,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							electID.ElectIDNum++;
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
				return Insert(electID,false);
			}
		}

		///<summary>Inserts one ElectID into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(ElectID electID,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				electID.ElectIDNum=ReplicationServers.GetKey("electid","ElectIDNum");
			}
			string command="INSERT INTO electid (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="ElectIDNum,";
			}
			command+="PayorID,CarrierName,IsMedicaid,ProviderTypes,Comments) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(electID.ElectIDNum)+",";
			}
			command+=
				 "'"+POut.String(electID.PayorID)+"',"
				+"'"+POut.String(electID.CarrierName)+"',"
				+    POut.Bool  (electID.IsMedicaid)+","
				+"'"+POut.String(electID.ProviderTypes)+"',"
				+"'"+POut.String(electID.Comments)+"')";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				electID.ElectIDNum=Db.NonQ(command,true);
			}
			return electID.ElectIDNum;
		}

		///<summary>Updates one ElectID in the database.</summary>
		public static void Update(ElectID electID){
			string command="UPDATE electid SET "
				+"PayorID      = '"+POut.String(electID.PayorID)+"', "
				+"CarrierName  = '"+POut.String(electID.CarrierName)+"', "
				+"IsMedicaid   =  "+POut.Bool  (electID.IsMedicaid)+", "
				+"ProviderTypes= '"+POut.String(electID.ProviderTypes)+"', "
				+"Comments     = '"+POut.String(electID.Comments)+"' "
				+"WHERE ElectIDNum = "+POut.Long(electID.ElectIDNum);
			Db.NonQ(command);
		}

		///<summary>Updates one ElectID in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.</summary>
		public static void Update(ElectID electID,ElectID oldElectID){
			string command="";
			if(electID.PayorID != oldElectID.PayorID) {
				if(command!=""){ command+=",";}
				command+="PayorID = '"+POut.String(electID.PayorID)+"'";
			}
			if(electID.CarrierName != oldElectID.CarrierName) {
				if(command!=""){ command+=",";}
				command+="CarrierName = '"+POut.String(electID.CarrierName)+"'";
			}
			if(electID.IsMedicaid != oldElectID.IsMedicaid) {
				if(command!=""){ command+=",";}
				command+="IsMedicaid = "+POut.Bool(electID.IsMedicaid)+"";
			}
			if(electID.ProviderTypes != oldElectID.ProviderTypes) {
				if(command!=""){ command+=",";}
				command+="ProviderTypes = '"+POut.String(electID.ProviderTypes)+"'";
			}
			if(electID.Comments != oldElectID.Comments) {
				if(command!=""){ command+=",";}
				command+="Comments = '"+POut.String(electID.Comments)+"'";
			}
			if(command==""){
				return;
			}
			command="UPDATE electid SET "+command
				+" WHERE ElectIDNum = "+POut.Long(electID.ElectIDNum);
			Db.NonQ(command);
		}

		///<summary>Deletes one ElectID from the database.</summary>
		public static void Delete(long electIDNum){
			string command="DELETE FROM electid "
				+"WHERE ElectIDNum = "+POut.Long(electIDNum);
			Db.NonQ(command);
		}

	}
}