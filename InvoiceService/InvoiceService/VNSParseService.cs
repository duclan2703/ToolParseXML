using InvoiceService.Common;
using InvoiceService.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace InvoiceService
{
    public class VNSParseService
    {
        static ILog log = LogManager.GetLogger(typeof(VNSParseService));

        public static VNSUploadData ParseIInvoice(string filePath, List<VNSMapping> Lstvnsmap, List<SerialMapping> lstserialMapping, ref string message, out string comtaxcode)
        {
            message = "";
            comtaxcode = "";
            VNSUploadData uploaddata = new VNSUploadData();
            List<VNSUploadDataDetail> uploaddetail = new List<VNSUploadDataDetail>();
            try
            {
                if (!File.Exists(filePath))
                {
                    message = "Đường dẫn file " + filePath + " không tìm thấy";
                    return null;
                }
                DataSet dSet = new DataSet();
                dSet.ReadXml(filePath);

                DataRow dtInvoice = dSet.Tables["Invoice"].Rows[0];
                if (string.IsNullOrWhiteSpace(dtInvoice["InvSeries"].ToString()))
                {
                    message = "Ký hiệu [InvSeries] không có dữ liệu.";
                    return null;
                }

                VNSInvoiceType InvvoiceType;
                string invType = dtInvoice["InvType"].ToString(); // loại hóa đơn
                Enum.TryParse(invType, out InvvoiceType);
                uploaddata.InvType = InvvoiceType;

                var _strInvDate = dtInvoice["ArisingDate"].ToString();
                uploaddata.InvNo = dtInvoice["InvNo"].ToString();
                uploaddata.Fkey = dtInvoice["Fkey"].ToString();
                uploaddata.RefFkey = dtInvoice["RefFkey"].ToString();

                if (uploaddata.InvType == VNSInvoiceType.New)
                {
                    if (string.IsNullOrWhiteSpace(uploaddata.Fkey))
                    {
                        message = "[Fkey] không có giá trị";
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(uploaddata.InvNo))
                    {
                        message = "[InvNo] không có giá trị";
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(_strInvDate))
                    {
                        message = "[ArisingDate] không có giá trị";
                        return null;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(uploaddata.RefFkey))
                    {
                        message = "[RefFkey] không có giá trị";
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(_strInvDate))
                    {
                        _strInvDate = DateTime.Today.ToString("dd/MM/yyyy");
                    }
                }

                uploaddata.InvSeries = dtInvoice["InvSeries"].ToString().Trim(); // AA/19T

                var serialMapping = lstserialMapping.FirstOrDefault(x => x.Serial == uploaddata.InvSeries);
                if (serialMapping == null)
                {
                    message = "Không tìm thấy ký hiệu hoá đơn: " + uploaddata.InvSeries;
                    return null;
                }
                uploaddata.InvPattern = serialMapping.Pattern;

                uploaddata.Comp = !string.IsNullOrEmpty(dtInvoice["comp"].ToString()) ? dtInvoice["comp"].ToString().Trim() : "";

                uploaddata.InvBussi = !string.IsNullOrEmpty(dtInvoice["InvBussi"].ToString()) ? dtInvoice["InvBussi"].ToString().Trim() : "";

                uploaddata.SrcModule = !string.IsNullOrEmpty(dtInvoice["Srcmodule"].ToString()) ? dtInvoice["Srcmodule"].ToString() : "";

                uploaddata.SLAcount = !string.IsNullOrEmpty(dtInvoice["SLAcount"].ToString()) ? dtInvoice["SLAcount"].ToString() : null;
                //check bộ key tương ứng comtaxcode
                var mapping = new VNSMapping();
                if (uploaddata.Comp == "V1" && uploaddata.SrcModule == "SL")
                {
                    if (!string.IsNullOrEmpty(uploaddata.InvBussi))
                        mapping = Lstvnsmap.FirstOrDefault(x => x.Comp == uploaddata.Comp && x.Srcmodules.Contains(uploaddata.SrcModule) && x.InvBuss.Contains(uploaddata.InvBussi));
                }
                else
                    mapping = Lstvnsmap.FirstOrDefault(x => x.Comp == uploaddata.Comp && x.Srcmodules.Contains(uploaddata.SrcModule));
                if (mapping == null)
                {
                    message = "Không tìm thấy ComTaxCode tương ứng với bộ key";
                    return null;
                }
                uploaddata.ComTaxCode = comtaxcode = mapping.ComTaxCode;
                var ArisingDate = dtInvoice["ArisingDate"].ToString(); // Ngày hóa đơn ArisingDate
                uploaddata.ArisingDate = DateTime.ParseExact(ArisingDate, new string[] { "dd/MM/yyyy", "dd/M/yyyy", "dd.MM.yyyy" }, new CultureInfo("en-US"), DateTimeStyles.None);

                uploaddata.OrderNo = !string.IsNullOrEmpty(dtInvoice["OrderNo"].ToString()) ? dtInvoice["OrderNo"].ToString() : ""; // Số đơn hàng : OrderNo

                uploaddata.DeliveryNumber = !string.IsNullOrEmpty(dtInvoice["DeliveryNo"].ToString()) ? dtInvoice["DeliveryNo"].ToString() : ""; // Phiếu xuất kho số : DeliveryNumber

                uploaddata.DeliveryDate = !string.IsNullOrEmpty(dtInvoice["DeliveryDate"].ToString()) ? dtInvoice["DeliveryDate"].ToString() : ""; // Ngày giao hàng : DeiveryDate

                uploaddata.CusCode = !string.IsNullOrEmpty(dtInvoice["CusCode"].ToString()) ? dtInvoice["CusCode"].ToString() : "";

                uploaddata.SLAcount = !string.IsNullOrEmpty(dtInvoice["SLAcount"].ToString()) ? dtInvoice["SLAcount"].ToString() : null;

                uploaddata.Buyer = !string.IsNullOrEmpty(dtInvoice["Buyer"].ToString()) ? dtInvoice["Buyer"].ToString() : null;

                uploaddata.CusName = !string.IsNullOrEmpty(dtInvoice["CusName"].ToString()) ? dtInvoice["CusName"].ToString().Trim() : ""; //

                uploaddata.CusAddress = !string.IsNullOrEmpty(dtInvoice["CusAddress"].ToString()) ? dtInvoice["CusAddress"].ToString().Trim() : ""; //Địa chỉ: CusAddress = CusHouseNo + CusStreet + CusDistrict + CusCity + CusCountry

                if (uploaddata.ComTaxCode == "0303313001")
                    uploaddata.DeliveryPlace = "811-813 (thuộc Lô H8-2) đường Nguyễn Văn Linh, Phường Tân Phong, Quận 7, Thành phố Hồ Chí Minh, Việt Nam";
                else
                    uploaddata.DeliveryPlace = mapping.ComAdress;

                uploaddata.CusPhone = !string.IsNullOrEmpty(dtInvoice["CusPhone"].ToString()) ? dtInvoice["CusPhone"].ToString() : ""; // điện thoại KH

                uploaddata.CusEmail = !string.IsNullOrEmpty(dtInvoice["CusEmail"].ToString()) ? dtInvoice["CusEmail"].ToString() : ""; // Email KH

                uploaddata.CusTaxCode = !string.IsNullOrEmpty(dtInvoice["CusTaxCode"].ToString()) ? dtInvoice["CusTaxCode"].ToString() : ""; // Mã số thuế KH

                uploaddata.PaymentMethod = "chuyển khoản/tiền mặt";

                uploaddata.Model = !string.IsNullOrEmpty(dtInvoice["Model"].ToString()) ? dtInvoice["Model"].ToString() : "";

                uploaddata.Color = !string.IsNullOrEmpty(dtInvoice["Colour"].ToString()) ? dtInvoice["Colour"].ToString() : ""; // Màu xe: Color

                uploaddata.Upholstery = !string.IsNullOrEmpty(dtInvoice["Upholstery"].ToString()) ? dtInvoice["Upholstery"].ToString() : ""; // Nệm bọc : Upholstery

                if (uploaddata.SrcModule == "VS")
                    uploaddata.CarType = "ô tô con";

                uploaddata.ChassisNumber = !string.IsNullOrEmpty(dtInvoice["VINNo"].ToString()) ? dtInvoice["VINNo"].ToString() : ""; // Số khung: ChassisNumber

                uploaddata.EngineNumber = !string.IsNullOrEmpty(dtInvoice["EngineNo"].ToString()) ? dtInvoice["EngineNo"].ToString() : ""; // Số máy: EngineNumber

                uploaddata.ProductionYear = !string.IsNullOrEmpty(dtInvoice["ProYear"].ToString()) ? dtInvoice["ProYear"].ToString() : ""; // Năm sản xuất: ProductionYear

                uploaddata.Inclusive = !string.IsNullOrEmpty(dtInvoice["Inclusive"].ToString()) ? dtInvoice["Inclusive"].ToString() : "";

                uploaddata.RegNo = !string.IsNullOrEmpty(dtInvoice["RegNo"].ToString()) ? dtInvoice["RegNo"].ToString() : "";

                uploaddata.KmNo = !string.IsNullOrEmpty(dtInvoice["KM"].ToString()) ? dtInvoice["KM"].ToString() : "";

                uploaddata.RegDate = !string.IsNullOrEmpty(dtInvoice["DateOfFirstReg"].ToString()) ? dtInvoice["DateOfFirstReg"].ToString() : "";

                uploaddata.ServiceDate = !string.IsNullOrEmpty(dtInvoice["LastDate"].ToString()) ? dtInvoice["LastDate"].ToString() : "";

                uploaddata.DealerCode = !string.IsNullOrEmpty(dtInvoice["DealerCode"].ToString()) ? dtInvoice["DealerCode"].ToString() : "";

                uploaddata.RONo = !string.IsNullOrEmpty(dtInvoice["RONo"].ToString()) ? dtInvoice["RONo"].ToString() : "";

                uploaddata.RODate = !string.IsNullOrEmpty(dtInvoice["RODate"].ToString()) ? dtInvoice["RODate"].ToString() : "";

                uploaddata.ReceivedTime = !string.IsNullOrEmpty(dtInvoice["RecTime"].ToString()) ? dtInvoice["RecTime"].ToString() : "";

                uploaddata.ReceivedDate = !string.IsNullOrEmpty(dtInvoice["RecDate"].ToString()) ? dtInvoice["RecDate"].ToString() : "";

                uploaddata.ReceivedBy = !string.IsNullOrEmpty(dtInvoice["RecBy"].ToString()) ? dtInvoice["RecBy"].ToString() : "";

                uploaddata.BroughtBy = !string.IsNullOrEmpty(dtInvoice["BroughtBy"].ToString()) ? dtInvoice["BroughtBy"].ToString() : "";

                uploaddata.DeadlineTime = !string.IsNullOrEmpty(dtInvoice["DeadlineTime"].ToString()) ? dtInvoice["DeadlineTime"].ToString() : "";

                uploaddata.DeadlineDate = !string.IsNullOrEmpty(dtInvoice["DeadlineDate"].ToString()) ? dtInvoice["DeadlineDate"].ToString() : "";

                uploaddata.Notes = !string.IsNullOrEmpty(dtInvoice["Notes"].ToString()) ? dtInvoice["Notes"].ToString() : "";

                uploaddata.Reason = !string.IsNullOrEmpty(dtInvoice["Reason"].ToString()) ? dtInvoice["Reason"].ToString() : "";

                uploaddata.SLType = !string.IsNullOrEmpty(dtInvoice["SLType"].ToString()) ? dtInvoice["SLType"].ToString() : "";

                var Total = !string.IsNullOrEmpty(dtInvoice["Total"].ToString()) ? dtInvoice["Total"].ToString() : "0"; // cộng tiền hàng trước thuế : Amount
                uploaddata.TotalBeforeTax = !string.IsNullOrEmpty(Total) ? decimal.Parse(Total) : 0;

                var VATRate = !string.IsNullOrEmpty(dtInvoice["VATRate"].ToString()) ? dtInvoice["VATRate"].ToString() : "0";
                uploaddata.VATRate = !string.IsNullOrEmpty(VATRate) ? float.Parse(VATRate) : 0;

                var VATAmount = !string.IsNullOrEmpty(dtInvoice["VATAmount"].ToString()) ? dtInvoice["VATAmount"].ToString() : "0";
                uploaddata.VATAmount = !string.IsNullOrEmpty(VATAmount) ? decimal.Parse(VATAmount) : 0;

                var Amount = !string.IsNullOrEmpty(dtInvoice["Amount"].ToString()) ? dtInvoice["Amount"].ToString() : "0";
                uploaddata.Amount = !string.IsNullOrEmpty(Amount) ? decimal.Parse(Amount) : 0;

                var Lubricants = !string.IsNullOrEmpty(dtInvoice["Lubricants"].ToString()) ? dtInvoice["Lubricants"].ToString() : "0";
                uploaddata.Lubricants = !string.IsNullOrEmpty(Lubricants) ? decimal.Parse(Lubricants) : 0;

                var LabourCharge = !string.IsNullOrEmpty(dtInvoice["LabourCharge"].ToString()) ? dtInvoice["LabourCharge"].ToString() : "0";
                uploaddata.LabourCharge = !string.IsNullOrEmpty(LabourCharge) ? decimal.Parse(LabourCharge) : 0;

                var Parts = !string.IsNullOrEmpty(dtInvoice["Parts"].ToString()) ? dtInvoice["Parts"].ToString() : "0";
                uploaddata.Parts = !string.IsNullOrEmpty(Parts) ? decimal.Parse(Parts) : 0;

                var Subcontract = !string.IsNullOrEmpty(dtInvoice["Subcontract"].ToString()) ? dtInvoice["Subcontract"].ToString() : "0";
                uploaddata.Subcontract = !string.IsNullOrEmpty(Subcontract) ? decimal.Parse(Subcontract) : 0;

                var Miscellaneous = !string.IsNullOrEmpty(dtInvoice["Miscellaneous"].ToString()) ? dtInvoice["Miscellaneous"].ToString() : "0";
                uploaddata.Miscellaneous = !string.IsNullOrEmpty(Miscellaneous) ? decimal.Parse(Miscellaneous) : 0;

                var ExceedInsurance = !string.IsNullOrEmpty(dtInvoice["ExceedInsurance"].ToString()) ? dtInvoice["Miscellaneous"].ToString() : "0";
                uploaddata.ExceedInsurance = !string.IsNullOrEmpty(ExceedInsurance) ? decimal.Parse(ExceedInsurance) : 0;

                int stt = 1;
                foreach (DataRow dtProduct in dSet.Tables["Product"].Rows)
                {
                    VNSUploadDataDetail prd = new VNSUploadDataDetail();
                    prd.Position = stt;
                    prd.Code = !string.IsNullOrEmpty(dtProduct["Code"].ToString()) ? dtProduct["Code"].ToString() : ""; // Mã sản phẩm : Code

                    prd.Name = !string.IsNullOrEmpty(dtProduct["ProdName1"].ToString()) ? dtProduct["ProdName1"].ToString() : ""; // Mô tả: Name

                    prd.Name2 = !string.IsNullOrEmpty(dtProduct["ProdName2"].ToString()) ? dtProduct["ProdName2"].ToString() : ""; // Mô tả: Name

                    prd.Name3 = !string.IsNullOrEmpty(dtProduct["ProdName3"].ToString()) ? dtProduct["ProdName3"].ToString() : ""; // Mô tả: Name

                    prd.Name4 = !string.IsNullOrEmpty(dtProduct["ProdName4"].ToString()) ? dtProduct["ProdName4"].ToString() : ""; // Mô tả: Name

                    prd.Unit = !string.IsNullOrEmpty(dtProduct["ProdUnit"].ToString()) ? dtProduct["ProdUnit"].ToString() : ""; // Mô tả: Name

                    var Quantity = !string.IsNullOrWhiteSpace(dtProduct["ProdQuantity"].ToString()) ? dtProduct["ProdQuantity"].ToString() : "0"; // Số lượng
                    prd.Quantity = decimal.Parse(Quantity);

                    var UnitPrice = !string.IsNullOrWhiteSpace(dtProduct["ProdPrice"].ToString()) ? dtProduct["ProdPrice"].ToString() : "0"; // Đơn giá
                    prd.Price = decimal.Parse(UnitPrice);

                    var ProdTotal = !string.IsNullOrWhiteSpace(dtProduct["Total"].ToString()) ? dtProduct["Total"].ToString() : "0"; //Tổng tiền hàng trước thuế : ProdTotal
                    prd.Total = decimal.Parse(ProdTotal);

                    var ProdVATRate = !string.IsNullOrWhiteSpace(dtProduct["VatRate"].ToString()) ? dtProduct["VatRate"].ToString() : "0"; // Thuế tiêu thục đặc biệt: VATRate
                    prd.VATRate = float.Parse(ProdVATRate);

                    var ProdVATAmount = !string.IsNullOrWhiteSpace(dtProduct["VATAmount"].ToString()) ? dtProduct["VATAmount"].ToString() : "0"; //Tiền thuế tiêu thụ đặc biệt : VATAmount
                    prd.VATAmount = decimal.Parse(ProdVATAmount);

                    prd.Description = !string.IsNullOrEmpty(dtProduct["Description"].ToString()) ? dtProduct["Description"].ToString() : ""; // Diễn giải

                    prd.Contribution = !string.IsNullOrEmpty(dtProduct["Contribution"].ToString()) ? dtProduct["Contribution"].ToString() : "";

                    var ProdAmount = !string.IsNullOrWhiteSpace(dtProduct["Amount"].ToString()) ? dtProduct["Amount"].ToString() : "0"; //Tiền thuế tiêu thụ đặc biệt : VATAmount
                    prd.Amount = decimal.Parse(ProdAmount);
                    uploaddata.Details.Add(prd);
                    stt++;
                }

                uploaddata.FileName = Path.GetFileName(filePath);
                uploaddata.Status = VNSUploadStatus.CreateNew;
                uploaddata.XMLData = makeStringInvoice(uploaddata, out message);
                return uploaddata;
            }
            catch (Exception ex)
            {
                log.Error("ParseXML error-" + ex);
                message = ex.ToString();
                return null;
            }
        }

        private static string makeStringInvoice(VNSUploadData invoices, out string ErrorMessage)
        {
            ErrorMessage = "";
            try
            {
                StringBuilder sbInvoice = new StringBuilder("<Invoice>");
                sbInvoice.AppendFormat("<InvType>{0}</InvType>", invoices.InvType);
                sbInvoice.AppendFormat("<FileName>{0}</FileName>", convertSpecialCharacter(invoices.FileName));
                sbInvoice.AppendFormat("<RefFkey>{0}</RefFkey>", invoices.RefFkey);
                sbInvoice.AppendFormat("<Description>{0}</Description>", convertSpecialCharacter(invoices.Description));
                sbInvoice.AppendFormat("<InvNo>{0}</InvNo>", invoices.InvNo);
                sbInvoice.AppendFormat("<InvPattern>{0}</InvPattern>", invoices.InvPattern);
                sbInvoice.AppendFormat("<InvSerial>{0}</InvSerial>", invoices.InvSeries);
                sbInvoice.AppendFormat("<CusCode>{0}</CusCode>", invoices.SLAcount ?? invoices.CusCode);
                sbInvoice.AppendFormat("<CusName>{0}</CusName>", invoices.CusName);
                sbInvoice.AppendFormat("<Buyer>{0}</Buyer>", convertSpecialCharacter(invoices.Buyer));
                sbInvoice.AppendFormat("<CusAddress>{0}</CusAddress>", convertSpecialCharacter(invoices.CusAddress));
                sbInvoice.AppendFormat("<CusEmail>{0}</CusEmail>", convertSpecialCharacter(invoices.CusEmail));
                sbInvoice.AppendFormat("<CusTaxCode>{0}</CusTaxCode>", convertSpecialCharacter(invoices.CusTaxCode));
                sbInvoice.AppendFormat("<OrderNo>{0}</OrderNo>", convertSpecialCharacter(invoices.OrderNo));
                sbInvoice.AppendFormat("<DeliveryNumber>{0}</DeliveryNumber>", convertSpecialCharacter(invoices.DeliveryNumber));
                sbInvoice.AppendFormat("<DeliveryDate>{0}</DeliveryDate>", invoices.DeliveryDate);
                sbInvoice.AppendFormat("<DeliveryPlace>{0}</DeliveryPlace>", convertSpecialCharacter(invoices.DeliveryPlace));
                sbInvoice.AppendFormat("<PaymentMethod>{0}</PaymentMethod>", invoices.PaymentMethod);
                sbInvoice.AppendFormat("<Model>{0}</Model>", convertSpecialCharacter(invoices.Model));
                sbInvoice.AppendFormat("<CarType>{0}</CarType>", convertSpecialCharacter(invoices.CarType));
                sbInvoice.AppendFormat("<Color>{0}</Color>", invoices.Color);
                sbInvoice.AppendFormat("<Inclusive>{0}</Inclusive>", convertSpecialCharacter(invoices.Inclusive));
                sbInvoice.AppendFormat("<Upholstery>{0}</Upholstery>", convertSpecialCharacter(invoices.Upholstery));
                sbInvoice.AppendFormat("<ChassisNumber>{0}</ChassisNumber>", convertSpecialCharacter(invoices.ChassisNumber));
                sbInvoice.AppendFormat("<EngineNumber>{0}</EngineNumber>", convertSpecialCharacter(invoices.EngineNumber));
                sbInvoice.AppendFormat("<ProductionYear>{0}</ProductionYear>", invoices.ProductionYear);
                sbInvoice.AppendFormat("<RONo>{0}</RONo>", convertSpecialCharacter(invoices.RONo));
                sbInvoice.AppendFormat("<RODate>{0}</RODate>", invoices.RODate);
                sbInvoice.AppendFormat("<RegNo>{0}</RegNo>", convertSpecialCharacter(invoices.RegNo));
                sbInvoice.AppendFormat("<ReceivedTime>{0}</ReceivedTime>", convertSpecialCharacter(invoices.ReceivedTime));
                sbInvoice.AppendFormat("<ReceivedDate>{0}</ReceivedDate>", invoices.ReceivedDate);
                sbInvoice.AppendFormat("<KmNo>{0}</KmNo>", invoices.KmNo);
                sbInvoice.AppendFormat("<BroughtBy>{0}</BroughtBy>", convertSpecialCharacter(invoices.BroughtBy));
                sbInvoice.AppendFormat("<RegDate>{0}</RegDate>", invoices.RegDate);
                sbInvoice.AppendFormat("<ServiceDate>{0}</ServiceDate>", invoices.ServiceDate);
                sbInvoice.AppendFormat("<ReceivedBy>{0}</ReceivedBy>", invoices.ReceivedBy);
                sbInvoice.AppendFormat("<DeadlineTime>{0}</DeadlineTime>", invoices.DeadlineTime);
                sbInvoice.AppendFormat("<DeadlineDate>{0}</DeadlineDate>", invoices.DeadlineDate);

                sbInvoice.Append(makeStringProducts(invoices.Details));
                sbInvoice.AppendFormat("<ArisingDate>{0}</ArisingDate>", invoices.ArisingDate.ToString("dd/MM/yyyy"));
                sbInvoice.AppendFormat("<LabourCharge>{0}</LabourCharge>", invoices.LabourCharge);
                sbInvoice.AppendFormat("<Parts>{0}</Parts>", invoices.Parts);
                sbInvoice.AppendFormat("<Lubricants>{0}</Lubricants>", invoices.Lubricants);
                sbInvoice.AppendFormat("<Subcontract>{0}</Subcontract>", invoices.Subcontract);
                sbInvoice.AppendFormat("<Miscellaneous>{0}</Miscellaneous>", invoices.Miscellaneous);
                sbInvoice.AppendFormat("<ExceedInsurance>{0}</ExceedInsurance>", invoices.ExceedInsurance);
                sbInvoice.AppendFormat("<Total>{0}</Total>", invoices.TotalBeforeTax);
                sbInvoice.AppendFormat("<VATRate>{0}</VATRate>", invoices.VATRate);
                sbInvoice.AppendFormat("<VATAmount>{0}</VATAmount>", invoices.VATAmount);
                sbInvoice.AppendFormat("<Amount>{0}</Amount>", invoices.Amount);
                sbInvoice.AppendFormat("<Reason>{0}</Reason>", invoices.Reason);
                string tienbangchu = null;
                long amountinvoice = (long)invoices.Amount;
                tienbangchu = NumberToLeter.DocTienBangChu(amountinvoice);
                sbInvoice.AppendFormat("<AmountInWords>{0}</AmountInWords>", tienbangchu);
                sbInvoice.Append("</Invoice>");
                return sbInvoice.ToString();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                ErrorMessage = ex.Message;
                return null;
            }
        }

        private static string convertSpecialCharacter(string xmlData)
        {
            return "<![CDATA[" + xmlData + "]]>";
        }

        private static string makeStringProducts(List<VNSUploadDataDetail> lstproduct)
        {
            StringBuilder strProducts = new StringBuilder("<Products>");
            foreach (var item in lstproduct)
            {
                strProducts.Append("<Product>");
                strProducts.AppendFormat("<Code>{0}</Code>", convertSpecialCharacter(item.Code));
                strProducts.AppendFormat("<ProdName>{0}</ProdName>", convertSpecialCharacter(item.Name));
                strProducts.AppendFormat("<ProdName2>{0}</ProdName2>", convertSpecialCharacter(item.Name2));
                strProducts.AppendFormat("<ProdName3>{0}</ProdName3>", convertSpecialCharacter(item.Name3));
                strProducts.AppendFormat("<ProdName4>{0}</ProdName4>", convertSpecialCharacter(item.Name4));
                strProducts.AppendFormat("<Contribution>{0}</Contribution>", convertSpecialCharacter(item.Contribution));
                strProducts.AppendFormat("<Description>{0}</Description>", convertSpecialCharacter(item.Description));
                strProducts.AppendFormat("<ProdUnit>{0}</ProdUnit>", item.Unit);
                strProducts.AppendFormat("<ProdQuantity>{0}</ProdQuantity>", item.Quantity);
                strProducts.AppendFormat("<ProdPrice>{0}</ProdPrice>", item.Price);
                strProducts.AppendFormat("<Total>{0}</Total>", item.Total);
                strProducts.AppendFormat("<VATRate>{0}</VATRate>", item.VATRate);
                strProducts.AppendFormat("<VATAmount>{0}</VATAmount>", item.VATAmount);
                strProducts.AppendFormat("<Amount>{0}</Amount>", item.Amount);
                strProducts.Append("</Product>");
            }
            strProducts.Append("</Products>");
            return strProducts.ToString();
        }
    }
}
