using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Drawing;

namespace OpenDentalWebService {
	///<summary>This file is generated automatically by the crud, do not make any changes to this file because they will get overwritten.</summary>
	public class Commlog {

		///<summary></summary>
		public static string Serialize(OpenDentBusiness.Commlog commlog) {
			StringBuilder sb=new StringBuilder();
			sb.Append("<Commlog>");
			sb.Append("<CommlogNum>").Append(commlog.CommlogNum).Append("</CommlogNum>");
			sb.Append("<PatNum>").Append(commlog.PatNum).Append("</PatNum>");
			sb.Append("<CommDateTime>").Append(commlog.CommDateTime.ToString("yyyyMMddHHmmss")).Append("</CommDateTime>");
			sb.Append("<CommType>").Append(commlog.CommType).Append("</CommType>");
			sb.Append("<Note>").Append(SerializeStringEscapes.EscapeForXml(commlog.Note)).Append("</Note>");
			sb.Append("<Mode_>").Append((int)commlog.Mode_).Append("</Mode_>");
			sb.Append("<SentOrReceived>").Append((int)commlog.SentOrReceived).Append("</SentOrReceived>");
			sb.Append("<UserNum>").Append(commlog.UserNum).Append("</UserNum>");
			sb.Append("<Signature>").Append(SerializeStringEscapes.EscapeForXml(commlog.Signature)).Append("</Signature>");
			sb.Append("<SigIsTopaz>").Append((commlog.SigIsTopaz)?1:0).Append("</SigIsTopaz>");
			sb.Append("<DateTStamp>").Append(commlog.DateTStamp.ToString("yyyyMMddHHmmss")).Append("</DateTStamp>");
			sb.Append("<DateTimeEnd>").Append(commlog.DateTimeEnd.ToString("yyyyMMddHHmmss")).Append("</DateTimeEnd>");
			sb.Append("</Commlog>");
			return sb.ToString();
		}

		///<summary></summary>
		public static OpenDentBusiness.Commlog Deserialize(string xml) {
			OpenDentBusiness.Commlog commlog=new OpenDentBusiness.Commlog();
			using(XmlReader reader=XmlReader.Create(new StringReader(xml))) {
				reader.MoveToContent();
				while(reader.Read()) {
					//Only detect start elements.
					if(!reader.IsStartElement()) {
						continue;
					}
					switch(reader.Name) {
						case "CommlogNum":
							commlog.CommlogNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "PatNum":
							commlog.PatNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "CommDateTime":
							commlog.CommDateTime=DateTime.ParseExact(reader.ReadContentAsString(),"yyyyMMddHHmmss",null);
							break;
						case "CommType":
							commlog.CommType=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "Note":
							commlog.Note=reader.ReadContentAsString();
							break;
						case "Mode_":
							commlog.Mode_=(OpenDentBusiness.CommItemMode)System.Convert.ToInt32(reader.ReadContentAsString());
							break;
						case "SentOrReceived":
							commlog.SentOrReceived=(OpenDentBusiness.CommSentOrReceived)System.Convert.ToInt32(reader.ReadContentAsString());
							break;
						case "UserNum":
							commlog.UserNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "Signature":
							commlog.Signature=reader.ReadContentAsString();
							break;
						case "SigIsTopaz":
							commlog.SigIsTopaz=reader.ReadContentAsString()!="0";
							break;
						case "DateTStamp":
							commlog.DateTStamp=DateTime.ParseExact(reader.ReadContentAsString(),"yyyyMMddHHmmss",null);
							break;
						case "DateTimeEnd":
							commlog.DateTimeEnd=DateTime.ParseExact(reader.ReadContentAsString(),"yyyyMMddHHmmss",null);
							break;
					}
				}
			}
			return commlog;
		}


	}
}