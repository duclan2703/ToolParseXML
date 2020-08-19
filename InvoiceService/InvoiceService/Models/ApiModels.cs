using System.Collections.Generic;

namespace InvoiceService.Models
{
    public class ApiResult
    {
        public string status { get; set; }
        public string messages { get; set; }
        public IList<InvResult> data { get; set; } = new List<InvResult>();
    }

    public class InvResult
    {
        public bool isSuccess { get; set; }
        public string FileName { get; set; }
        public string Pattern { get; set; }
        public string Serial { get; set; }
        public string InvNo { get; set; }
        public string Fkey { get; set; }
    }
}
