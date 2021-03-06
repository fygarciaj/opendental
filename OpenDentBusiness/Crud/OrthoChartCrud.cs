//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class OrthoChartCrud {
		///<summary>Gets one OrthoChart object from the database using the primary key.  Returns null if not found.</summary>
		public static OrthoChart SelectOne(long orthoChartNum){
			string command="SELECT * FROM orthochart "
				+"WHERE OrthoChartNum = "+POut.Long(orthoChartNum);
			List<OrthoChart> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one OrthoChart object from the database using a query.</summary>
		public static OrthoChart SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<OrthoChart> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of OrthoChart objects from the database using a query.</summary>
		public static List<OrthoChart> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<OrthoChart> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<OrthoChart> TableToList(DataTable table){
			List<OrthoChart> retVal=new List<OrthoChart>();
			OrthoChart orthoChart;
			for(int i=0;i<table.Rows.Count;i++) {
				orthoChart=new OrthoChart();
				orthoChart.OrthoChartNum= PIn.Long  (table.Rows[i]["OrthoChartNum"].ToString());
				orthoChart.PatNum       = PIn.Long  (table.Rows[i]["PatNum"].ToString());
				orthoChart.DateService  = PIn.Date  (table.Rows[i]["DateService"].ToString());
				orthoChart.FieldName    = PIn.String(table.Rows[i]["FieldName"].ToString());
				orthoChart.FieldValue   = PIn.String(table.Rows[i]["FieldValue"].ToString());
				retVal.Add(orthoChart);
			}
			return retVal;
		}

		///<summary>Inserts one OrthoChart into the database.  Returns the new priKey.</summary>
		public static long Insert(OrthoChart orthoChart){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				orthoChart.OrthoChartNum=DbHelper.GetNextOracleKey("orthochart","OrthoChartNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(orthoChart,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							orthoChart.OrthoChartNum++;
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
				return Insert(orthoChart,false);
			}
		}

		///<summary>Inserts one OrthoChart into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(OrthoChart orthoChart,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				orthoChart.OrthoChartNum=ReplicationServers.GetKey("orthochart","OrthoChartNum");
			}
			string command="INSERT INTO orthochart (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="OrthoChartNum,";
			}
			command+="PatNum,DateService,FieldName,FieldValue) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(orthoChart.OrthoChartNum)+",";
			}
			command+=
				     POut.Long  (orthoChart.PatNum)+","
				+    POut.Date  (orthoChart.DateService)+","
				+"'"+POut.String(orthoChart.FieldName)+"',"
				+"'"+POut.String(orthoChart.FieldValue)+"')";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				orthoChart.OrthoChartNum=Db.NonQ(command,true);
			}
			return orthoChart.OrthoChartNum;
		}

		///<summary>Updates one OrthoChart in the database.</summary>
		public static void Update(OrthoChart orthoChart){
			string command="UPDATE orthochart SET "
				+"PatNum       =  "+POut.Long  (orthoChart.PatNum)+", "
				+"DateService  =  "+POut.Date  (orthoChart.DateService)+", "
				+"FieldName    = '"+POut.String(orthoChart.FieldName)+"', "
				+"FieldValue   = '"+POut.String(orthoChart.FieldValue)+"' "
				+"WHERE OrthoChartNum = "+POut.Long(orthoChart.OrthoChartNum);
			Db.NonQ(command);
		}

		///<summary>Updates one OrthoChart in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.</summary>
		public static void Update(OrthoChart orthoChart,OrthoChart oldOrthoChart){
			string command="";
			if(orthoChart.PatNum != oldOrthoChart.PatNum) {
				if(command!=""){ command+=",";}
				command+="PatNum = "+POut.Long(orthoChart.PatNum)+"";
			}
			if(orthoChart.DateService != oldOrthoChart.DateService) {
				if(command!=""){ command+=",";}
				command+="DateService = "+POut.Date(orthoChart.DateService)+"";
			}
			if(orthoChart.FieldName != oldOrthoChart.FieldName) {
				if(command!=""){ command+=",";}
				command+="FieldName = '"+POut.String(orthoChart.FieldName)+"'";
			}
			if(orthoChart.FieldValue != oldOrthoChart.FieldValue) {
				if(command!=""){ command+=",";}
				command+="FieldValue = '"+POut.String(orthoChart.FieldValue)+"'";
			}
			if(command==""){
				return;
			}
			command="UPDATE orthochart SET "+command
				+" WHERE OrthoChartNum = "+POut.Long(orthoChart.OrthoChartNum);
			Db.NonQ(command);
		}

		///<summary>Deletes one OrthoChart from the database.</summary>
		public static void Delete(long orthoChartNum){
			string command="DELETE FROM orthochart "
				+"WHERE OrthoChartNum = "+POut.Long(orthoChartNum);
			Db.NonQ(command);
		}

	}
}