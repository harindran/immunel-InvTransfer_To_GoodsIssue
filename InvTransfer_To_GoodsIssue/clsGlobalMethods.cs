using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvTransfer_To_GoodsIssue
{
    class clsGlobalMethods
    {

        public clsGlobalMethods()
        {
        }
        

        public void WriteErrorLog(string Str)
        {
            string Foldername, Attachpath;
            Attachpath = @"C:\ProgramData\Altrocks Tech\InvTransfer_To_GoodsIssue\Logs\";
            Foldername = Attachpath;

            if (!Directory.Exists(Foldername))
            {
                Directory.CreateDirectory(Foldername);
            }
            FileStream fs;
            string chatlog = Foldername + DateTime.Now.ToString("yyyy-MM-dd") + "_" + Environment.UserName + ".txt";
            if (File.Exists(chatlog))
            {
            }
            else
            {
                fs = new FileStream(chatlog, FileMode.Create, FileAccess.Write);
                fs.Close();
            }
            string sdate;
            sdate = Convert.ToString(DateTime.Now);
            if (File.Exists(chatlog) == true)
            {
                var objWriter = new StreamWriter(chatlog, true);
                objWriter.WriteLine(sdate + " : " + Str);
                objWriter.Close();
            }
            else
            {
                var objWriter = new StreamWriter(chatlog, false);
            }
        }
    }
}
