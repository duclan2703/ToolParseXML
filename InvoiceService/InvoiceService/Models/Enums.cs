using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Models
{
    public enum VNSInvoiceType
    {
        [Description("Hóa đơn mới")]
        New,
        [Description("Hóa đơn hủy")]
        InvCancel,
        [Description("Hóa đơn thay thế")]
        InvReplace
    }

    public enum VNSUploadStatus
    {
        /// <summary>
        /// Mới tạo = 0
        /// </summary>
        [Description("Chờ duyệt")]
        CreateNew,
        /// <summary>
        /// Hủy bỏ hóa đơn upload = 1
        /// </summary>
        [Description("Từ chối")]
        Reject,
        /// <summary>
        /// Đã duyệt hóa đơn = 2
        /// </summary>
        [Description("Chờ phát hành")]
        Approved,
        /// <summary>
        /// Phát hành = 3
        /// </summary>
        [Description("Đã phát hành")]
        Published
    }
}
