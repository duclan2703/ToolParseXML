using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Models
{
    public class VNSUploadDataDetail
    {
        public virtual Guid id { get; set; }
        public virtual int ComID { get; set; }
        public virtual string InvPattern { get; set; }
        public virtual string InvSeries { get; set; }
        public virtual string InvID { get; set; }
        public virtual string Name { get; set; }
        public virtual string Name2 { get; set; }
        public virtual string Name3 { get; set; }
        public virtual string Name4 { get; set; }
        public virtual Decimal Price { get; set; }
        public virtual Decimal Quantity { get; set; }
        public virtual string Unit { get; set; }
        public virtual float VATRate { get; set; }
        public virtual Decimal Discount { get; set; }
        public virtual Decimal VATAmount { get; set; }
        public virtual Decimal DiscountAmount { get; set; }
        public virtual Decimal Amount { get; set; }
        public virtual string Code { get; set; }
        public virtual Decimal Total { get; set; }
        public virtual int Position { get; set; }
        public virtual string Description { get; set; }
        public virtual string Contribution { get; set; }
    }
}
