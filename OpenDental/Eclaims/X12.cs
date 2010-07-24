using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;

namespace OpenDental.Eclaims
{
	/// <summary>
	/// Summary description for X12.
	/// </summary>
	public class X12{
		///<summary></summary>
		public X12()
		{
			
		}

		///<summary>Gets the filename for this batch. Used when saving or when rolling back.</summary>
		private static string GetFileName(Clearinghouse clearhouse,int batchNum){
			string saveFolder=clearhouse.ExportPath;
			if(!Directory.Exists(saveFolder)) {
				MessageBox.Show(saveFolder+" not found.");
				return "";
			}
			if(clearhouse.CommBridge==EclaimsCommBridge.RECS){
				if(File.Exists(ODFileUtils.CombinePaths(saveFolder,"ecs.txt"))){
					MsgBox.Show("FormClaimsSend","You must send your existing claims from the RECS program before you can create another batch.");
					return "";//prevents overwriting an existing ecs.txt.
				}
				return ODFileUtils.CombinePaths(saveFolder,"ecs.txt");
			}
			else{
				return ODFileUtils.CombinePaths(saveFolder,"claims"+batchNum.ToString()+".txt");
			}
		}

		///<summary>If file creation was successful but communications failed, then this deletes the X12 file.  This is not used in the Tesia bridge because of the unique filenaming.</summary>
		public static void Rollback(Clearinghouse clearhouse,int batchNum){
			if(clearhouse.CommBridge==EclaimsCommBridge.RECS){
				//A RECS rollback never deletes the file, because there is only one
			}
			else{
				//This is a Windows extension, so we do not need to worry about Unix path separator characters.
				File.Delete(ODFileUtils.CombinePaths(clearhouse.ExportPath,"claims"+batchNum.ToString()+".txt"));
			}
		}

		///<summary>Called from Eclaims and includes multiple claims.  Returns the string that was sent.  The string needs to be parsed to determine the transaction numbers used for each claim.</summary>
		public static string SendBatch(List<ClaimSendQueueItem> queueItems,int batchNum){
			Clearinghouse clearhouse=ClearinghouseL.GetClearinghouse(queueItems[0].ClearinghouseNum);
			List<ClaimSendQueueItem> functionalGroupDental=new List<ClaimSendQueueItem>();
			List<ClaimSendQueueItem> functionalGroupMedical=new List<ClaimSendQueueItem>();
			for(int i=0;i<queueItems.Count;i++){
				if(queueItems[i].IsMedical){
					functionalGroupMedical.Add(queueItems[i]);
				}
				else{
					functionalGroupDental.Add(queueItems[i]);
				}
			}
			if(functionalGroupMedical.Count>0 && functionalGroupDental.Count>0) {
				MessageBox.Show(Lan.g("X12","You must send dental and medical claims in separate batches."));
				return "";//not allowed
			}
			string saveFile=GetFileName(clearhouse,batchNum);
			if(saveFile==""){
				return "";
			}
			using(StreamWriter sw=new StreamWriter(saveFile,false,Encoding.ASCII))
			{
				//Interchange Control Header (Interchange number tracked separately from transactionNum)
				//We set it to between 1 and 999 for simplicity
				sw.Write("ISA*00*          *"//ISA01,ISA02: 00 + 10 spaces
					+"00*          *"//ISA03,ISA04: 00 + 10 spaces
					+clearhouse.ISA05+"*"//ISA05: Sender ID type: ZZ=mutually defined. 30=TIN. Validated
					+X12Generator.GetISA06(clearhouse)+"*"//ISA06: Sender ID(TIN). Or might be TIN of Open Dental
					+clearhouse.ISA07+"*"//ISA07: Receiver ID type: ZZ=mutually defined. 30=TIN. Validated
					+Sout(clearhouse.ISA08,15,15)+"*"//ISA08: Receiver ID. Validated to make sure length is at least 2.
					+DateTime.Today.ToString("yyMMdd")+"*"//ISA09: today's date
					+DateTime.Now.ToString("HHmm")+"*"//ISA10: current time
					+"U*00401*"//ISA11 and ISA12. 
					//ISA13: interchange control number, right aligned:
					+batchNum.ToString().PadLeft(9,'0')+"*"
					+"0*"//ISA14: no acknowledgment requested
					+clearhouse.ISA15+"*");//ISA15: T=Test P=Production. Validated.
					sw.WriteLine(":~");//ISA16: use ':'
				//Functional groups: one for dental and one for medical
				//But we instead need to restrict file output to either medical OR dental, not both.
				//So this part is changing.  One or the other.
				if(functionalGroupMedical.Count>0){
					WriteFunctionalGroup(sw,functionalGroupMedical,batchNum,clearhouse);
				}
				if(functionalGroupDental.Count>0){
					WriteFunctionalGroup(sw,functionalGroupDental,batchNum,clearhouse);
				}
				//Interchange Control Trailer
				sw.WriteLine("IEA*1*"//IEA01: number of functional groups
					+batchNum.ToString().PadLeft(9,'0')+"~");//IEA02: Interchange control number
			}//using sw
			if(clearhouse.CommBridge==EclaimsCommBridge.PostnTrack){
				//need to clear out all CRLF from entire file
				string strFile="";
				using(StreamReader sr=new StreamReader(saveFile,Encoding.ASCII)){
					strFile=sr.ReadToEnd();
				}
				strFile=strFile.Replace("\r","");
				strFile=strFile.Replace("\n","");
				using(StreamWriter sw=new StreamWriter(saveFile,false,Encoding.ASCII)){
					sw.Write(strFile);
				}
			}
			CopyToArchive(saveFile);
			return File.ReadAllText(saveFile);
		}

		private static void WriteFunctionalGroup(StreamWriter sw,List<ClaimSendQueueItem> queueItems,int batchNum,Clearinghouse clearhouse){
			int transactionNum=1;//Gets incremented for each carrier. Can be reused in other functional groups and interchanges, so not persisted
			//Functional Group Header
			string groupControlNumber=batchNum.ToString();//Must be unique within file.  We will use batchNum
			bool isMedical=queueItems[0].IsMedical;
			//this needs to be changed.  Medical should not be sent with dental.
			if(isMedical){
				//groupControlNumber="2";//this works for now because only two groups
				sw.WriteLine("GS*HC*"//GS01: Health Care Claim
					+X12Generator.GetGS02(clearhouse)+"*"//GS02: Senders Code. Sometimes OpenDental.  Sometimes the sending clinic. Validated
					+Sout(clearhouse.GS03,15,2)+"*"//GS03: Application Receiver's Code
					+DateTime.Today.ToString("yyyyMMdd")+"*"//GS04: today's date
					+DateTime.Now.ToString("HHmm")+"*"//GS05: current time
					+groupControlNumber+"*"//GS06: Group control number. Max length 9. No padding necessary. 
					+"X*"//GS07: X
					+"005010X222~");//GS08: Version
			}
			else{//dental
				//groupControlNumber="1";
				sw.WriteLine("GS*HC*"//GS01: Health Care Claim
					+X12Generator.GetGS02(clearhouse)+"*"//GS02: Senders Code. Sometimes Jordan Sparks.  Sometimes the sending clinic.
					+Sout(clearhouse.GS03,15,2)+"*"//GS03: Application Receiver's Code
					+DateTime.Today.ToString("yyyyMMdd")+"*"//GS04: today's date
					+DateTime.Now.ToString("HHmm")+"*"//GS05: current time
					+groupControlNumber+"*"//GS06: Group control number. Max length 9. No padding necessary.
					+"X*"//GS07: X
					+"004010X097A1~");//GS08: Version
			}
			//Gets an array with PayorID,ProvBill,Subscriber,PatNum,ClaimNum all in the correct order
			List<long> claimNums=new List<long>();
			for(int i=0;i<queueItems.Count;i++) {
				claimNums.Add(queueItems[i].ClaimNum);
			}
			List<X12TransactionItem> claimItems=Claims.GetX12TransactionInfo(claimNums);
			bool newTrans;//true if this loop has transaction header
			bool hasFooter;//true if this loop has transaction footer(if the Next loop is a newTrans)
			int HLcount=1;
			int parentProv=0;//the HL sequence # of the current provider.
			int parentSubsc=0;//the HL sequence # of the current subscriber.
			string hasSubord="";//0 if no subordinate, 1 if at least one subordinate
			Claim claim;
			InsPlan insPlan;
			InsPlan otherPlan=new InsPlan();
			Patient patient;
			Patient subscriber;
			Patient otherSubsc=new Patient();
			Carrier carrier;
			Carrier otherCarrier=new Carrier();
			List<ClaimProc> claimProcList;//all claimProcs for a patient.
			List<ClaimProc> claimProcs;
			List<Procedure> procList;
			List<ToothInitial> initialList;
			Procedure proc;
			ProcedureCode procCode;
			Provider provTreat;//might be different for each proc
			Provider billProv=null;
			Clinic clinic=null;
			int seg=0;//segments for a particular ST-SE transaction
			for(int i=0;i<claimItems.Count;i++){
				#region Transaction Set Header
				if(i==0//if this is the first claim
					|| claimItems[i].PayorId0 != claimItems[i-1].PayorId0)//or the payorID has changed
				{
					newTrans=true;
					seg=0;
				}
				else newTrans=false;
				if(newTrans){
					//Transaction Set Header (one for each carrier)
					//transactionNum gets incremented in SE section
					//ST02 Transact. control #. Must be unique within ISA
					//NO: So we used combination of transaction and group, eg 00011
					seg++;
					if(isMedical){
						sw.WriteLine("ST*837*"//ST01
							+transactionNum.ToString().PadLeft(4,'0')+"*"//ST02 4/9
							+"005010X222~");//ST03: Implementation convention reference
					}
					else{//dental
						sw.WriteLine("ST*837*"//ST01
							+transactionNum.ToString().PadLeft(4,'0')+"~");//ST02
					}
					seg++;
					sw.WriteLine("BHT*0019*00*"
						+transactionNum.ToString().PadLeft(4,'0')+"*"//BHT03. Can be same as ST02
						+DateTime.Now.ToString("yyyyMMdd")+"*"//BHT04: Date
						+DateTime.Now.ToString("HHmmss")+"*"//BHT05: Time
						+"CH~");//BHT06: Type=Chargable
					if(!isMedical){
						seg++;
						if(clearhouse.ISA15=="T"){//validated
							sw.WriteLine("REF*87*004010X097DA1~");
						}
						else{
							sw.WriteLine("REF*87*004010X097A1~");
						}
					}
					//1000A Submitter is OPEN DENTAL and sometimes it's the practice
						//(depends on clearinghouse and Partnership agreements)
						//See 2010AA PER (after REF) for the new billing provider contact phone number
					//1000A NM1: required
					seg++;
					Write1000A_NM1(sw,clearhouse);
					//1000A PER: required. Contact number.
					seg++;//always one seg
					Write1000A_PER(sw,clearhouse);
					//1000B Receiver is always the Clearinghouse
					//1000B NM1: required
					seg++;
					sw.WriteLine("NM1*40*"//NM101: 40=receiver
						+"2*"//NM102: 2=nonperson
						+Sout(clearhouse.Description,35,1)+"*"//NM103:Receiver Name
						+"****"//NM104-NM107 not used since not a person
						+"46*"//NM108: 46 indicates ETIN
						+Sout(clearhouse.ISA08,80,2)+"~");//NM109: Receiver ID Code. aka ETIN#.
					HLcount=1;
					parentProv=0;//the HL sequence # of the current provider.
					parentSubsc=0;//the HL sequence # of the current subscriber.
					hasSubord="";//0 if no subordinate, 1 if at least one subordinate
				}
				#endregion
				//HL Loops:
				#region Billing Provider
				if(i==0//if first claim
					|| newTrans //or new Transaction set
					|| claimItems[i].ProvBill1 != claimItems[i-1].ProvBill1)//or prov has changed
				{
					//this is a workaround for finding clinic address.  In OD, address is not a provider level field.  All we have access to is the claim.
					//So providers shouldn't move between clinics.  We will use the clinic of the first claim, which is arbitrary.
					//An improvement would be to generate another loop if provider changes address for different claims.  Complicated.
					if(!PrefC.GetBool(PrefName.EasyNoClinics)){//if using clinics
						Claim clm=Claims.GetClaim(claimItems[i].ClaimNum4);
						long clinicNum=clm.ClinicNum;
						clinic=Clinics.GetClinic(clinicNum);
					}
					//2000A HL: Billing/Pay-to provider HL loop
					seg++;
					sw.WriteLine("HL*"+HLcount.ToString()+"*"//HL01: Heirarchical ID
						+"*"//HL02: No parent. Not used
						+"20*"//HL03: Heirarchical level code. 20=Information source
						+"1~");//HL04: Heirarchical child code. 1=child HL present
					billProv=ProviderC.ListLong[Providers.GetIndexLong(claimItems[i].ProvBill1)];
					if(isMedical){
						//2000A PRV: Provider Specialty Information
						seg++;
						sw.WriteLine("PRV*BI*"//PRV01: Provider Code. BI=Billing
							+"PXC*"//PRV02: taxonomy code
							+X12Generator.GetTaxonomy(billProv)+"~");//PRV03: Provider taxonomy code
					}
					else{//dental
						//2000A PRV: Provider Specialty Information (Optional Rendering prov for all claims in this HL)
						//used instead of 2310B.
						seg++;
						sw.WriteLine("PRV*PT*"//PRV01: Provider Code. BI=Billing, PT=Pay-To
							+"ZZ*"//PRV02: mutually defined taxonomy codes
							+X12Generator.GetTaxonomy(billProv)+"~");//PRV03: Provider taxonomy code
					}
					//2010AA NM1: Billing provider
					seg++;
					sw.Write("NM1*85*");//NM101: 85=Billing provider
					if(billProv.FName==""){
						sw.Write("2*");//NM102: 1=person,2=non-person
					}
					else{
						sw.Write("1*");
					}
					sw.Write(Sout(billProv.LName,35)+"*"//NM103: Last name
						//NM103 allowable length increased to 60?
						+Sout(billProv.FName,25)+"*"//NM104: First name. Might be blank.
						+Sout(billProv.MI,25)+"*"//NM105: Middle name. Since this is optional, there is no min length.
						+"*"//NM106: not used
						+"*");//NM107: Name suffix. not used
					//It's after the NPI date now, so only one choice here:
					sw.Write("XX*");//NM108: ID code qualifier. 24=EIN. 34=SSN, XX=NPI
					sw.WriteLine(Sout(billProv.NationalProvID,80)+"~");//NM109: ID code. NPI validated
					//2010AA N3: Billing provider address
					seg++;
					if(PrefC.GetBool(PrefName.UseBillingAddressOnClaims)) {
						sw.Write("N3*"+Sout(PrefC.GetString(PrefName.PracticeBillingAddress),55));//N301: Address
					}
					else if(clinic==null){
						sw.Write("N3*"+Sout(PrefC.GetString(PrefName.PracticeAddress),55));//N301: Address
					}
					else{
						sw.Write("N3*"+Sout(clinic.Address,55));//N301: Address
					}
					if(PrefC.GetBool(PrefName.UseBillingAddressOnClaims)) {
						if(PrefC.GetString(PrefName.PracticeBillingAddress2)=="") {
							sw.WriteLine("~");
						}
						else {
							//N302: Address2. Optional.
							sw.WriteLine("*"+Sout(PrefC.GetString(PrefName.PracticeBillingAddress2),55)+"~");
						}
					}
					else if(clinic==null) {
						if(PrefC.GetString(PrefName.PracticeAddress2)==""){
							sw.WriteLine("~");
						}
						else{
							//N302: Address2. Optional.
							sw.WriteLine("*"+Sout(PrefC.GetString(PrefName.PracticeAddress2),55)+"~");
						}
					}
					else{
						if(clinic.Address2==""){
							sw.WriteLine("~");
						}
						else{
							//N302: Address2. Optional.
							sw.WriteLine("*"+Sout(clinic.Address2,55)+"~");
						}
					}
					//2010AA N4: Billing prov City,State,Zip
					seg++;
					if(PrefC.GetBool(PrefName.UseBillingAddressOnClaims)) {
						sw.WriteLine("N4*"+Sout(PrefC.GetString(PrefName.PracticeBillingCity),30)+"*"//N401: City
							+Sout(PrefC.GetString(PrefName.PracticeBillingST),2)+"*"//N402: State
							+Sout(PrefC.GetString(PrefName.PracticeBillingZip).Replace("-",""),15)+"~");//N403: Zip
					}
					else if(clinic==null){
						sw.WriteLine("N4*"+Sout(PrefC.GetString(PrefName.PracticeCity),30)+"*"//N401: City
							+Sout(PrefC.GetString(PrefName.PracticeST),2)+"*"//N402: State
							+Sout(PrefC.GetString(PrefName.PracticeZip).Replace("-",""),15)+"~");//N403: Zip
					}
					else{
						sw.WriteLine("N4*"+Sout(clinic.City,30)+"*"//N401: City
							+Sout(clinic.State,2)+"*"//N402: State
							+Sout(clinic.Zip.Replace("-",""),15)+"~");//N403: Zip
					}
					if(!isMedical){
						//2010AA REF: Office phone number. Required by WebMD.  Can possibly be removed now that we're using NPI.
						if(clearhouse.ISA08=="0135WCH00"){//if WebMD
							seg++;
							if(clinic==null){
								sw.WriteLine("REF*LU*"
									+PrefC.GetString(PrefName.PracticePhone)+"~");
							}
							else{
								sw.WriteLine("REF*LU*"
									+clinic.Phone+"~");
							}
						}
						//2010AA REF: License #. Required by RECS clearinghouse,
						//but everyone else should find it useful too.
						seg++;
						sw.WriteLine("REF*0B*"//REF01: 0B=state license #.
							+Sout(billProv.StateLicense,30)+"~");
						//2010AA REF: Secondary ID number(s). Only required by some carriers.
						seg+=WriteProv_REF(sw,billProv,claimItems[i].PayorId0);
					}
					//It's after the NPI date, now.  So either TIN or SSN (EI or SY) is required here.
					seg++;
					sw.Write("REF*");
					if(billProv.UsingTIN) {
						sw.Write("EI*");//REF01: qualifier. EI=EIN
					}
					else {//SSN
						sw.Write("SY*");//REF01: qualifier. SY=SSN
					}
					sw.WriteLine(Sout(billProv.SSN,30)+"~");//REF02: ID #
					if(isMedical){
						//2010AA PER: Billing Provider Contact Info. Required if different than in 1000A
						//We'll always include it for simplicity
						seg++;
						if(clinic==null){
							sw.WriteLine("PER*IC*"//PER01: IC=Information Contact
								+Sout(PrefC.GetString(PrefName.PracticeTitle),60,1)+"*"//PER02:Name. Practice title
								+"TE*"//PER03:Comm Number Qualifier: TE=Telephone
								+Sout(PrefC.GetString(PrefName.PracticePhone),256,1)+"~");//PER04:Comm Number. aka telephone number
						}
						else{
							sw.WriteLine("PER*IC*"//PER01: IC=Information Contact
								+Sout(PrefC.GetString(PrefName.PracticeTitle),60,1)+"*"//PER02:Name. Practice title
								+"TE*"//PER03:Comm Number Qualifier: TE=Telephone
								+Sout(clinic.Phone,256,1)+"~");//PER04:Comm Number. aka telephone number
						}
					}
					parentProv=HLcount;
					HLcount++;
				}
				#endregion Billing Provider
				claim=Claims.GetClaim(claimItems[i].ClaimNum4);				
				insPlan=InsPlans.GetPlan(claim.PlanNum,new List <InsPlan> ());
				//insPlan could be null if db corruption. No error checking for that
				if(claim.PlanNum2>0){
					otherPlan=InsPlans.GetPlan(claim.PlanNum2,new List <InsPlan> ());
					otherSubsc=Patients.GetPat(otherPlan.Subscriber);
					otherCarrier=Carriers.GetCarrier(otherPlan.CarrierNum);
				}
				patient=Patients.GetPat(claim.PatNum);
				subscriber=Patients.GetPat(insPlan.Subscriber);
				carrier=Carriers.GetCarrier(insPlan.CarrierNum);
				claimProcList=ClaimProcs.Refresh(patient.PatNum);
				claimProcs=ClaimProcs.GetForSendClaim(claimProcList,claim.ClaimNum);
				procList=Procedures.Refresh(claim.PatNum);
				initialList=ToothInitials.Refresh(claim.PatNum);
				#region Attachments
				/*if(clearhouse.ISA08=="113504607" && claim.Attachments.Count>0){//If Tesia and has attachments
					claim.ClaimNote="TESIA#"+claim.ClaimNum.ToString()+" "+claim.ClaimNote;
					string saveFolder=clearhouse.ExportPath;//we've already tested to make sure this exists.
					string saveFile;
					string storedFile;
					for(int c=0;c<claim.Attachments.Count;c++){
						saveFile=ODFileUtils.CombinePaths(saveFolder,
							claim.ClaimNum.ToString()+"_"+(c+1).ToString()+Path.GetExtension(claim.Attachments[c].DisplayedFileName)+".IMG");
						storedFile=ODFileUtils.CombinePaths(FormEmailMessageEdit.GetAttachPath(),claim.Attachments[c].ActualFileName);
						File.Clone(storedFile,saveFile,true);
					}					
				}*/
				#endregion
				#region Subscriber
				if(i==0 || claimItems[i].PatNum3 != claimItems[i-1].PatNum3//if patient changed
					|| claimItems[i].ProvBill1 != claimItems[i-1].ProvBill1)//or prov has changed
				{
					if(claimItems[i].PatNum3==claimItems[i].Subscriber2){//if patient is the subscriber
						hasSubord="0";//-claim level will follow
						//subordinate patients will not follow in this loop.  The subscriber loop will be duplicated for them.
					}
					else{//patient is not the subscriber
						hasSubord="1";//-patient will always follow
					}
					//2000B HL: Subscriber HL loop
					seg++;
					sw.WriteLine("HL*"+HLcount.ToString()+"*"//HL01: Heirarchical ID
						+parentProv.ToString()+"*"//HL02: parent is always the provider HL
						+"22*"//HL03: 22=Subscriber
						+hasSubord+"~");//HL04: 1=additional subordinate HL segment (patient). 0=no subord
					//2000B SBR:
					seg++;
					sw.Write("SBR*");
					if(claim.ClaimType=="P"){
						sw.Write("P*");//SBR01: Payer responsibility code
					}
					else if(claim.ClaimType=="S"){
						sw.Write("S*");
					}
					else{
						sw.Write("T*");//T=Tertiary
					}
//todo: what about Cap?
					if(claimItems[i].PatNum3==claimItems[i].Subscriber2){//if patient is the subscriber
						sw.Write("18*");//SBR02: Relationship. 18=self
					}
					else{
						sw.Write("*");//empty if patient is not subscriber.
					}
					sw.Write(Sout(insPlan.GroupNum,30)+"*"//SBR03: Group Number
						+Sout(insPlan.GroupName,60)+"*"//SBR04: Group Name
						+"*");//SBR05: Not used
					if(isMedical){
						sw.Write("*");//SBR06 not used.
					}
					else{
						if(claim.PlanNum2>0){
							sw.Write("1*");//SBR06: 1=Coordination of benefits. 6=No coordination
						}
						else{
							sw.Write("6*");
						}
					}
					sw.Write("**");//SBR07 & 08 not used.
					sw.WriteLine(GetFilingCode(insPlan)+"~");//"CI~");//SBR09: 12=PPO,17=DMO,BL=BCBS,CI=CommercialIns,FI=FEP,HM=HMO
							//,MC=Medicaid,SA=self-administered, etc. 
							//SBR09 will not be used once PlanID is mandated.
					//if(isMedical){
						//2000B PAT. Required when patient is subscriber and one of the fields is needed.
						//We will never need these fields: deceased date, weight, or pregnancy
					//}
					//2010BA NM1: Subscriber Name
					seg++;
					sw.WriteLine("NM1*IL*"//NM101: IL=Insured or Subscriber
						+"1*"//NM102: 1=Person
						+Sout(subscriber.LName,35)+"*"//NM103: LName
						+Sout(subscriber.FName,25)+"*"//NM104: FName
						+Sout(subscriber.MiddleI,25)+"*"//NM105: MiddleName
						+"*"//NM106: not used
						+"*"//NM107: suffix. Not present in Open Dental yet.
						+"MI*"//NM108: MI=MemberID
						+Sout(insPlan.SubscriberID.Replace("-",""),80)+"~");//NM109: Subscriber ID
					//At the request of WebMD, we are including N3,N4,and DMG even if patient is not subscriber.
					//This does not make the transaction non-compliant, and they find it useful.
					//if(claimAr[3,i].ToString()==claimAr[2,i].ToString()){//if patient is the subscriber
					//2010BA N3: Subscriber Address. Only if patient is the subscriber
						seg++;
						sw.Write("N3*"+Sout(subscriber.Address,55,1));//N301: address
							if(subscriber.Address2==""){
								sw.WriteLine("~");
							}
							else{
								//N302: Address2. Optional.
								sw.WriteLine("*"+Sout(subscriber.Address2,55)+"~");
							}
					//2010BA N4: CityStZip. Only if patient is the subscriber
						seg++;
						sw.WriteLine("N4*"
							+Sout(subscriber.City,30,2)+"*"//N401: City
							+Sout(subscriber.State,2,2)+"*"//N402: State
							+Sout(subscriber.Zip.Replace("-",""),15,3)+"~");//N403: Zip
					//2010BA DMG: Subscr. Demographics. Only if patient is the subscriber
						seg++;
						if(subscriber.Birthdate.Year<1900){
							sw.WriteLine("DMG*D8*"//DMG01: use D8
								+subscriber.Birthdate.ToString("19000101")+"*"//DMG02: birthdate
								+GetGender(subscriber.Gender)+"~");//DMG03: gender. F,M,or U
						}
						else{
							sw.WriteLine("DMG*D8*"//DMG01: use D8
								+subscriber.Birthdate.ToString("yyyyMMdd")+"*"//DMG02: birthdate
								+GetGender(subscriber.Gender)+"~");//DMG03: gender. F,M,or U
						}
					//}//if provider is the subscriber
					//2010BA REF: Secondary ID. Situational. Not used.
					//2010BA REF: Casualty Claim number. Not used.
					//Medical: 2010BA PER: Property and casualty subscriber contact info. Not used
					//2010BB: PayerName
					//2010BB NM1: Name
					seg++;
					sw.Write("NM1*PR*"//NM101: PR=Payer
						+"2*");//NM102: 2=Non person
					if(clearhouse.ISA08=="EMS"){
						//This is a special situation requested by EMS.  This tacks the employer onto the end of the carrier.
						sw.Write(Sout(carrier.CarrierName,17)+"|"+Sout(Employers.GetName(insPlan.EmployerNum),17)+"*");
					}
					else{
						sw.Write(Sout(carrier.CarrierName,35)+"*");//NM103: Name. Length can be 60 in the new medical specs.
					}
					sw.Write("****"//NM104-07 not used
						+"PI*");//NM108: PI=PayorID
					string electid=carrier.ElectID;
					if(electid=="" && clearhouse.ISA08=="113504607"){//only for Tesia
						electid="00000";
					}
					if(electid.Length<3){
						electid="06126";
					}
					sw.WriteLine(Sout(electid,80,2)+"~");//NM109: PayorID
					//2010BB N3: Carrier Address
					seg++;
					sw.Write("N3*"+Sout(carrier.Address,55));//N301: address
						if(carrier.Address2==""){
							sw.WriteLine("~");
						}
						else{
							//N302: Address2. Optional.
							sw.WriteLine("*"+Sout(carrier.Address2,55)+"~");
						}
					//2010BB N4: Carrier City,St,Zip
					seg++;
					sw.WriteLine("N4*"
						+Sout(carrier.City,30,2)+"*"//N401: City
						+Sout(carrier.State,2,2)+"*"//N402: State
						+Sout(carrier.Zip.Replace("-",""),15,3)+"~");//N403: Zip
					//2010BB Ref: Payer secondary ID. Not used
					//Credit card info. Not used.
					parentSubsc=HLcount;
					HLcount++;
				}
				#endregion
				#region Patient
				//if((i==0 || claimAr[3,i].ToString() != claimAr[3,i-1].ToString())//if patient changed
				//	&& claimAr[3,i].ToString() != claimAr[2,i].ToString())//AND patient is not subscriber
				//{
				if(claimItems[i].PatNum3 != claimItems[i].Subscriber2)//if patient is not subscriber
				{
					//2000C Patient HL loop
					seg++;
					sw.WriteLine("HL*"+HLcount.ToString()+"*"//HL01:Heirarchical ID
						+parentSubsc.ToString()+"*"//HL02: parent is always the subscriber HL
						+"23*"//HL03: 23=Dependent
						+"0~");//HL04: never a subordinate
					//2000C PAT
					seg++;
					if(isMedical){
						sw.WriteLine("PAT*"
							+GetRelat(claim.PatRelat)+"~");//PAT01: Relat
							//PAT04 not used, so no further lines needed.
					}
					else{
						sw.WriteLine("PAT*"
							+GetRelat(claim.PatRelat)+"*"//PAT01: Relat
							+"**"//PAT02 & 03 not used
							+GetStudent(patient.StudentStatus)+"~");//PAT04: Student status code: N,P,or F
					}
					//2010CA NM1: Patient Name
					seg++;
					sw.Write("NM1*QC*"//NM101: QC=Patient
						+"1*"//NM102: 1=Person
						+Sout(patient.LName,35)+"*"//NM103: Lname
						+Sout(patient.FName,25));//NM104: Fname
					string patID=patient.SSN;
					List <PatPlan> patPlans=PatPlans.Refresh(patient.PatNum);
					for(int p=0;p<patPlans.Count;p++){
						if(patPlans[p].PlanNum==claim.PlanNum){
							patID=patPlans[p].PatID.Replace("-","");
						}
					}
					if(isMedical){
						if(patient.MiddleI==""){
							sw.WriteLine("~");
						}
						else{
							sw.WriteLine("*"+Sout(patient.MiddleI,25)+"~");//NM105: Mid name
						}
						//NM106: prefix not used. NM107: No suffix field in Open Dental
						//NM108-NM112 no longer allowed to be used.
						//instead of including a patID here, the patient should get their own subsriber loop.
					}
					else{
						if(patID==""){
							if(patient.MiddleI==""){
								sw.WriteLine("~");
							}
							else{
								sw.WriteLine("*"+Sout(patient.MiddleI,25)+"~");//NM105: Mid name
							}
						}
						else{//id not blank
							sw.WriteLine("*"+Sout(patient.MiddleI,25)+"*"//NM105: Mid name (whether or not empty)
							+"**"//NM106: prefix not used. NM107: No suffix field in Open Dental
							+"MI*"//NM108: MI=Member ID
							+Sout(patID,80)+"~");//NM109: Patient ID
						}
					}
					//2010CA N3: Patient address
					seg++;
					sw.Write("N3*"
						+Sout(patient.Address,55));//N301: address
						if(patient.Address2==""){
								sw.WriteLine("~");
							}
							else{
								//N302: Address2. Optional.
								sw.WriteLine("*"+Sout(patient.Address2,55)+"~");
							}
					//2010CA N4: City State Zip
					seg++;
					sw.WriteLine("N4*"
						+Sout(patient.City,30)+"*"//N401: City
						+Sout(patient.State,2)+"*"//N402: State
						+Sout(patient.Zip.Replace("-",""),15)+"~");//N403: Zip
					//2010CA DMG: Patient demographic
					seg++;
					sw.WriteLine("DMG*D8*"//DMG01: use D8
						+patient.Birthdate.ToString("yyyyMMdd")+"*"//DMG02: Birthdate
						+GetGender(patient.Gender)+"~");//DMG03: gender
					//2010CA REF: Property and casualty claim number.
					//2010CA PER: Property and casualty patient contact info
					HLcount++;
				}
				#endregion
				#region Claim
				//2300 CLM: Claim
				seg++;
				sw.Write("CLM*"
					+Sout(claim.PatNum.ToString()+"/"+claim.ClaimNum.ToString(),20)+"*"//CLM01: A unique id. Can support 20 char.  By using both PatNum and ClaimNum, it is possible to search for a patient as well as to ensure uniqueness. We might need to allow user to override for claims based on preauths.
					+claim.ClaimFee.ToString()+"*"//CLM02: Claim Fee
					+"**"//CLM03 & 04 not used
					+GetPlaceService(claim.PlaceService)+"::1*"//CLM05: place+1. 1=Original claim
					+"Y*"//CLM06: prov sig on file (always yes)
					+"A*");//CLM07: prov accepts medicaid assignment. OD has no field for this, so no choice
				if(insPlan.AssignBen){
					sw.Write("Y*");//CLM08: assign ben. Y or N
				}
				else{
					sw.Write("N*");
				}
				//if(insPlan.ReleaseInfo){
					sw.Write("Y");//CLM09: release info. Y or I(which we don't support)
				//}
				//else{
				//	sw.Write("N");//this is not allowed and is now blocked way ahead of time.
				//}
				if(!isMedical && claim.ClaimType=="PreAuth"){
					sw.WriteLine("**"//* for CLM09. CLM10 not used
						+GetRelatedCauses(claim)+"*"//CLM11: Accident related, including state. Might be blank.
						+"*"//CLM12: special programs like EPSTD
						+"******"//CLM13-18 not used
						+"PB~");//CLM19 PB=Predetermination of Benefits. Not allowed in medical claims. What is the replacement??
				}
				else{
					if(GetRelatedCauses(claim)==""){
						sw.WriteLine("~");
					}
					else{
						sw.WriteLine("**"//* for CLM09. CLM10 not used
							+GetRelatedCauses(claim)+"~");//CLM11: Accident related, including state
					}
				}
						//CLM20: delay reason code
				//2300 DTP: Date of onset of current illness (medical)
				//2300 DTP: Initial treatment date (spinal manipulation) (medical)
				//2300 DTP: Date last seen (foot care) (medical)
				//2300 DTP: Date accute manifestation (spinal manipulation) (medical)
				//2300 DTP: Date referral
				//2300 DTP: Date accident
				if(claim.AccidentDate.Year>1880){
					seg++;
					sw.WriteLine("DTP*439*"//DTP01: 439=accident
						+"D8*"//DTP02: use D8
						+claim.AccidentDate.ToString("yyyyMMdd")+"~");
				}
				//2300 DTP: (a bunch more useless medical dates)
				if(!isMedical){
					//2300 DTP: Date ortho appliance placed
					if(claim.OrthoDate.Year>1880){
						seg++;
						sw.WriteLine("DTP*452*"//DTP01: 452=Appliance placement
							+"D8*"//DTP02: use D8
							+claim.OrthoDate.ToString("yyyyMMdd")+"~");
					}
					//2300 DTP: Date of service for claim. Not used if predeterm
					if(claim.ClaimType!="PreAuth"){
						if(claim.DateService.Year>1880){
							seg++;
							sw.WriteLine("DTP*472*"//DTP01: 472=Service
								+"D8*"//DTP02: use D8
								+claim.DateService.ToString("yyyyMMdd")+"~");
						}
					}
					//2300 DN1: Months of ortho service
					if(claim.IsOrtho){
						seg++;
						sw.WriteLine("DN1*"
							+"*"//DN101 not used because no field yet in OD.
							+claim.OrthoRemainM.ToString()+"~");
					}
					//2300 DN2: Missing teeth
					ArrayList missingTeeth=ToothInitials.GetMissingOrHiddenTeeth(initialList);
					bool doSkip;
					for(int j=0;j<missingTeeth.Count;j++){
						//if the missing tooth is missing because of an extraction being billed here, then exclude it
						//still needed, even though missing teeth are not based on procedures any longer
						doSkip=false;
						for(int p=0;p<claimProcs.Count;p++){
							proc=Procedures.GetProcFromList(procList,claimProcs[p].ProcNum);
							procCode=ProcedureCodes.GetProcCode(proc.CodeNum);
							if(procCode.PaintType==ToothPaintingType.Extraction && proc.ToothNum==(string)missingTeeth[j]){
								doSkip=true;
								break;
							}
						}
						if(doSkip){
							continue;
						}
						seg++;
						sw.WriteLine("DN2*"
							+missingTeeth[j]+"*"//DN201: tooth number
							+"M~");//DN202: M=Missing, I=Impacted, E=To be extracted
					}
				}
				//2300 PWK: Paperwork. Used to identify attachments.
				/*if(clearhouse.ISA08=="113504607" && claim.Attachments.Count>0) {//If Tesia and has attachments
					seg++;
					sw.WriteLine("PWK*"
						+"OZ*"//PWK01: ReportTypeCode. OZ=Support data for claim.
						+"EL*"//PWK02: Report Transmission Code. EL=Electronic
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+"TES"+claim.ClaimNum.ToString()+"~");//PWK06: Identification Code.
				}*/
				//No validation is done.  However, warnings are displayed if:
				//Warning if attachments are listed as Mail even though we are sending electronically.
				//Warning if any PWK segments are needed, and there is no ID code.
				//PWK can repeat max 10 times.
				string pwk02;
				string idCode=claim.AttachmentID;
				if(idCode==""){//must be min of two char, so we need to make one up.
					idCode="  ";
				}
				idCode=Sout(idCode,80,2);
				if(claim.AttachedFlags.Contains("Mail")){
					pwk02="BM";//By Mail
				}
				else{
					pwk02="EL";//Elect
				}
				if(claim.AttachedFlags.Contains("EoB")){
					seg++;
					sw.WriteLine("PWK*"
						+"EB*"//PWK01: ReportTypeCode. EB=EoB.
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				if(claim.AttachedFlags.Contains("Note")){
					seg++;
					sw.WriteLine("PWK*"
						+"OB*"//PWK01: ReportTypeCode. OB=Operative Note.
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				if(claim.AttachedFlags.Contains("Perio")){
					seg++;
					sw.WriteLine("PWK*"
						+"P6*"//PWK01: ReportTypeCode. P6=Perio
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				if(claim.AttachedFlags.Contains("Misc") || claim.AttachedImages>0){
					seg++;
					sw.WriteLine("PWK*"
						+"OZ*"//PWK01: ReportTypeCode. OZ=Support Data For Claim.
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				if(claim.Radiographs>0){
					seg++;
					sw.WriteLine("PWK*"
						+"RB*"//PWK01: ReportTypeCode. RB=Radiology Films.
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				if(claim.AttachedModels>0){
					seg++;
					sw.WriteLine("PWK*"
						+"DA*"//PWK01: ReportTypeCode. DA=Dental Models.
						+pwk02+"*"//PWK02: Report Transmission Code. EL=Electronic, BM=By Mail
						+"**"//PWK03 and 04: not used
						+"AC*"//PWK05: Identification Code Qualifier. AC=Attachment Control Number
						+idCode+"~");//PWK06: Identification Code.
				}
				//2300 CN1: Contract Info (medical)
				//2300 AMT: Patient amount paid
				//2300 AMT: Total Purchased Service Amt (medical)
				//2300 REF: (A bunch of ref segments for medical which we don't need)
				//2300 REF: Predetermination ID
				if(claim.PreAuthString!=""){
					seg++;
					//note for REF01: G1 replaced G3 in 837_A specs, including 837pro.
					sw.WriteLine("REF*G1*"//REF01: G1=prior auth number
						+Sout(claim.PreAuthString,30)+"~");//REF02: Prior Auth Identifier
				}
				//2300 REF: Referral number
				if(claim.RefNumString!=""){
					seg++;
					sw.WriteLine("REF*9F*"//REF01: 9F=Referral number. Ok for medical, too.
						+Sout(claim.RefNumString,30)+"~");
				}
				//2300 K3: File info (medical). Not used.
				//2300 NTE: Note
				string note="";
				if(claim.AttachmentID!="" && !claim.ClaimNote.StartsWith(claim.AttachmentID)){
					note=claim.AttachmentID+" ";
				}
				note+=claim.ClaimNote;
				if(note!=""){
					seg++;
					sw.WriteLine("NTE*ADD*"//NTE01: ADD=Additional infor
						+Sout(note,80)+"~");
				}
				//2300 CR1: (medical)Ambulance transport info
				//2300 CR2: (medical) Spinal Manipulation Service Info
				//2300 CRC: (medical) About 3 irrelevant segments
				ArrayList diagnoses=new ArrayList();//princDiag will always be the first element.
				if(isMedical){
					for(int j=0;j<claimProcs.Count;j++){
						proc=Procedures.GetProcFromList(procList,claimProcs[j].ProcNum);
						if(proc.DiagnosticCode==""){
							continue;
						}
						if(proc.IsPrincDiag){
							if(diagnoses.Contains(proc.DiagnosticCode)){
								diagnoses.Remove(proc.DiagnosticCode);
							}
							diagnoses.Insert(0,proc.DiagnosticCode);//princDiag always goes first. There will always be one.
						}
						else{//not princDiag
							if(!diagnoses.Contains(proc.DiagnosticCode)){
								diagnoses.Add(proc.DiagnosticCode);
							}
						}
					}
					//2300 HI: (medical) Health Care Diagnosis Code. Required
					seg++;
					sw.Write("HI*"
						+"BK:"//HI01-1: BK=ICD-9 Principal Diagnosis
						+Sout((string)diagnoses[0],30).Replace(".",""));//HI01-2: Diagnosis code. No periods.
					for(int j=1;j<diagnoses.Count;j++){
						if(j>11){//maximum of 12 diagnoses
							continue;
						}
						sw.Write("*"//this is the * from the _previous_ field.
							+"BF:"//HI0#-1: BF=ICD-9 Diagnosis
							+Sout((string)diagnoses[j],30).Replace(".",""));//HI0#-2: Diagnosis code. No periods.
					}
					sw.WriteLine("~");
					//2300 HI: (medical) Anesthesia related procedure
					//2300 HI: (medical) Condition information
				}
				//2300 HCP: (medical) (not used) Claim Pricing/Repricing Info
				//2310A Referring provider. We don't use.
				//2310B Rendering provider. Only required if different from the billing provider
				//But required by WebClaim, so we will always include it
				provTreat=ProviderC.ListLong[Providers.GetIndexLong(claim.ProvTreat)];
				//if(claim.ProvTreat!=claim.ProvBill){
				//2310B NM1: name
				seg++;
				sw.Write("NM1*82*"//82=rendering prov
					+"1*"//NM102: 1=person
					+Sout(provTreat.LName,35)+"*"//NM103: LName
					+Sout(provTreat.FName,25)+"*"//NM104: FName
					+Sout(provTreat.MI,25)+"*"//NM105: MiddleName
					+"*"//NM106: not used
					+"*");//NM107: suffix. We don't support
				//It's after the NPI date, now, so always send NPI here:
				sw.Write("XX*");//NM108: ID code qualifier. 24=EIN. 34=SSN, XX=NPI
				sw.WriteLine(Sout(provTreat.NationalProvID,80)+"~");//NM109: ID code.  NPI validated.
				//2310B PRV: Rendering provider information
				if(isMedical){
					seg++;
					sw.WriteLine("PRV*"
						+"PE*"//PRV01: PE=Performing
						+"PXC*"//PRV02: PXC=Health Care Provider Taxonomy Code
						+X12Generator.GetTaxonomy(provTreat)+"~");//PRV03: Taxonomy code
				}
				else{//dental
					seg++;
					sw.WriteLine("PRV*"
						+"PE*"//PRV01: PE=Performing
						+"ZZ*"//PRV02: ZZ=mutually defined taxonomy code
						+X12Generator.GetTaxonomy(provTreat)+"~");//PRV03: Taxonomy code
				}
				//2310B REF: Rendering provider secondary ID
					//All of these will be eliminated when NPI is mandated.
					//isMedical, only allowed types are 0B,1G,G2,and LU.
				//2310B REF: Rendering provider secondary ID.
				seg++;
				sw.WriteLine("REF*0B*"//REF01: 0B=state license #
					+Sout(provTreat.StateLicense,30)+"~");
				if(!isMedical){//we can't support these numbers very well yet for medical
					//2310B REF: Rendering Provider Secondary ID number(s). Only required by some carriers.
					seg+=WriteProv_REF(sw,provTreat,claimItems[i].PayorId0);
				}
				//2310C (medical)Purchased Service provider secondary ID. We don't support this for medical
				//2310C (not medical)NM1: Service facility location.  Only required if PlaceService is 21,22,31, or 35. 35 does not exist in CPT, so we assume 33
				//if(!isMedical && 
				if(claim.PlaceService==PlaceOfService.InpatHospital || claim.PlaceService==PlaceOfService.OutpatHospital
					|| claim.PlaceService==PlaceOfService.SkilledNursFac || claim.PlaceService==PlaceOfService.CustodialCareFacility) {//AdultLivCareFac
					seg++;
					sw.WriteLine("NM1*FA*"//FA=Facility
						+"2*"//NM102: 2=non-person
						+Sout(billProv.LName,35)+"*"//NM103:Facility Name
						+"*"//NM104: not used
						+"*"//NM105: not used
						+"*"//NM106: not used
						+"*"//NM107: not used
						+"XX*"//NM108: XX=NPI
						+Sout(billProv.NationalProvID,80)+"~");//NM109: NPI. Validated.
				}
				//or 2310D (medical)NM1: Service facility location. Required if different from 2010AA. Not supported.
				//2310D (medical)N3,N4,REF,PER: not supported.
				//2310E (medical)NM1,REF Supervising Provider. Not supported.
				//2310F (medical)NM1,N3,N4 Ambulance Pickup location. Not supported.
				//2310G (medical)NM1,N3,N4 Ambulance Dropoff location. Not supported.
				//2320 Other subscriber
				if(claim.PlanNum2>0){
					seg++;
					sw.Write("SBR*");
					if(claim.ClaimType=="S"){
						sw.Write("P*");//SBR01: Payer responsibility code
					}
					else if(claim.ClaimType=="P"){
						sw.Write("S*");
					}
					else{
						sw.Write("T*");//T=Tertiary
					}
					sw.Write(GetRelat(claim.PatRelat2)+"*");//SBR02: Individual Relationship Code
					sw.Write(Sout(otherPlan.GroupNum,30)+"*"//SBR03: Group Number
						+Sout(otherPlan.GroupName,60)+"*"//SBR04: Group Name
						+"*"//SBR05: Dental: Not used, Medical: not supported
						+"*");//SBR06: Not used
					sw.Write("**");//SBR07 & 08 not used.
					sw.WriteLine("CI~");//SBR09: 12=PPO,17=DMO,BL=BCBS,CI=CommercialIns,FI=FEP,HM=HMO
							//,MC=Medicaid,SA=self-administered, etc. There are too many. I'm just going 
							//to use CI for everyone. I don't think anyone will care.
							//After mandated National Plan ID, then not used anymore.
				//2320 CAS: Claim Level Adjustments. Not supported.
				//2320 AMT: COB Payer paid amount
					if(claim.ClaimType!="P"){
						double paidOtherIns=0;
						for(int j=0;j<claimProcs.Count;j++) {
							paidOtherIns+=ClaimProcs.ProcInsPayPri(claimProcList,claimProcs[j].ProcNum,claimProcs[j].PlanNum);
						}
						seg++;
						sw.WriteLine("AMT*D*"//AMT01: D=Payor amount paid
							+paidOtherIns.ToString("F")+"~");//AMT02: Amount
					}
				//2320 AMT: (medical) COB Total Non-Covered Amt (A8)
				//2320 AMT: (medical) Remaining Patient liability (EAF)
				//2320 AMT: COB Approved Amt (AAE)
				//2320 AMT: COB Allowed Amt (B6)
				//2320 AMT: COB Patient Responsibility (F2)
				//2320 AMT: COB Coverage Amt (AU)
				//2320 AMT: COB Discount Amt (D8)
				//2320 AMT: COB Patient Paid Amt (F5)
				//2320 DMG: Other subscriber demographics
					seg++;
					sw.WriteLine("DMG*D8*"//DMG01: use D8
						+otherSubsc.Birthdate.ToString("yyyyMMdd")+"*"//DMG02: Birthdate
						+GetGender(otherSubsc.Gender)+"~");//DMG03: gender
				//2320 OI: Other ins info
					seg++;
					sw.Write("OI***");//OI01 & 02 not used
					if(otherPlan.AssignBen){
						sw.Write("Y***");//OI03: assign ben. Y or N. OI04(medical): we don't support
					}
					else{
						sw.Write("N***");
					}
					//if(otherPlan.ReleaseInfo){
						sw.WriteLine("Y~");//OI06: release info. Y or I(which we don't support)
					//}
					//else{
					//	sw.WriteLine("N~");
					//}
					//2320 MOA: (medical) We don't support
					//2330A NM1: Other subscriber name
					seg++;
					sw.WriteLine("NM1*IL*"//NM010: IL=insured
						+"1*"//NM102: 1=person
						+Sout(otherSubsc.LName,35)+"*"//NM103: LName
						+Sout(otherSubsc.FName,25)+"*"//NM104: FName
						+Sout(otherSubsc.MiddleI,25)+"*"//NM105: MiddleName
						+"*"//NM106: not used
						+"*"//NM107: suffix. No corresponding field in OD
						+"MI*"//NM108: MI=Member ID
						+Sout(otherPlan.SubscriberID,80)+"~");//NM109: ID
					//2330A N3: Address
					seg++;
					sw.Write("N3*"
						+Sout(otherSubsc.Address,55));//N301: address
						if(otherSubsc.Address2==""){
							sw.WriteLine("~");
						}
						else{
							sw.WriteLine("*"+Sout(otherSubsc.Address2,55)+"~");//N302: address2
						}
					//2330A N4: City State Zip
					seg++;
					sw.WriteLine("N4*"
						+Sout(otherSubsc.City,30)+"*"//N401: City
						+Sout(otherSubsc.State,2)+"*"//N402: State
						+Sout(otherSubsc.Zip,15)+"~");//N403: Zip
					//2330A REF: Other subscriber secondary ID. Not supported.
					//2330B NM1: Other payer name
					seg++;
					sw.Write("NM1*PR*"//NM101: PR=Payer
						+"2*");//NM102: 2=non-person
					if(clearhouse.ISA08=="EMS") {
						//This is a special situation requested by EMS.  This tacks the employer onto the end of the carrier.
						sw.Write(Sout(otherCarrier.CarrierName,17)+"|"+Sout(Employers.GetName(otherPlan.EmployerNum),17)+"*");
					}
					else {
						sw.Write(Sout(otherCarrier.CarrierName,35)+"*");//NM103: Name. Length can be 60 in the new medical specs.
					}
					sw.Write(
						 "*"//NM104:
						+"*"//NM105:
						+"*"//NM106: not used
						+"*"//NM107: not used
						+"PI*");//NM108: PI=Payor ID. XV must be used after national plan ID mandated.
					if(otherCarrier.ElectID.Length<3) {
						sw.WriteLine("06126~");//NM109: PayorID
					}
					else {
						sw.WriteLine(Sout(otherCarrier.ElectID,80,2)+"~");//NM109: PayorID
					}
					//2230B N3,N4: (medical) Other payer address. We don't support.
					//2330B PER: Other payer contact info. Not needed.
					//2330B DTP: Claim Paid date
					if(claim.ClaimType!="P") {
						DateTime datePaidOtherIns=DateTime.Today;
						DateTime dtThisCP;
						for(int j=0;j<claimProcs.Count;j++) {
							dtThisCP=ClaimProcs.GetDatePaid(claimProcList,claimProcs[j].ProcNum,claimProcs[j].PlanNum);
							if(dtThisCP>datePaidOtherIns){
								datePaidOtherIns=dtThisCP;
							}
						}
						//it's a required segment, so always include it.
						seg++;
						sw.WriteLine("DTP*573*"//DTP01: 573=Date claim paid
							+"D8*"//DTP02: date time format qualifier
							+datePaidOtherIns.ToString("yyyyMMdd")+"~");//DTP03 date
					}
					//2330B DTP: (medical) Claim adjudication date. We might need to add this
					//2330B REF: Other payer secondary ID
					//2330B REF: Other payer referral number
					//2330B REF: Other payer claim adjustment indicator
					//2330C: Other payer patient info. Only used in COB.
					//2330D: Other payer referring provider. We don't support
					//2330E,F,G,H,I: (medical) not supported
				}//if(claim.PlanNum2>0){
				#endregion
				#region Line Items
				//2400 Service Lines
				for(int j=0;j<claimProcs.Count;j++){
					proc=Procedures.GetProcFromList(procList,claimProcs[j].ProcNum);
					procCode=ProcedureCodes.GetProcCode(proc.CodeNum);
					//2400 LX: Line Counter. or (medical) Service Line Number
					seg++;
					sw.WriteLine("LX*"+(j+1).ToString()+"~");
					if(isMedical){
						//2400 SV1: Professional Service
						seg++;
						sw.Write("SV1*"
							//SV101 Composite Medical Procedure Identifier
							+"HC:"//SV101-1: HC=Health Care
							+Sout(claimProcs[j].CodeSent)+"*"//SV101-2: Procedure code. The rest of SV101 is not supported
							+claimProcs[j].FeeBilled.ToString()+"*"//SV102: Charge Amt
							+"MJ*"//SV103: MJ=minutes
							+"0*");//SV104: Quantity of anesthesia. We don't support, so always 0.
						if(proc.PlaceService==claim.PlaceService){
							sw.Write("*");//SV105: Place of Service Code if different from claim
						}
						else{
							sw.Write(GetPlaceService(proc.PlaceService)+"*");
						}
						sw.Write("*");//SV106: not used
						//SV107: Composite Diagnosis Code Pointer. Required when 2300HI(Health Care Diagnosis Code) is used (always).
						//SV107-1: Primary diagnosis. Only allowed pointers 1-8 even though 2300HI supports 12 diagnoses.
						//We don't validate that there are not more than 8 diagnoses on one claim.
						//If the diagnosis we need is not in the first 8, then we will use the primary.
						if(proc.DiagnosticCode==""){//If the diagnosis is blank, we will use the primary.
							sw.Write("1");//use primary.
						}
						else{
							int diagI=1;
							for(int d=0;d<diagnoses.Count;d++){
								if(d>7){//we can't point to any except first 8.
									continue;
								}
								if((string)diagnoses[d]==proc.DiagnosticCode){
									diagI=d+1;
								}
							}
							sw.Write(diagI.ToString());
						}
						//SV107-2 through 4: Other diagnoses, which we don't support yet.
						sw.WriteLine("~");
						//sw.Write("*");//SV108: not used
						//sw.Write("*");//SV109: Emergency indicator. Required if emergency. Y or blank. Not supported.
						//sw.Write("*");//SV110: not used
						//sw.Write("*");//SV111: EPSTD indicator (Medicaid from screening). Y or blank. Not supported.
						//sw.Write("*");//SV112: Family planning indicator for Medicaid. Y or blank. Not supported.
						//sw.Write("**");//SV113 and SV114: not used
						//SV115: Copay status code: 0 or blank. Not supported
						//2400 SV5,PWK,CR1,CR2,CR3,CR5,CRC(x4): (medical)Unsupported
					}//if isMedical
					else{
						//2400 SV3: Dental Service
						seg++;
						sw.Write("SV3*"
							+"AD:"+Sout(claimProcs[j].CodeSent,5)+"*"//SV301-1: AD=ADA CDT, SV301-2:Procedure code
							+claimProcs[j].FeeBilled.ToString()+"*");//SV302: Charge Amount
						if(proc.PlaceService==claim.PlaceService){
							sw.Write("*");//SV303: Location Code if different from claim
						}
						else{
							sw.Write(GetPlaceService(proc.PlaceService)+"*");
						}
						sw.WriteLine(GetArea(proc,procCode)+"*"//SV304: Area
							+proc.Prosthesis+"*"//SV305: Initial or Replacement
							+"1~");//SV306: Procedure count
						//2400 TOO: Tooth number/surface
						if(procCode.TreatArea==TreatmentArea.Tooth){
							seg++;
							sw.WriteLine("TOO*JP*"//TOO01: JP=National tooth numbering
								+proc.ToothNum+"~");//TOO02: Tooth number
						}
						else if(procCode.TreatArea==TreatmentArea.Surf){
							seg++;
							sw.Write("TOO*JP*"//TOO01: JP=National tooth numbering
								+proc.ToothNum+"*");//TOO02: Tooth number
							string validSurfaces=Tooth.SurfTidyForClaims(proc.Surf,proc.ToothNum);
							for(int k=0;k<validSurfaces.Length;k++){
								if(k>0){
									sw.Write(":");
								}
								sw.Write(validSurfaces.Substring(k,1));//TOO03: Surfaces
							}
							sw.WriteLine("~");
						}
						else if(procCode.TreatArea==TreatmentArea.ToothRange) {
							string[] individTeeth=proc.ToothRange.Split(',');
							for(int t=0;t<individTeeth.Length;t++) {
								seg++;
								sw.WriteLine("TOO*JP*"//TOO01: JP=National tooth numbering
									+individTeeth[t]+"~");//TOO02: Tooth number
							}
						}
					}
					//2400 DTP: Date of service if different from claim, but we will always show the date. Better compatibility.
					//Required if medical anyway.
					//if(claim.DateService!=proc.ProcDate){
					if(claim.ClaimType!="PreAuth"){
						seg++;
						sw.WriteLine("DTP*472*"//DTP01: 472=Service
							+"D8*"//DTP02: use D8
							+proc.ProcDate.ToString("yyyyMMdd")+"~");
					}
					//}
					//2400 DTP: Date prior placement
					if(proc.Prosthesis=="R"){//already validated date
						seg++;
						sw.WriteLine("DTP*441*"//DTP01: 441=Prior placement
							+"D8*"//DTP02: use D8
							+proc.DateOriginalProsth.ToString("yyyyMMdd")+"~");
					}
					//2400 DTP: Date ortho appliance placed. Not used.
					//2400 DTP: Date ortho appliance replaced.  Not used.
					//2400 DTP: (medical) Rx date. Not supported.
					//2400 DTP: (medical) Date Certification Revision. Not supported.
					//2400 DTP: (medical) Date begin therapy. Not supported.
					//2400 DTP: (medical) Date last certification. Not supported.
					//2400 DTP: (medical) Date last seen. Not supported.
					//2400 DTP: (medical) Dialysis test dates. Not supported.
					//2400 DTP: (medical) Date blood gas test. Not supported.
					//2400 DTP: (medical) Date shipped. Not supported.
					//2400 DTP: (medical) Date last x-ray. Not supported.
					//2400 DTP: (medical) Date initial tx. Not supported.
					//2400 QTY: (medical) Ambulance patient count. Not supported.
					//2400 QTY: Anesthesia quantity. Not used.
					//2400 MEA,CN1: (medical) . Not supported.
					//2400 REF: (medical) A variety of medical REFs not supported.
					//2400 REF: Pretermination ID. Not used.
					//2400 REF: Referral #. Not used.
					//2400 REF: Line item control number(Proc Num)
					seg++;
					sw.WriteLine("REF*6R*"//REF01: 6R=Procedure control number
						+proc.ProcNum.ToString()+"~");
					//2400 AMT(x4): (medical) Various amounts. Not supported
					//2400 K3: (medical) Not supported.
					//2400 NTE: Line note
					if(proc.ClaimNote!=""){
						seg++;
						sw.WriteLine("NTE*ADD*"//NTE01: ADD=Additional info
							+Sout(proc.ClaimNote,80)+"~");
					}
					//2400 NTE,PS1,HCP: (medical) Not supported
					//2410 LIN,CTP,REF: (medical) Not supported
					//2420A NM1: Rendering provider name. Only if different from the claim.
					if(claim.ProvTreat!=proc.ProvNum
						&& PrefC.GetBool(PrefName.EclaimsSeparateTreatProv))
					{
						provTreat=ProviderC.ListLong[Providers.GetIndexLong(proc.ProvNum)];
						seg++;
						sw.Write("NM1*82*"//82=rendering prov
							+"1*"//NM102: 1=person
							+Sout(provTreat.LName,35)+"*"//NM103: LName
							+Sout(provTreat.FName,25)+"*"//NM104: FName
							+Sout(provTreat.MI,25)+"*"//NM105: MiddleName
							+"*"//NM106: not used.
							+"*");//NM107: suffix. not supported.
						//After NPI date, so always do it one way:
						sw.Write("XX*");//NM108: XX=NPI
						sw.Write(Sout(provTreat.NationalProvID,80));//NM109: ID.  NPI validated.
						sw.WriteLine("~");
						//2420A PRV: Rendering provider information
						seg++;
						sw.Write("PRV*");
						sw.Write("PE*");//PRV01: PE=Performing
						if(isMedical){
							sw.Write("PXC*");//PRV02: PXC=health care provider taxonomy code
						}
						else{
							sw.Write("ZZ*");//PRV02: ZZ=mutually defined taxonomy code
						}
						sw.WriteLine(X12Generator.GetTaxonomy(provTreat)+"~");//PRV03: Taxonomy code
						//2420A REF: Rendering provider secondary ID. 
						//2420A REF: (medical)Required before NPI date. We already enforce NPI in NM109.  Less allowed values.
						seg++;
						sw.WriteLine("REF*0B*"//REF01: 0B=state license #
							+Sout(provTreat.StateLicense,30)+"~");
						//2420A REF: Rendering provider secondary ID
						//if(!isMedical){//we can't support these numbers very well yet for medical
						//	seg+=WriteProv_REF(sw,provTreat,(string)claimAr[0,i]);
						//}
						//2420B,C,D,E,F,G,H,I,2430,2440: (medical) not supported
					}
				}//for int i claimProcs
				#endregion
				if(i==claimItems.Count-1//if this is the last loop
					|| claimItems[i].PayorId0 != claimItems[i+1].PayorId0)//or the payorID will change
				{
					hasFooter=true;
				}
				else{
					hasFooter=false;
				}
				if(hasFooter){
					//Transaction Trailer
					seg++;
					sw.WriteLine("SE*"
						+seg.ToString()+"*"//SE01: Total segments, including ST & SE
						+transactionNum.ToString().PadLeft(4,'0')+"~");
					if(i<claimItems.Count-1){//if this is not the last loop
						transactionNum++;
					}
					//sw.WriteLine();
				}
			}//for claimAr i
			//Functional Group Trailer
			sw.WriteLine("GE*"+transactionNum.ToString()+"*"//GE01: Number of transaction sets included
				+groupControlNumber+"~");//GE02: Group Control number. Must be identical to GS06
			
		}

		

		

		///<summary>Sometimes writes the name information for Open Dental. Sometimes it writes practice info.</summary>
		private static void Write1000A_NM1(StreamWriter sw,Clearinghouse clearhouse){
			if(clearhouse.SenderTIN==""){//use OD
				sw.WriteLine("NM1*41*"//NM101: 41=submitter
					+"2*"//NM102: 2=nonperson
					+"OPEN DENTAL SOFTWARE*"//NM103:Submitter Name
					+"****46*"//NM108: 46 indicates ETIN
					+"810624427~");//NM109: ID Code. aka ETIN#.
			}
			else{
				sw.WriteLine("NM1*41*"//NM101: 41=submitter
					+"2*"//NM102: 1=person,2=nonperson
					+Sout(clearhouse.SenderName,35,1)+"*"//NM103:Submitter Name. Validated.
					+"****46*"//NM108: 46 indicates ETIN
					+Sout(clearhouse.SenderTIN,80,2)+"~");//NM109: ID Code. aka ETIN#. Validated to be at least 2.
			}
		}

		///<summary>Usually writes the contact information for Open Dental. But for inmediata and AOS clearinghouses, it writes practice contact info.</summary>
		private static void Write1000A_PER(StreamWriter sw,Clearinghouse clearhouse){
			if(clearhouse.SenderTIN=="") {//use OD
				sw.WriteLine("PER*IC*"//PER01:Function code: IC=Information Contact
					+"JORDAN SPARKS*"//PER02:Name
					+"TE*"//PER03:Comm Number Qualifier: TE=Telephone
					+"8776861248~");//PER04:Comm Number. aka telephone number
			}
			else {
				sw.WriteLine("PER*IC*"//PER01:Function code: IC=Information Contact
					+Sout(clearhouse.SenderName,60,1)+"*"//PER02:Name. Validated.
					+"TE*"//PER03:Comm Number Qualifier: TE=Telephone
					+clearhouse.SenderTelephone+"~");//PER04:Comm Number. aka telephone number. Validated to be exactly 10 digits.
			}
		}

		///<summary>This is depedent only on the electronic payor id # rather than the clearinghouse.  Used for billing prov and also for treating prov. Returns the number of segments written</summary>
		private static int WriteProv_REF(StreamWriter sw,Provider prov,string payorID){
			int retVal=0;
			ElectID electID=ElectIDs.GetID(payorID);
			//if(electID==null){
			//	return;
			//}
			if(electID!=null && electID.IsMedicaid){
				retVal++;
				sw.WriteLine("REF*"
					+"1D*"//REF01: ID qualifier. 1D=Medicaid
					+Sout(prov.MedicaidID,30,1)+"~");//REF02. ID number
			}
			//I don't think there would be additional id's if Medicaid, but just in case, no return.
			ProviderIdent[] provIdents=ProviderIdents.GetForPayor(prov.ProvNum,payorID);
			for(int i=0;i<provIdents.Length;i++){
				retVal++;
				sw.WriteLine("REF*"
					//REF01: ID qualifier, a 2 letter code representing type
					+GetProvTypeQualifier(provIdents[i].SuppIDType)+"*"
					+provIdents[i].IDNumber+"~");//REF02. ID number
			}
			return retVal;
		}

		private static string GetProvTypeQualifier(ProviderSupplementalID provType){
			switch(provType){
				case ProviderSupplementalID.BlueCross:
					return "1A";
				case ProviderSupplementalID.BlueShield:
					return "1B";
				case ProviderSupplementalID.SiteNumber:
					return "G5";
				case ProviderSupplementalID.CommercialNumber:
					return "G2";
			}
			return "";
		}

		private static string GetGender(PatientGender patGender){
			switch(patGender){
				case PatientGender.Male:
					return "M";
				case PatientGender.Female:
					return "F";
				case PatientGender.Unknown:
					return "U";
			}
			return "";
		}

		private static string GetRelat(Relat relat){
			switch(relat){
				case(Relat.Self):
					return "18";
				case(Relat.Child):
					return "19";
				case(Relat.Dependent):
					return "76";
				case(Relat.Employee):
					return "20";
				case(Relat.HandicapDep):
					return "22";
				case(Relat.InjuredPlaintiff):
					return "41";
				case(Relat.LifePartner):
					return "53";
				case(Relat.SignifOther):
					return "29";
				case(Relat.Spouse):
					return "01";
			}
			return "";
		}

		private static string GetStudent(string studentStatus){
			if(studentStatus=="P"){
				return "P";
			}
			if(studentStatus=="F"){
				return "F";
			}
			return "N";//either N or blank
		}

		private static string GetPlaceService(PlaceOfService place){
			switch(place){
				case PlaceOfService.AmbulatorySurgicalCenter:
					return "24";
				case PlaceOfService.CustodialCareFacility:
					return "33";
				case PlaceOfService.EmergencyRoomHospital:
					return "23";
				case PlaceOfService.FederalHealthCenter:
					return "50";
				case PlaceOfService.InpatHospital:
					return "21";
				case PlaceOfService.MilitaryTreatFac:
					return "26";
				case PlaceOfService.MobileUnit:
					return "15";
				case PlaceOfService.Office:
				case PlaceOfService.OtherLocation:
					return "11";
				case PlaceOfService.OutpatHospital:
					return "22";
				case PlaceOfService.PatientsHome:
					return "12";
				case PlaceOfService.PublicHealthClinic:
					return "71";
				case PlaceOfService.RuralHealthClinic:
					return "72";
				case PlaceOfService.School:
					return "03";
				case PlaceOfService.SkilledNursFac:
					return "31";
			}
			return "11";
		}

		private static string GetRelatedCauses(Claim claim){
			if(claim.AccidentRelated==""){
				return "";
			}
			//even though the specs let you submit all three types at once, we only allow one of the three
			if(claim.AccidentRelated=="A"){
				return "AA:::"+Sout(claim.AccidentST,2,2);
			}
			else if(claim.AccidentRelated=="E"){
				return "EM";
			}
			else{// if(claim.AccidentRelated=="O"){
				return "OA";
			}
		}

		///<summary>This used to be an enumeration.</summary>
		private static string GetFilingCode(InsPlan plan){
			string filingcode=InsFilingCodes.GetEclaimCode(plan.FilingCode);
			//must be one or two char in length.
			if(filingcode=="" || filingcode.Length>2){
				return "CI";
			}
			return Sout(filingcode,2,1);
			/*
			switch(plan.FilingCode){
				case InsFilingCodeOld.SelfPay:
					return "09";
				case InsFilingCodeOld.OtherNonFed:
					return "11";
				case InsFilingCodeOld.PPO:
					return "12";
				case InsFilingCodeOld.POS:
					return "13";
				case InsFilingCodeOld.EPO:
					return "14";
				case InsFilingCodeOld.Indemnity:
					return "15";
				case InsFilingCodeOld.HMO_MedicareRisk:
					return "16";
				case InsFilingCodeOld.DMO:
					return "17";
				case InsFilingCodeOld.BCBS:
					return "BL";
				case InsFilingCodeOld.Champus:
					return "CH";
				case InsFilingCodeOld.Commercial_Insurance:
					return "CI";
				case InsFilingCodeOld.Disability:
					return "DS";
				case InsFilingCodeOld.FEP:
					return "FI";
				case InsFilingCodeOld.HMO:
					return "HM";
				case InsFilingCodeOld.LiabilityMedical:
					return "LM";
				case InsFilingCodeOld.MedicarePartB:
					return "MB";
				case InsFilingCodeOld.Medicaid:
					return "MC";
				case InsFilingCodeOld.ManagedCare_NonHMO:
					return "MH";
				case InsFilingCodeOld.OtherFederalProgram:
					return "OF";
				case InsFilingCodeOld.SelfAdministered:
					return "SA";
				case InsFilingCodeOld.Veterans:
					return "VA";
				case InsFilingCodeOld.WorkersComp:
					return "WC";
				case InsFilingCodeOld.MutuallyDefined:
					return "ZZ";
				default:
					return "CI";
			}
			*/
		}

		private static string GetArea(Procedure proc,ProcedureCode procCode){
			if(procCode.TreatArea==TreatmentArea.Arch){
				if(proc.Surf=="U"){
					return "01";
				}
				if(proc.Surf=="L"){
					return "02";
				}
			}
			if(procCode.TreatArea==TreatmentArea.Mouth){
				return "";
			}
			if(procCode.TreatArea==TreatmentArea.Quad){
				if(proc.Surf=="UR"){
					return "10";
				}
				if(proc.Surf=="UL"){
					return "20";
				}
				if(proc.Surf=="LR"){
					return "40";
				}
				if(proc.Surf=="LL"){
					return "30";
				}
			}
			if(procCode.TreatArea==TreatmentArea.Sextant){
				//we will assume that these are very rarely billed to ins
				return "";
			}
			if(procCode.TreatArea==TreatmentArea.Surf){
				return "";//might need to enhance this
			}
			if(procCode.TreatArea==TreatmentArea.Tooth){
				return "";//might need to enhance this
			}
			if(procCode.TreatArea==TreatmentArea.ToothRange){
				//already checked for blank tooth range
				if(Tooth.IsMaxillary(proc.ToothRange.Split(',')[0])){
					return "01";
				}
				else{
					return "02";
				}
			}
			return "";
		}
		
		///<summary>Converts any string to an acceptable format for X12. Converts to all caps and strips off all invalid characters. Optionally shortens the string to the specified length and/or makes sure the string is long enough by padding with spaces.</summary>
		private static string Sout(string intputStr,int maxL,int minL){
			string retStr=intputStr.ToUpper();
			//Debug.Write(retStr+",");
			retStr=Regex.Replace(retStr,//replaces characters in this input string
				//Allowed: !"&'()+,-./;?=(space)#   # is actually part of extended character set
				"[^\\w!\"&'\\(\\)\\+,-\\./;\\?= #]",//[](any single char)^(that is not)\w(A-Z or 0-9) or one of the above chars.
				"");
			retStr=Regex.Replace(retStr,"[_]","");//replaces _
			retStr=retStr.Trim();//removes leading and trailing spaces.
			if(maxL!=-1){
				if(retStr.Length>maxL){
					retStr=retStr.Substring(0,maxL);
				}
			}
			if(minL!=-1){
				if(retStr.Length<minL){
					retStr=retStr.PadRight(minL,' ');
				}
			}
			//Debug.WriteLine(retStr);
			return retStr;
		}

		///<summary>Converts any string to an acceptable format for X12. Converts to all caps and strips off all invalid characters. Optionally shortens the string to the specified length and/or makes sure the string is long enough by padding with spaces.</summary>
		private static string Sout(string str,int maxL){
			return Sout(str,maxL,-1);
		}

		///<summary>Converts any string to an acceptable format for X12. Converts to all caps and strips off all invalid characters. Optionally shortens the string to the specified length and/or makes sure the string is long enough by padding with spaces.</summary>
		private static string Sout(string str){
			return Sout(str,-1,-1);
		}

		///<summary>Returns a string describing all missing data on this claim.  Claim will not be allowed to be sent electronically unless this string comes back empty.  There is also an out parameter containing any warnings.  Warnings will not block sending.</summary>
		public static string GetMissingData(ClaimSendQueueItem queueItem, out string warning){
			StringBuilder strb=new StringBuilder();
			warning="";
			Clearinghouse clearhouse=ClearinghouseL.GetClearinghouse(queueItem.ClearinghouseNum);
			Claim claim=Claims.GetClaim(queueItem.ClaimNum);
			Clinic clinic=Clinics.GetClinic(claim.ClinicNum);
			//if(clearhouse.Eformat==ElectronicClaimFormat.X12){//not needed since this is always true
			X12Validate.ISA(clearhouse,strb);
			if(clearhouse.GS03.Length<2) {
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Clearinghouse GS03");
			}
			List<X12TransactionItem> claimItems=Claims.GetX12TransactionInfo(((ClaimSendQueueItem)queueItem).ClaimNum);//just to get prov. Needs work.
			Provider billProv=ProviderC.ListLong[Providers.GetIndexLong(claimItems[0].ProvBill1)];
			Provider treatProv=ProviderC.ListLong[Providers.GetIndexLong(claim.ProvTreat)];
			InsPlan insPlan=InsPlans.GetPlan(claim.PlanNum,new List <InsPlan> ());
			if(insPlan.IsMedical && !PrefC.GetBool(PrefName.MedicalEclaimsEnabled)) {
				return "Medical e-claims not allowed";
			}
			//billProv
			X12Validate.BillProv(billProv,strb);
			//treatProv
			if(treatProv.LName==""){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Treating Prov LName");
			}
			if(treatProv.FName==""){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Treating Prov FName");
			}
			if(treatProv.SSN.Length<2){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Treating Prov SSN");
			}
			if(treatProv.NationalProvID.Length<2) {
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Treating Prov NPI");
			}
			if(treatProv.StateLicense==""){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Treating Prov Lic #");
			}
			if(insPlan.IsMedical){
				if(treatProv.NationalProvID.Length<2){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Treating Prov NPI");
				}
			}
			if(PrefC.GetString(PrefName.PracticeTitle)=="") {
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Practice Title");
			}
			if(clinic==null){
				X12Validate.PracticeAddress(strb);
			}
			else{
				X12Validate.Clinic(clinic,strb);
			}
			if(!insPlan.ReleaseInfo){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("InsPlan Release of Info");
			}
			Carrier carrier=Carriers.GetCarrier(insPlan.CarrierNum);
			X12Validate.Carrier(carrier,strb);
			ElectID electID=ElectIDs.GetID(carrier.ElectID);
			if(electID!=null && electID.IsMedicaid && billProv.MedicaidID==""){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("BillProv Medicaid ID");
			}
			if(claim.PlanNum2>0){
				InsPlan insPlan2=InsPlans.GetPlan(claim.PlanNum2,new List <InsPlan> ());
				Carrier carrier2=Carriers.GetCarrier(insPlan2.CarrierNum);
				if(carrier2.Address==""){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Secondary Carrier Address");
				}
				if(carrier2.City.Length<2){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Secondary Carrier City");
				}
				if(carrier2.State.Length!=2){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Secondary Carrier State(2 char)");
				}
				if(carrier2.Zip.Length<3){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Secondary Carrier Zip");
				}
				if(claim.PatNum != insPlan2.Subscriber//if patient is not subscriber
					&& claim.PatRelat2==Relat.Self) {//and relat is self
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append("Secondary Relationship");
				}
			}
			//Provider Idents:
			/*ProviderSupplementalID[] providerIdents=ElectIDs.GetRequiredIdents(carrier.ElectID);
				//No longer any required supplemental IDs
			for(int i=0;i<providerIdents.Length;i++){
				if(!ProviderIdents.IdentExists(providerIdents[i],billProv.ProvNum,carrier.ElectID)){
					if(retVal!="")
						strb.Append(",";
					strb.Append("Billing Prov Supplemental ID:"+providerIdents[i].ToString();
				}
			}*/
			if(insPlan.SubscriberID.Length<2){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("SubscriberID");
			}
			Patient patient=Patients.GetPat(claim.PatNum);
			Patient subscriber=Patients.GetPat(insPlan.Subscriber);
			if(claim.PatNum != insPlan.Subscriber//if patient is not subscriber
				&& claim.PatRelat==Relat.Self){//and relat is self
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Claim Relationship");
			}
			if(patient.Address==""){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Patient Address");
			}
			if(patient.City.Length<2){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Patient City");
			}
			if(patient.State.Length!=2){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Patient State");
			}
			if(patient.Zip.Length<3){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Patient Zip");
			}
			if(patient.Birthdate.Year<1880){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Patient Birthdate");
			}
			if(claim.AccidentRelated=="A" && claim.AccidentST.Length!=2){//auto accident with no state
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Auto accident State");
			}
			/*if(clearhouse.ISA08=="113504607" && claim.Attachments.Count>0) {//If Tesia and has attachments
				string storedFile;
				for(int c=0;c<claim.Attachments.Count;c++) {
					storedFile=ODFileUtils.CombinePaths(FormEmailMessageEdit.GetAttachPath(),claim.Attachments[c].ActualFileName);
					if(!File.Exists(storedFile)){
						if(retVal!="")
							strb.Append(",";
						strb.Append("attachments missing";
						break;
					}
				}
			}*/
			//Warning if attachments are listed as Mail even though we are sending electronically.
			bool pwkNeeded=false;
			if(claim.AttachedFlags!="Mail"){//in other words, if there are additional flags.
				pwkNeeded=true;
			}
			if(claim.Radiographs>0 || claim.AttachedImages>0 || claim.AttachedModels>0){
				pwkNeeded=true;
			}
			if(claim.AttachedFlags.Contains("Mail") && pwkNeeded){
				if(warning!="")
					warning+=",";
				warning+="Attachments set to Mail";
			}
			//Warning if any PWK segments are needed, and there is no ID code.
			if(pwkNeeded && claim.AttachmentID==""){
				if(warning!="")
					warning+=",";
				warning+="Attachment ID missing";
			}
			List<ClaimProc> claimProcList=ClaimProcs.Refresh(patient.PatNum);
			List<ClaimProc> claimProcs=ClaimProcs.GetForSendClaim(claimProcList,claim.ClaimNum);
			List<Procedure> procList=Procedures.Refresh(claim.PatNum);
			Procedure proc;
			ProcedureCode procCode;
			bool princDiagExists=false;
			for(int i=0;i<claimProcs.Count;i++){
				proc=Procedures.GetProcFromList(procList,claimProcs[i].ProcNum);
				procCode=ProcedureCodes.GetProcCode(proc.CodeNum);		
				if(procCode.TreatArea==TreatmentArea.Arch && proc.Surf==""){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append(procCode.AbbrDesc+" missing arch");
				}
				if(procCode.TreatArea==TreatmentArea.ToothRange && proc.ToothRange==""){
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append(procCode.AbbrDesc+" tooth range");
				}
				if((procCode.TreatArea==TreatmentArea.Tooth || procCode.TreatArea==TreatmentArea.Surf)
					&& !Tooth.IsValidDB(proc.ToothNum)) {
					if(strb.Length!=0) {
						strb.Append(",");
					}
					strb.Append(procCode.AbbrDesc+" tooth number");
				}
				if(procCode.IsProsth){
					if(proc.Prosthesis==""){//they didn't enter whether Initial or Replacement
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append(procCode.AbbrDesc+" Prosthesis");
					}
					if(proc.Prosthesis=="R"
						&& proc.DateOriginalProsth.Year<1880)
					{//if a replacement, they didn't enter a date
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append(procCode.AbbrDesc+" Prosth Date");
					}
				}
				if(insPlan.IsMedical){
					if(proc.DiagnosticCode==""){
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Procedure Diagnosis");
					}
					if(proc.IsPrincDiag && proc.DiagnosticCode!=""){
						princDiagExists=true;
					}
				}
				if(claim.ProvTreat!=proc.ProvNum && PrefC.GetBool(PrefName.EclaimsSeparateTreatProv)){
					treatProv=ProviderC.ListLong[Providers.GetIndexLong(proc.ProvNum)];
					if(treatProv.LName==""){
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Treating Prov LName");
					}
					if(treatProv.FName==""){
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Treating Prov FName");
					}
					if(treatProv.SSN.Length<2){
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Treating Prov SSN");
					}
					if(treatProv.NationalProvID.Length<2) {
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Treating Prov NPI");
					}
					if(treatProv.StateLicense==""){
						if(strb.Length!=0) {
							strb.Append(",");
						}
						strb.Append("Treating Prov Lic #");
					}
					//will add any other checks as needed. Can't think of any others at the moment.
				}
			}//for int i claimProcs
			if(insPlan.IsMedical && !princDiagExists){
				if(strb.Length!=0) {
					strb.Append(",");
				}
				strb.Append("Princ Diagnosis");
			}

			/*
						if(==""){
							if(strb.Length!=0) {
								strb.Append(",");
							}
							strb.Append("";
						}*/

			return strb.ToString();
		}

	///<summary>Copies the given file to an archive directory within the same directory as the file.</summary>
		private static void CopyToArchive(string fileName){
			string direct=Path.GetDirectoryName(fileName);
			string fileOnly=Path.GetFileName(fileName);
			string archiveDir=ODFileUtils.CombinePaths(direct,"archive");
			if(!Directory.Exists(archiveDir)){
				Directory.CreateDirectory(archiveDir);
			}
			File.Copy(fileName,ODFileUtils.CombinePaths(archiveDir,fileOnly),true);
		}

		

		
	



		

	}
}
