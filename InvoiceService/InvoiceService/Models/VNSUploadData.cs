using System;
using System.Collections.Generic;

namespace InvoiceService.Models
{
    public class VNSUploadData
    {
        public virtual long ID { get; set; }
        public virtual string ComTaxCode { get; set; }
        public virtual int ComID { get; set; }
        public virtual string InvBussi { get; set; }
        public virtual string XMLData { get; set; }
        public virtual string XMLTemplate { get; set; }
        public virtual string FileName { get; set; }
        public virtual VNSInvoiceType InvType { get; set; }
        public virtual VNSUploadStatus Status { get; set; }
        public virtual string Comp { get; set; }
        public virtual string InvPattern { get; set; }
        public virtual string InvSeries { get; set; }
        public virtual string InvNo { get; set; }
        public virtual DateTime ArisingDate { get; set; }
        public virtual string Buyer { get; set; }
        public virtual string RefFkey { get; set; }
        public virtual string Fkey { get; set; }
        public virtual string PaymentMethod { get; set; }
        public virtual string OrderNo { get; set; }
        public virtual string DeliveryNumber { get; set; }
        public virtual string DeliveryDate { get; set; }
        public virtual string CusCode { get; set; }
        public virtual string SLAcount { get; set; }
        public virtual string CusName { get; set; }
        public virtual string CusAddress { get; set; }
        public virtual string CusTaxCode { get; set; }
        public virtual string CusPhone { get; set; }
        public virtual string CusEmail { get; set; }
        public virtual string ProdCode { get; set; }
        public virtual string ProdName1 { get; set; }
        public virtual string ProdName2 { get; set; }
        public virtual string ProdName3 { get; set; }
        public virtual string ProdName4 { get; set; }
        public virtual string Description { get; set; }
        public virtual string Contribution { get; set; }
        public virtual string ProdUnit { get; set; }
        public virtual decimal ProdQuantity { get; set; }
        public virtual decimal ProdPrice { get; set; }
        public virtual decimal ProdTotal { get; set; }
        public virtual float ProdVatRate { get; set; }
        public virtual decimal ProdVatAmount { get; set; }
        public virtual decimal ProdAmount { get; set; }
        public virtual decimal TotalBeforeTax { get; set; }
        public virtual float VATRate { get; set; }
        public virtual decimal VATAmount { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Model { get; set; }
        public virtual string Color { get; set; }
        public virtual string Upholstery { get; set; }
        public virtual string ChassisNumber { get; set; }
        public virtual string EngineNumber { get; set; }
        public virtual string ProductionYear { get; set; }
        public virtual string DeliveryPlace { get; set; }
        public virtual string Inclusive { get; set; }
        public virtual string CarType { get; set; }
        public virtual string RegNo { get; set; }
        public virtual string KmNo { get; set; }
        public virtual string RegDate { get; set; }
        public virtual string ServiceDate { get; set; }
        public virtual string DealerCode { get; set; }
        public virtual decimal OptionCharge { get; set; }
        public virtual string RONo { get; set; }
        public virtual string RODate { get; set; }
        public virtual string ReceivedTime { get; set; }
        public virtual string ReceivedDate { get; set; }
        public virtual string ReceivedBy { get; set; }
        public virtual string BroughtBy { get; set; }
        public virtual string DeadlineTime { get; set; }
        public virtual string DeadlineDate { get; set; }
        public virtual decimal LabourCharge { get; set; }
        public virtual decimal Parts { get; set; }
        public virtual decimal Lubricants { get; set; }
        public virtual decimal Subcontract { get; set; }
        public virtual decimal Miscellaneous { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual string ApprovedBy { get; set; }
        public virtual DateTime? ApprovedDate { get; set; }
        public virtual string PublishBy { get; set; }
        public virtual DateTime? PublishDate { get; set; }
        public virtual string SLType { get; set; }
        public virtual string Notes { get; set; }
        public virtual string Reason { get; set; }
        public virtual string SrcModule { get; set; }
        public virtual string Module { get; set; }
        public virtual decimal ExceedInsurance { get; set; }

        public List<VNSUploadDataDetail> Details = new List<VNSUploadDataDetail>();
    }
}
