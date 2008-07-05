﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using OpenDental.DataAccess;

namespace OpenDentBusiness{
	///<summary></summary>
	public class Sites{
		///<summary></summary>
		public static DataTable RefreshCache(){
			string c="SELECT * from site ORDER BY Description";
			DataTable table=General.GetTable(c);
			table.TableName="Site";
			FillCache(table);
			return table;
		}

		public static void FillCache(DataTable table){
			SiteC.List=new Site[table.Rows.Count];
			for(int i=0;i<SiteC.List.Length;i++){
				SiteC.List[i]=new Site();
				SiteC.List[i].IsNew=false;
				SiteC.List[i].SiteNum    = PIn.PInt   (table.Rows[i][0].ToString());
				SiteC.List[i].Description= PIn.PString(table.Rows[i][1].ToString());
				SiteC.List[i].Note       = PIn.PString(table.Rows[i][2].ToString());
			}
		}

		///<Summary>Gets one Site from the database.</Summary>
		public static Site CreateObject(int siteNum){
			return DataObjectFactory<Site>.CreateObject(siteNum);
		}

		public static List<Site> GetSites(int[] siteNums){
			Collection<Site> collectState=DataObjectFactory<Site>.CreateObjects(siteNums);
			return new List<Site>(collectState);		
		}

		///<summary></summary>
		public static void WriteObject(Site site){
			DataObjectFactory<Site>.WriteObject(site);
		}

		///<summary></summary>
		public static void DeleteObject(int siteNum){
			//validate that not already in use.
			string command="SELECT LName,FName FROM patient WHERE SiteNum="+POut.PInt(siteNum);
			DataTable table=General.GetTable(command);
			//int count=PIn.PInt(General.GetCount(command));
			string pats="";
			for(int i=0;i<table.Rows.Count;i++){
				if(i>0){
					pats+=", ";
				}
				pats+=table.Rows[i]["FName"].ToString()+" "+table.Rows[i]["LName"].ToString();
			}
			if(table.Rows.Count>0){
				throw new ApplicationException(Lan.g("Sites","Site is already in use by patient(s). Not allowed to delete. "+pats));
			}
			DataObjectFactory<Site>.DeleteObject(siteNum);
		}

		//public static void DeleteObject(int siteNum){
		//	DataObjectFactory<Site>.DeleteObject(siteNum);
		//}

		public static string GetDescription(int siteNum){
			if(siteNum==0){
				return "";
			}
			for(int i=0;i<SiteC.List.Length;i++){
				if(SiteC.List[i].SiteNum==siteNum){
					return SiteC.List[i].Description;
				}
			}
			return "";
		}

	}
}