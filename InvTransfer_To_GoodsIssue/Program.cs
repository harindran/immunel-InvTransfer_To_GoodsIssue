using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvTransfer_To_GoodsIssue
{
    static class Program
    {       
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run();
            clsGoodsIssue GoodsIssue = new clsGoodsIssue();
            GoodsIssue.CompanyConnection();
           


        }

       




    }
}
