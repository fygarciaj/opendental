using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Drawing;

namespace OpenDentalWebService {
	///<summary>This file is generated automatically by the crud, do not make any changes to this file because they will get overwritten.</summary>
	public class SupplyOrderItem {

		///<summary></summary>
		public static string Serialize(OpenDentBusiness.SupplyOrderItem supplyorderitem) {
			StringBuilder sb=new StringBuilder();
			sb.Append("<SupplyOrderItem>");
			sb.Append("<SupplyOrderItemNum>").Append(supplyorderitem.SupplyOrderItemNum).Append("</SupplyOrderItemNum>");
			sb.Append("<SupplyOrderNum>").Append(supplyorderitem.SupplyOrderNum).Append("</SupplyOrderNum>");
			sb.Append("<SupplyNum>").Append(supplyorderitem.SupplyNum).Append("</SupplyNum>");
			sb.Append("<Qty>").Append(supplyorderitem.Qty).Append("</Qty>");
			sb.Append("<Price>").Append(supplyorderitem.Price).Append("</Price>");
			sb.Append("</SupplyOrderItem>");
			return sb.ToString();
		}

		///<summary></summary>
		public static OpenDentBusiness.SupplyOrderItem Deserialize(string xml) {
			OpenDentBusiness.SupplyOrderItem supplyorderitem=new OpenDentBusiness.SupplyOrderItem();
			using(XmlReader reader=XmlReader.Create(new StringReader(xml))) {
				reader.MoveToContent();
				while(reader.Read()) {
					//Only detect start elements.
					if(!reader.IsStartElement()) {
						continue;
					}
					switch(reader.Name) {
						case "SupplyOrderItemNum":
							supplyorderitem.SupplyOrderItemNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "SupplyOrderNum":
							supplyorderitem.SupplyOrderNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "SupplyNum":
							supplyorderitem.SupplyNum=System.Convert.ToInt64(reader.ReadContentAsString());
							break;
						case "Qty":
							supplyorderitem.Qty=System.Convert.ToInt32(reader.ReadContentAsString());
							break;
						case "Price":
							supplyorderitem.Price=System.Convert.ToDouble(reader.ReadContentAsString());
							break;
					}
				}
			}
			return supplyorderitem;
		}


	}
}