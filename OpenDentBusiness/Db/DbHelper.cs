﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDentBusiness {
	///<summary>This class contains methods used to generate database independent SQL.</summary>
	public class DbHelper {

		///<summary>Use when you already have a WHERE clause in the query. Uses AND RowNum... for Oracle.</summary>
		public static string LimitAnd(int n) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				return "AND RowNum <= " + n;
			}
			else {
				return "LIMIT " + n;
			}
		}

		///<summary>Use when you do not otherwise have a WHERE clause in the query. Uses WHERE RowNum... for Oracle.</summary>
		public static string LimitWhere(int n) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				return "WHERE RowNum <= " + n;
			}
			else {
				return "LIMIT " + n;
			}
		}

		/*
		///<summary>Use when there is a GROUP BY clause in the query. Uses HAVING RowNum... for Oracle. Needs testing. May need to use subquery instead (like with ORDER BY).</summary>
		public static string LimitHaving(int n) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				return "HAVING RowNum <= " + n;
			}
			else {
				return "LIMIT " + n;
			}
		}*/

		///<summary>Use when there is an ORDER BY clause in the query. Uses RowNum... for Oracle.</summary>
		public static string LimitOrderBy(string query,int n) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				return "SELECT * FROM (" + query + ") WHERE RowNum <= " + n;
			}
			else {
				return query + " LIMIT " + n;
			}
		}

		/// <summary>If passing in a literal, surround with single quotes first.</summary>
		public static string Concat(params string[] values) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				string result="(";
				for(int i=0;i<values.Length;i++) {
					if(i!=0) {
						result+=" || ";
					}
					result+=values[i];
				}
				result+=")";
				return result;
			}
			else {
				string result="CONCAT(";
				for(int i=0;i<values.Length; i++) {
					if(i!=0) {
						result+=",";
					}
					result+=values[i];
				}
				result+=")";
				return result;
			}
		}


	}
}
