﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using OpenDentBusiness;

namespace OpenDental{
	public class ApptViewItemL{
		///<summary>A list of the ApptViewItems for the current view.</summary>
		public static List<ApptViewItem> ForCurView;
		//these two are subsets of provs and ops. You can't include hidden prov or op in this list.
		///<summary>Visible provider bars in appt module.  List of indices to ProviderC.List(short).  Also see VisOps.  This is a subset of the available provs.  You can't include a hidden prov in this list.</summary>
		public static List<int> VisProvs;
		///<summary>Visible ops in appt module.  List of indices to Operatories.ListShort[ops].  Also see VisProvs.  This is a subset of the available ops.  You can't include a hidden op in this list.  If user has set View.OnlyScheduledProvs, and not isWeekly, then the only opsto show will be for providers that have schedules for the day and ops with no provs assigned.</summary>
		public static List<int> VisOps;
		///<summary>Subset of ForCurView. Just items for rowElements, including apptfielddefs. If no view is selected, then the elements are filled with default info.</summary>
		public static List<ApptViewItem> ApptRows;
		public static ApptView ApptViewCur;

		public static void GetForCurView(int indexInList,bool isWeekly,List<Schedule> dailySched) {
			if(indexInList<0){//might be -1 or -2
				GetForCurView(null,isWeekly,dailySched);
			}
			else{
				GetForCurView(ApptViewC.List[indexInList],isWeekly,dailySched);
			}
		}

		///<summary>Gets (list)ForCurView, VisOps, VisProvs, and ApptRows.  Also sets TwoRows. Works even if supply -1 to indicate no apptview is selected.  Pass in null for the dailySched if this is a weekly view or if in FormApptViewEdit.</summary>
		public static void GetForCurView(ApptView av,bool isWeekly,List<Schedule> dailySched){
			ApptViewCur=av;
			ForCurView=new List<ApptViewItem>();
			VisProvs=new List<int>();
			VisOps=new List<int>();
			ApptRows=new List<ApptViewItem>();
			int index;
			//If there are no appointment views set up (therefore, none selected), then use a hard-coded default view.
			if(ApptViewCur==null){
				//MessageBox.Show("apptcategorynum:"+ApptCategories.Cur.ApptCategoryNum.ToString());
				//make visible ops exactly the same as the short ops list (all except hidden)
				for(int i=0;i<OperatoryC.ListShort.Count;i++){
					VisOps.Add(i);
				}
				//make visible provs exactly the same as the prov list (all except hidden)
				for(int i=0;i<ProviderC.List.Length;i++){
					VisProvs.Add(i);
				}
				//Hard coded elements showing
				ApptRows.Add(new ApptViewItem("PatientName",0,Color.Black));
				ApptRows.Add(new ApptViewItem("ASAP",1,Color.DarkRed));
				ApptRows.Add(new ApptViewItem("MedUrgNote",2,Color.DarkRed));
				ApptRows.Add(new ApptViewItem("PremedFlag",3,Color.DarkRed));
				ApptRows.Add(new ApptViewItem("Lab",4,Color.DarkRed));
				ApptRows.Add(new ApptViewItem("Procs",5,Color.Black));
				ApptRows.Add(new ApptViewItem("Note",6,Color.Black));
				ContrApptSheet.RowsPerIncr=1;
			}
			//An appointment view is selected, so add provs and ops from the view to our lists of indexes.
			else{
				for(int i=0;i<ApptViewItemC.List.Length;i++){
					if(ApptViewItemC.List[i].ApptViewNum==ApptViewCur.ApptViewNum){
						ForCurView.Add(ApptViewItemC.List[i]);
						if(ApptViewItemC.List[i].OpNum>0){//op
							if(ApptViewCur.OnlyScheduledProvs && !isWeekly) {
								continue;//handled below
							}
							index=Operatories.GetOrder(ApptViewItemC.List[i].OpNum);
							if(index!=-1){
								VisOps.Add(index);
							}
						}
						else if(ApptViewItemC.List[i].ProvNum>0){//prov
							index=Providers.GetIndex(ApptViewItemC.List[i].ProvNum);
							if(index!=-1){
								VisProvs.Add(index);
							}
						}
						else{//element or apptfielddef
							ApptRows.Add(ApptViewItemC.List[i]);
						}
					}
				}
				ContrApptSheet.RowsPerIncr=ApptViewCur.RowsPerIncr;
			}
			//if this appt view has the option to show only scheduled providers and this is daily view.
			//Remember that there is no intelligence in weekly view for this option, and it behaves just like it always did.
			if(ApptViewCur!=null && ApptViewCur.OnlyScheduledProvs && !isWeekly) {
				//intelligently decide what ops to show.  It's based on the schedule for the day.
				//VisOps will be totally empty right now because it looped out of the above section of code.
				List<long> listSchedOps;
				bool opAdded;
				int indexOp;
				for(int i=0;i<OperatoryC.ListShort.Count;i++){//loop through all ops for all views (except the hidden ones, of course)
					//find any applicable sched for the op
					opAdded=false;
					for(int s=0;s<dailySched.Count;s++){
						if(dailySched[s].SchedType!=ScheduleType.Provider){
							continue;
						}
						if(dailySched[s].StartTime==new TimeSpan(0)) {//skip if block starts at midnight.
							continue;
						}
						if(dailySched[s].StartTime==dailySched[s].StopTime) {//skip if block has no length.
							continue;
						}
						if(ApptViewCur.OnlySchedAfterTime > new TimeSpan(0,0,0)) {
							if(dailySched[s].StartTime < ApptViewCur.OnlySchedAfterTime
								|| dailySched[s].StopTime < ApptViewCur.OnlySchedAfterTime) 
							{
								continue;
							}
						}
						if(ApptViewCur.OnlySchedBeforeTime > new TimeSpan(0,0,0)) {
							if(dailySched[s].StartTime > ApptViewCur.OnlySchedBeforeTime
								|| dailySched[s].StopTime > ApptViewCur.OnlySchedBeforeTime) 
							{
								continue;
							}
						}
						//this 'sched' must apply to this situation.
						//listSchedOps is the ops for this 'sched'.
						listSchedOps=dailySched[s].Ops;
						//Add all the ops for this 'sched' to the list of visible ops
						for(int p=0;p<listSchedOps.Count;p++) {
							if(listSchedOps[p]==OperatoryC.ListShort[i].OperatoryNum) {
								indexOp=Operatories.GetOrder(listSchedOps[p]);
								if(indexOp!=-1 && !VisOps.Contains(indexOp)) {//prevents adding duplicate ops
									VisOps.Add(indexOp);
									opAdded=true;
									break;
								}
							}
						}
						//Also add any ops that are assigned to this dentist by default.
						if(OperatoryC.ListShort[i].ProvDentist==dailySched[s].ProvNum) {
							indexOp=Operatories.GetOrder(OperatoryC.ListShort[i].OperatoryNum);
							if(indexOp!=-1 && !VisOps.Contains(indexOp)) {
								VisOps.Add(indexOp);
								opAdded=true;
							}
							//index=Providers.GetIndex(OperatoryC.ListShort[i].ProvDentist);
							//if(index!=-1 && !VisProvs.Contains(index)) {
							//	VisProvs.Add(index);
							//}
						}
						if(opAdded) {
							break;//break out of the loop of schedules.  Continue with the next op.
						}
					}
				}
			}
			VisOps.Sort();
			VisProvs.Sort();
		}

		///<summary>Returns the index of the provNum within VisProvs.</summary>
		public static int GetIndexProv(long provNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<VisProvs.Count;i++) {
				if(ProviderC.List[VisProvs[i]].ProvNum==provNum)
					return i;
			}
			return -1;
		}

		///<summary>Only used in FormApptViewEdit. Must have run GetForCurView first.</summary>
		public static bool OpIsInView(long opNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<ForCurView.Count;i++) {
				if(ForCurView[i].OpNum==opNum)
					return true;
			}
			return false;
		}

		///<summary>Only used in ApptViewItem setup. Must have run GetForCurView first.</summary>
		public static bool ProvIsInView(long provNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<ForCurView.Count;i++) {
				if(ForCurView[i].ProvNum==provNum)
					return true;
			}
			return false;
		}

		///<summary>Returns the index of the opNum within VisOps.  Returns -1 if not in visOps.</summary>
		public static int GetIndexOp(long opNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<VisOps.Count;i++) {
				if(OperatoryC.ListShort[VisOps[i]].OperatoryNum==opNum)
					return i;
			}
			return -1;
		}



	}
}
