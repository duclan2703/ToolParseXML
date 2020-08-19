using InvoiceService.Models;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace InvoiceService
{
    public class VNSService
    {
        static ILog log = LogManager.GetLogger(typeof(VNSService));
        public void Processing()
        {
            string folderPath = ConfigurationManager.AppSettings["FolderXML"];
            List<VNSUploadData> lstUploadData = new List<VNSUploadData>();
            var vnsmappingFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\Config\\VNSMapping.json";
            var vnsmapping_text = File.ReadAllText(vnsmappingFile);
            List<VNSMapping> Lstvnsmap = JsonConvert.DeserializeObject<List<VNSMapping>>(vnsmapping_text);

            var imsMapping = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\Config\\SerialMapping.json";
            var imsMapping_text = File.ReadAllText(imsMapping);
            List<SerialMapping> lstserialMapping = JsonConvert.DeserializeObject<List<SerialMapping>>(imsMapping_text);

            #region Đọc File từ folder
            string[] extensions = { ".xml", ".XML" };
            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.*").Where(s => extensions.Any(ext => ext == Path.GetExtension(s))))
            {
                string fileName = Path.GetFileName(filePath);
                string comtaxcode = string.Empty;
                try
                {
                    string mesError = "";
                    var uploadData = VNSParseService.ParseIInvoice(filePath, Lstvnsmap, lstserialMapping, ref mesError, out comtaxcode);
                    if (uploadData != null)
                        lstUploadData.Add(uploadData);
                    else
                    {
                        log.Error("Parse File: " + fileName + " - Lỗi: " + mesError);
                        string path = Path.Combine(folderPath, "Error");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        var movePath = Path.Combine(path, fileName);
                        if (File.Exists(movePath))
                            movePath = Path.Combine(path, fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(filePath));
                        if (File.Exists(filePath))
                            File.Move(filePath, movePath);

                        //Gửi mail báo lỗi
                        if (ConfigurationManager.AppSettings["SendMail"].ToString() == "1")
                        {
                            string subject = "Lỗi đọc file hóa đơn điện tử, tên file: " + fileName;
                            string body = "Đã xảy ra lỗi khi đọc file " + fileName + ": " + mesError;
                            SendFailedMail(subject, body);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Parse file: " + fileName + " - Lỗi: " + ex.Message);
                    if (!string.IsNullOrEmpty(comtaxcode))
                        MoveFile(folderPath, filePath, comtaxcode, false);
                    else
                    {
                        string path = Path.Combine(folderPath, "Error");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        var movePath = Path.Combine(path, fileName);
                        if (File.Exists(movePath))
                            movePath = Path.Combine(path, fileName + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(filePath));
                        if (File.Exists(filePath))
                            File.Move(filePath, movePath);
                    }
                    //Gửi mail báo lỗi
                    if (ConfigurationManager.AppSettings["SendMail"].ToString() == "1")
                    {
                        string subject = "Lỗi đọc file hóa đơn điện tử, tên file: " + fileName;
                        string body = "Đã xảy ra lỗi khi đọc file " + fileName + ": " + ex.Message;
                        SendFailedMail(subject, body);
                    }
                }
            }
            #endregion

            #region Gom dữ liệu theo mst
            var groupbyMST = lstUploadData.GroupBy(x => x.ComTaxCode).ToList();
            foreach (var gr in groupbyMST)
            {
                string comtaxcode = gr.Key;
                var LstdataByMST = gr.ToList();
                if (LstdataByMST.Count() > 0)
                {
                    if (LstdataByMST.Count() <= 50)
                    {
                        ProcessResult(LstdataByMST, comtaxcode, folderPath);
                    }
                    else
                    {
                        for (int i = 0; i < LstdataByMST.Count() / 50; i++)
                        {
                            var lst50 = LstdataByMST.Skip(i * 50).Take(50).ToList();
                            ProcessResult(lst50, comtaxcode, folderPath);
                        }
                    }
                }
            }


            #endregion
        }

        private void ProcessResult(List<VNSUploadData> lstData, string comtaxcode, string folderPath)
        {
            ApiResult result = new ApiResult();
            result = SaveXMLData(lstData, comtaxcode);
            if (result != null)
            {
                if (result.status == "OK")
                {
                    var resultConvert = ConvertResultAPI(result.data);
                    RemoveFile("Success", folderPath, comtaxcode, result.data);
                    log.Error("Upload SUCCESS list invoice: " + resultConvert);
                }
                else
                {
                    log.Error("Upload file hóa đơn lỗi: " + result.status + "-" + result.messages);
                    var groupDatas = result.data.GroupBy(x => x.isSuccess).ToList();
                    var lstSuccess = groupDatas.Where(x => x.Key == true).FirstOrDefault();
                    var lstFailse = groupDatas.Where(x => x.Key == false).FirstOrDefault();
                    if (lstSuccess != null && lstSuccess.Count() > 0)
                    {
                        var resultConvert = ConvertResultAPI(lstSuccess.ToList());
                        RemoveFile("Success", folderPath, comtaxcode, (IList<InvResult>)lstSuccess);
                        log.Error("Upload SUCCESS list invoice: " + resultConvert);
                    }
                    if (lstFailse != null && lstFailse.Count() > 0)
                    {
                        var resultConvert = ConvertResultAPI(lstFailse.ToList());
                        RemoveFile("Error", folderPath, comtaxcode, (IList<InvResult>)lstFailse);
                        log.Error("Upload ERROR list invoice: " + resultConvert);

                        //Gửi mail báo lỗi
                        if (ConfigurationManager.AppSettings["SendMail"].ToString() == "1")
                        {
                            string subject = "Lỗi lưu danh sách file hóa đơn điện tử";
                            string body = "Đã xảy ra lỗi khi lưu danh sách file hóa đơn.<br> Nội dung: " + result.messages;
                            SendFailedMail(subject, body);
                        }
                    }
                }
            }
            else
                log.Error("Kết nối đến api không thành công.");
        }

        private string ConvertResultAPI(IList<InvResult> data)
        {
            string result = "";
            List<string> strConvert = new List<string>();
            foreach (var item in data)
            {
                string x = string.Format("{0}|{1}|{2}", item.FileName, item.Serial, item.InvNo);
                strConvert.Add(x);
            }
            result = string.Join(",", strConvert);
            return result;
        }

        private void RemoveFile(string folderName, string folderPath, string comtaxcode, IList<InvResult> results)
        {
            string path = Path.Combine(folderPath, comtaxcode, folderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var item in results)
            {
                var filePath = Path.Combine(folderPath, item.FileName);
                var movePath = Path.Combine(path, item.FileName);
                if (File.Exists(movePath))
                    movePath = Path.Combine(path, Path.GetFileNameWithoutExtension(filePath) + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(filePath));
                File.Move(filePath, movePath);
            }
        }

        public void MoveFile(string folderPath, string filePath, string comtaxcode, bool isSuccess)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string sourcePath = "";
            if (isSuccess == false)
                sourcePath = Path.Combine(folderPath, comtaxcode, "Error");
            else
                sourcePath = Path.Combine(folderPath, comtaxcode, "Success");
            if (Directory.Exists(sourcePath))
            {
                var movePath = Path.Combine(sourcePath, fileName + Path.GetExtension(filePath));
                if (File.Exists(movePath))
                    movePath = Path.Combine(sourcePath, Path.GetFileNameWithoutExtension(filePath) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(filePath));
                if (File.Exists(filePath))
                    File.Move(filePath, movePath);
            }
            else
            {
                Directory.CreateDirectory(sourcePath);
                if (File.Exists(filePath))
                    File.Move(filePath, Path.Combine(sourcePath, fileName + Path.GetExtension(filePath)));
            }
        }

        public static ApiResult SaveXMLData(List<VNSUploadData> LstInv, string comtaxcode)
        {
            var response = CallApi("api/XMLInvoice/SaveXMLData", JsonConvert.SerializeObject(LstInv), comtaxcode);
            if (response == null)
                return null;
            var result = JsonConvert.DeserializeObject<ApiResult>(response.Content);
            return result;
        }

        static IRestResponse CallApi(string action, string data, string comtaxcode)
        {
            string url = ConfigurationManager.AppSettings["Url"].ToString();
            string username = ConfigurationManager.AppSettings["Username"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();

            var client = new RestClient(url);
            var request = new RestRequest(action);
            request.Method = Method.POST;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("userName", username);
            request.AddHeader("password", password);
            request.AddHeader("taxCode", comtaxcode);
            request.AddParameter("application/json", data, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                log.Error("Call API lỗi: " + response.StatusCode + "-" + response.Content);
                return null;
            }
            return response;
        }

        static IRestResponse SendFile(string action, byte[] data, string filepath, out string errMes)
        {
            errMes = "";
            string url = ConfigurationManager.AppSettings["Url"].ToString();
            string username = ConfigurationManager.AppSettings["Username"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            string taxcode = ConfigurationManager.AppSettings["Taxcode"].ToString();

            var client = new RestClient(url);
            var request = new RestRequest(action);
            request.Method = Method.POST;
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddHeader("userName", username);
            request.AddHeader("password", password);
            request.AddHeader("taxCode", taxcode);
            request.AddFile("Reconcile", data, Path.GetFileName(filepath));
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                errMes = response.StatusCode + "-" + response.Content;
                return null;
            }
            return response;
        }

        private static void SendFailedMail(string subject, string body)
        {
            try
            {
                //thông tin mail
                var mailFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\Config\\mailinfo.json";
                var mailData = File.ReadAllText(mailFile);
                var mailInfo = JsonConvert.DeserializeObject<MailInfo>(mailData);

                var lstTo = mailInfo.To.Split(';').ToList();
                var lstCCMail = !string.IsNullOrWhiteSpace(mailInfo.CCMail) ? mailInfo.CCMail.Split(';').ToList() : null;
                MailMessage message = new MailMessage(mailInfo.From, lstTo[0], subject, body);
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                for (int i = 1; i < lstTo.Count; i++)
                {
                    message.To.Add(lstTo[i]);
                }
                if (lstCCMail != null)
                {
                    for (int i = 0; i < lstCCMail.Count; i++)
                    {
                        message.CC.Add(lstCCMail[i]);
                    }
                }
                SmtpClient client = new SmtpClient(mailInfo.Host, int.Parse(mailInfo.Port));
                client.Credentials = new NetworkCredential(mailInfo.Username, mailInfo.Password);
                bool ssl = true;
                client.EnableSsl = bool.TryParse(mailInfo.EnableSsl, out ssl) ? ssl : true;
                client.Send(message);
                log.Error("Send Email success");
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi mail lỗi: " + ex);
            }

        }
    }
}
