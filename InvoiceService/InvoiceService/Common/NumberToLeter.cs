using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Common
{
    public class NumberToLeter
    {
        static string[] ChuSo = new string[10] { " không", " một", " hai", " ba", " bốn", " năm", " sáu", " bảy", " tám", " chín" };
        static string[] Tien = new string[6] { "", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ" };
        static Dictionary<string, string> CurrencyType = new Dictionary<string, string> { { "VND", "đồng" }, { "USD", "dollars,cents" } };

        public static string ReadMoney(decimal sotien, string curency = "VND")
        {
            if (sotien < 0) return "Số tiền âm";
            string[] donvi = CurrencyType.First(p => p.Key.ToUpper() == curency.ToUpper()).Value.Split(',');
            if (sotien == 0) return "Không " + donvi[0];
            string sotienstr = "";
            if (curency == "VND")
                sotienstr = string.Format("{0:0,0}", sotien);
            else
                sotienstr = string.Format("{0:0,0.####}", sotien);
            var srtSoTien = sotienstr.Split('.');
            var result = "";
            for (int i = 0; i < srtSoTien.Count(); i++)
            {
                result += string.IsNullOrEmpty(result) ? (DocTienBangChu(Convert.ToDecimal(srtSoTien[i]), donvi[i]) + (srtSoTien.Count() > 1 ? ", " : "")) : DocTienBangChu(Convert.ToDecimal(srtSoTien[i]), donvi[i]);
            }
            return result.Substring(0, 1).ToUpper() + result.Substring(1);
        }
        // Hàm đọc số thành chữ
        public static string DocTienBangChu(decimal SoTien, string strTail = "đồng")
        {
            int lan, i;
            decimal so;
            string KetQua = "", tmp = "";
            int[] ViTri = new int[6];
            if (SoTien > 0)
            {
                so = SoTien;
            }
            else
            {
                so = -SoTien;
            }
            //Kiểm tra số quá lớn
            if (SoTien > 8999999999999999)
            {
                SoTien = 0;
                return "";
            }
            ViTri[5] = (int)(so / 1000000000000000);
            so = so - long.Parse(ViTri[5].ToString()) * 1000000000000000;
            ViTri[4] = (int)(so / 1000000000000);
            so = so - long.Parse(ViTri[4].ToString()) * +1000000000000;
            ViTri[3] = (int)(so / 1000000000);
            so = so - long.Parse(ViTri[3].ToString()) * 1000000000;
            ViTri[2] = (int)(so / 1000000);
            ViTri[1] = (int)((so % 1000000) / 1000);
            ViTri[0] = (int)(so % 1000);
            if (ViTri[5] > 0)
            {
                lan = 5;
            }
            else if (ViTri[4] > 0)
            {
                lan = 4;
            }
            else if (ViTri[3] > 0)
            {
                lan = 3;
            }
            else if (ViTri[2] > 0)
            {
                lan = 2;
            }
            else if (ViTri[1] > 0)
            {
                lan = 1;
            }
            else
            {
                lan = 0;
            }
            for (i = lan; i >= 0; i--)
            {
                tmp = DocSo3ChuSo(ViTri[i], i == lan);
                KetQua += tmp;
                if (ViTri[i] != 0) KetQua += Tien[i];
            }
            KetQua = KetQua.Trim() + " " + strTail;
            return KetQua;
        }
        // Hàm đọc số có 3 chữ số
        static string DocSo3ChuSo(int baso, bool max)
        {
            int tram, chuc, donvi;
            string KetQua = "";
            tram = (int)(baso / 100);
            chuc = (int)((baso % 100) / 10);
            donvi = baso % 10;
            if ((tram == 0) && (chuc == 0) && (donvi == 0)) return "";
            if (tram == 0 && !max)
            {
                KetQua += ChuSo[tram] + " trăm";
                if ((chuc == 0) && (donvi != 0)) KetQua += " lẻ";
            }
            if (tram > 0)
            {
                KetQua += ChuSo[tram] + " trăm";
                if ((chuc == 0) && (donvi != 0)) KetQua += " lẻ";
            }
            if ((chuc != 0) && (chuc != 1))
            {
                KetQua += ChuSo[chuc] + " mươi";
                if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " lẻ";
            }
            if (chuc == 1) KetQua += " mười";
            switch (donvi)
            {
                case 1:
                    if ((chuc != 0) && (chuc != 1))
                    {
                        KetQua += " mốt";
                    }
                    else
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
                case 5:
                    if (chuc == 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    else
                    {
                        KetQua += " lăm";
                    }
                    break;
                default:
                    if (donvi != 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
            }
            return KetQua;
        }
    }
}
