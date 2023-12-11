using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvTransfer_To_GoodsIssue
{
    class clsGoodsIssue
    {
       public SAPbobsCOM.Company objcompany;
        clsGlobalVariables globalVariables;
        clsGlobalMethods globalMethods;
        SAPbobsCOM.Recordset Recordset, objRs;
        string strQuery;

        public clsGoodsIssue()
        {
            globalVariables = new clsGlobalVariables();
            globalMethods = new clsGlobalMethods();
        }

        public void CompanyConnection()
        {
            var lretcode = 0;            
            globalVariables.HANA =Convert.ToBoolean(Getvalue_webconfig("HANA"));
            globalMethods.WriteErrorLog("Company Connecting...");
            objcompany = new SAPbobsCOM.Company();
            objcompany.Server = Getvalue_webconfig("SAPServername");
            objcompany.SLDServer = Getvalue_webconfig("SLDSERVER");
            objcompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
            objcompany.DbUserName = Getvalue_webconfig("SQLUserName");
            objcompany.DbPassword = Getvalue_webconfig("SQLPassword");
            objcompany.LicenseServer = Getvalue_webconfig("SAPLicenseName");
            objcompany.CompanyDB = Getvalue_webconfig("database");
            objcompany.UserName = Getvalue_webconfig("SAPUsername");
            objcompany.Password = Getvalue_webconfig("SAPPassword");
            objcompany.UseTrusted = false;

            lretcode = objcompany.Connect();
            if (lretcode != 0)
            {
                globalMethods.WriteErrorLog("Company Connection Failed: " + objcompany.GetLastErrorDescription().ToString());
                System.Windows.Forms.MessageBox.Show(objcompany.GetLastErrorDescription().ToString());
            }            
            else
            {
                globalMethods.WriteErrorLog("Company Connected Successfully!!!..." + objcompany.CompanyDB.ToString());
                clsTable table = new clsTable(objcompany, globalVariables);
                if(Getvalue_webconfig("FieldCreation").ToUpper() == "YES")   table.FieldCreation();
                Get_InventoryTransfer(Getvalue_webconfig("TranTriggerFrom"));
               if(objcompany.Connected==true) objcompany.Disconnect();
                globalMethods.WriteErrorLog("Company Disconnected...");

            }                

        }

        public string Getvalue_webconfig(string key)
        {
            try
            {
                string strConnectionString = System.Configuration.ConfigurationManager.AppSettings[key];
                return strConnectionString;
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.ToString());
                return "";
            }
        }

        public string getSingleValue(string StrSQL)
        {
            try
            {
                SAPbobsCOM.Recordset rset = (SAPbobsCOM.Recordset)objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                //string strReturnVal = "";
                rset.DoQuery(StrSQL);
                return Convert.ToString((rset.RecordCount) > 0 ? rset.Fields.Item(0).Value.ToString() : "");
            }
            catch (Exception ex)
            {
                //clsModule.objaddon.objapplication.StatusBar.SetText(" Get Single Value Function Failed :  " + ex.Message + StrSQL, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return "";
            }
        }

        private bool InvTransfer_To_GoodsIssue_Logs(string InvDocEntry, string InvDocNum, string ObjType, string GIEntry, string TranFlag, int ErrID, string ErrDesc)
        {
            try
            {
                bool Flag = false;
                string DocEntry;
                SAPbobsCOM.GeneralService oGeneralService;
                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralDataParams oGeneralParams;
                Recordset = (SAPbobsCOM.Recordset)objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oGeneralService =objcompany.GetCompanyService().GetGeneralService("ATITGI");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);
                oGeneralParams = (SAPbobsCOM.GeneralDataParams)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams);
                try
                {
                    if (globalVariables.HANA == true)
                        DocEntry = getSingleValue("Select \"DocEntry\" from \"@ATPL_ITGI\" where \"U_BaseEntry\"=" + InvDocEntry + " Order by \"DocEntry\" Desc");
                    else
                        DocEntry = getSingleValue("Select DocEntry from [@ATPL_ITGI] where U_BaseEntry=" + InvDocEntry + " Order by DocEntry Desc");
                    oGeneralParams.SetProperty("DocEntry", DocEntry);
                    oGeneralData = oGeneralService.GetByParams(oGeneralParams);
                    Flag = true;
                }
                catch (Exception ex)
                {
                    Flag = false;
                }               

                if (Flag == false)
                {
                    oGeneralData.SetProperty("U_GenDate", DateTime.Now.Date);
                    oGeneralData.SetProperty("U_BaseNo", InvDocNum);
                    oGeneralData.SetProperty("U_BaseEntry", InvDocEntry);
                    oGeneralData.SetProperty("U_DocObjType", ObjType);
                }
                oGeneralData.SetProperty("U_GIEntry", GIEntry);
                oGeneralData.SetProperty("U_ErrDesc", ErrDesc);
                oGeneralData.SetProperty("U_ErrId", Convert.ToString(ErrID));
                oGeneralData.SetProperty("U_Flag", TranFlag);
                oGeneralData.SetProperty("U_Status", (TranFlag == "Y") ? "Success" :  "Failure");

                if (Flag == true)
                {
                    oGeneralService.Update(oGeneralData);
                    return true;
                }
                else
                {
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    return true;
                }

            }

            catch (Exception ex)
            {
                globalMethods.WriteErrorLog("InvTransfer_To_GoodsIssue_Logs: " + ex.Message.ToString());
                //clsModule.objaddon.objapplication.StatusBar.SetText("InvTransfer_To_GoodsIssue_Logs: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private void Get_InventoryTransfer(string FromDate)
        {
            try
            {
                SAPbobsCOM.Recordset oRecordset;
                //string[] ToWhse=new[] { ""};
                //ToWhse = Getvalue_webconfig("ToWarehouse").Split(',');
                strQuery = String.Join(",", Getvalue_webconfig("ToWarehouse").Split(','));
                if (globalVariables.HANA==true)
                    strQuery = "Select distinct T1.\"DocEntry\",T1.\"DocNum\" from WTR1 T0 Left Join OWTR T1 On T0.\"DocEntry\"=T1.\"DocEntry\" Where T1.\"CANCELED\"='N' and T1.\"DocDate\">='"+ FromDate +"' and T0.\"WhsCode\" in (" + strQuery + ") and T0.\"Quantity\">0 and T1.\"U_AT_GIEntry\" is null";
                else
                    strQuery = "Select distinct T0.DocEntry,T1.DocNum from WTR1 T0 Left Join OWTR T1 On T0.DocEntry=T1.DocEntry Where T1.CANCELED='N' and T1.DocDate>='" + FromDate + "' and T0.WhsCode in (" + strQuery + ") and T0.Quantity>0 and T1.U_AT_GIEntry is null";


                oRecordset = (SAPbobsCOM.Recordset) objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(strQuery);
                if (oRecordset.RecordCount == 0) return;

                for (int i = 0; i < oRecordset.RecordCount; i++)
                {
                    Create_GoodsIssue(Convert.ToString(oRecordset.Fields.Item("DocEntry").Value), Convert.ToString(oRecordset.Fields.Item("DocNum").Value));
                    oRecordset.MoveNext();
                }
            }
            catch (Exception ex)
            {
                globalMethods.WriteErrorLog("Get_InventoryTransfer: " + ex.Message.ToString());
            }
        }

        private bool Create_GoodsIssue(string InvDocEntry,string InvDocNum)
        {
            SAPbobsCOM.Documents oGoodsIssue=null;
            try
            {                
                string DocEntry;
                if (globalVariables.HANA == true)
                {
                    strQuery = "Select T0.\"DocEntry\",T1.\"DocNum\",T0.\"LineNum\",T0.\"ItemCode\",T0.\"Dscription\",T0.\"Quantity\",T0.\"WhsCode\",T1.\"DocDate\",T1.\"TaxDate\",T0.\"OcrCode\",T0.\"OcrCode2\",";
                    strQuery += "\n T0.\"OcrCode3\",T0.\"OcrCode4\",T0.\"OcrCode5\",T0.\"Project\",T1.\"BPLId\",T1.\"BPLName\",(Select \"U_ExpAcc\" from OITM Where \"ItemCode\"=T0.\"ItemCode\") \"AcctCode\",";
                    strQuery += "\n (Select \"ManBtchNum\" from OITM Where \"ItemCode\"=T0.\"ItemCode\") \"Batch\",(Select \"ManSerNum\" from OITM Where \"ItemCode\"=T0.\"ItemCode\") \"Serial\"";
                    strQuery += "\n from WTR1 T0 Left Join OWTR T1 On T0.\"DocEntry\"=T1.\"DocEntry\" Where T1.\"CANCELED\"='N' and T0.\"DocEntry\"='"+ InvDocEntry + "' Order by T0.\"LineNum\"";
                }
                else
                {
                    strQuery = "Select T0.DocEntry,T1.DocNum,T0.LineNum,T0.ItemCode,T0.Dscription,T0.Quantity,T0.WhsCode,T1.DocDate,T1.TaxDate,T0.OcrCode,T0.OcrCode2,";
                    strQuery += "\n T0.OcrCode3,T0.OcrCode4,T0.OcrCode5,T0.Project,T1.BPLId,T1.BPLName,(Select U_ExpAcc from OITM Where ItemCode=T0.ItemCode) AcctCode,";
                    strQuery += "\n (Select ManBtchNum from OITM Where ItemCode=T0.ItemCode) Batch,(Select ManSerNum from OITM Where ItemCode=T0.ItemCode) Serial";
                    strQuery += "\n from WTR1 T0 Left Join OWTR T1 On T0.DocEntry=T1.DocEntry Where T1.CANCELED='N' and T0.DocEntry='" + InvDocEntry + "' Order by T0.LineNum";
                }

                Recordset = (SAPbobsCOM.Recordset)objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                Recordset.DoQuery(strQuery);
                oGoodsIssue =(SAPbobsCOM.Documents) objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
                globalMethods.WriteErrorLog("Goods Issue Creating Please wait.." + "Inventory Transfer DocNum: " + InvDocNum +" DocEntry: " + InvDocEntry);
                oGoodsIssue.DocDate = Convert.ToDateTime(Recordset.Fields.Item("DocDate").Value);
                oGoodsIssue.TaxDate = Convert.ToDateTime(Recordset.Fields.Item("TaxDate").Value);
                oGoodsIssue.UserFields.Fields.Item("U_AT_InvEntry").Value = InvDocEntry;

                if (Convert.ToString(Recordset.Fields.Item("BPLId").Value)!=null && Convert.ToString(Recordset.Fields.Item("BPLId").Value) != "0")
                {
                    oGoodsIssue.BPL_IDAssignedToInvoice = Convert.ToInt32(Recordset.Fields.Item("BPLId").Value);
                    //Branch Series
                    string kk = Convert.ToString(Recordset.Fields.Item("BPLId").Value);
                    if (globalVariables.HANA == true)
                    {
                        strQuery = getSingleValue("select \"Series\" From NNM1 where \"ObjectCode\"='60' and \"Indicator\"=(select Top 1 \"Indicator\"  from OFPR where '" + Convert.ToDateTime(Recordset.Fields.Item("DocDate").Value).ToString("yyyyMMdd") + "' between \"F_RefDate\" and \"T_RefDate\") " + " and \"BPLId\"='" + Convert.ToString(Recordset.Fields.Item("BPLId").Value) + "' ");
                    }
                    else
                    {
                        strQuery = getSingleValue("select Series From NNM1 where ObjectCode='60' and Indicator=(select Top 1 Indicator  from OFPR where '" + Convert.ToDateTime(Recordset.Fields.Item("DocDate").Value).ToString("yyyyMMdd") + "' between F_RefDate and T_RefDate) " + " and BPLId='" + Convert.ToString(Recordset.Fields.Item("BPLId").Value) + "' ");
                    }
                    if(strQuery!="") oGoodsIssue.Series = Convert.ToInt32(strQuery);
                }
                
                for (int i = 0; i < Recordset.RecordCount; i++)
                {
                    if (Convert.ToString(Recordset.Fields.Item("AcctCode").Value) == "")
                    {
                        globalMethods.WriteErrorLog("Account Code is not defined for the Item: " + Convert.ToString(Recordset.Fields.Item("ItemCode").Value));
                        InvTransfer_To_GoodsIssue_Logs(InvDocEntry, InvDocNum, "67", "0", "N", -1, "Account Code is not defined for the Item: " + Convert.ToString(Recordset.Fields.Item("ItemCode").Value));
                        return false;
                    }                    
                    oGoodsIssue.Lines.ItemCode = Convert.ToString(Recordset.Fields.Item("ItemCode").Value);
                    oGoodsIssue.Lines.Quantity = Convert.ToDouble(Recordset.Fields.Item("Quantity").Value);
                    oGoodsIssue.Lines.WarehouseCode = Convert.ToString(Recordset.Fields.Item("WhsCode").Value);
                    oGoodsIssue.Lines.AccountCode = Convert.ToString(Recordset.Fields.Item("AcctCode").Value);
                    oGoodsIssue.Lines.ProjectCode= Convert.ToString(Recordset.Fields.Item("Project").Value);

                    if (Convert.ToString(Recordset.Fields.Item("OcrCode").Value) != "")  oGoodsIssue.Lines.CostingCode = Convert.ToString(Recordset.Fields.Item("OcrCode").Value);
                    if (Convert.ToString(Recordset.Fields.Item("OcrCode2").Value) != "") oGoodsIssue.Lines.CostingCode2 = Convert.ToString(Recordset.Fields.Item("OcrCode2").Value);
                    if (Convert.ToString(Recordset.Fields.Item("OcrCode3").Value) != "") oGoodsIssue.Lines.CostingCode3 = Convert.ToString(Recordset.Fields.Item("OcrCode3").Value);
                    if (Convert.ToString(Recordset.Fields.Item("OcrCode4").Value) != "") oGoodsIssue.Lines.CostingCode4 = Convert.ToString(Recordset.Fields.Item("OcrCode4").Value);
                    if (Convert.ToString(Recordset.Fields.Item("OcrCode5").Value) != "") oGoodsIssue.Lines.CostingCode5 = Convert.ToString(Recordset.Fields.Item("OcrCode5").Value);

                    if (Convert.ToString(Recordset.Fields.Item("Batch").Value) == "Y")
                    {
                        if (globalVariables.HANA == true)
                        {
                            strQuery = "SELECT A.\"BatchNum\" as \"BatchSerial\", SUM(A.\"Quantity\") as \"Qty\" FROM (";
                            strQuery += "\n select T.\"BatchNum\" , T.\"Quantity\" from ibt1 T inner join oibt T1 on T.\"ItemCode\"=T1.\"ItemCode\" and T.\"BatchNum\"=T1.\"BatchNum\" and T.\"WhsCode\"=T1.\"WhsCode\"";
                            strQuery += "\n inner join wtr1 T2 on T2.\"DocEntry\"=T.\"BaseEntry\" and T2.\"ItemCode\"=T.\"ItemCode\" and T2.\"LineNum\"=T.\"BaseLinNum\" inner join owtr T3 on T2.\"DocEntry\"=T3.\"DocEntry\"";
                            strQuery += "\n where T.\"BaseType\"='67' and T.\"Direction\"=0 and T.\"ItemCode\"='"+ Convert.ToString(Recordset.Fields.Item("ItemCode").Value) + "' and T.\"BaseEntry\"= '"+ InvDocEntry +"'";
                            strQuery += "\n )A GROUP BY A.\"BatchNum\" having SUM(A.\"Quantity\") >0";
                        }
                        else
                        {
                            strQuery = "SELECT A.BatchNum as BatchSerial, SUM(A.Quantity) as Qty FROM (";
                            strQuery += "\n select T.BatchNum , T.Quantity from ibt1 T inner join oibt T1 on T.ItemCode=T1.ItemCode and T.BatchNum=T1.BatchNum and T.WhsCode=T1.WhsCode";
                            strQuery += "\n inner join wtr1 T2 on T2.DocEntry=T.BaseEntry and T2.ItemCode=T.ItemCode and T2.LineNum=T.BaseLinNum inner join owtr T3 on T2.DocEntry=T3.DocEntry";
                            strQuery += "\n where T.BaseType='67' and T.Direction=0 and T.ItemCode='" + Convert.ToString(Recordset.Fields.Item("ItemCode").Value) + "' and T.BaseEntry= '" + InvDocEntry + "'";
                            strQuery += "\n )A GROUP BY A.BatchNum having SUM(A.Quantity) >0";
                        }
                        objRs = (SAPbobsCOM.Recordset)objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        objRs.DoQuery(strQuery);
                        if(objRs.RecordCount>0)
                        {
                            for (int Rec = 0; Rec < objRs.RecordCount; Rec++)
                            {
                                oGoodsIssue.Lines.BatchNumbers.BatchNumber = Convert.ToString(objRs.Fields.Item("BatchSerial").Value);
                                oGoodsIssue.Lines.BatchNumbers.Quantity = Convert.ToDouble(objRs.Fields.Item("Qty").Value);
                                oGoodsIssue.Lines.BatchNumbers.Add();
                                objRs.MoveNext();
                            }
                        }                       
                    }

                    if (Convert.ToString(Recordset.Fields.Item("Serial").Value) == "Y")
                    {
                        if (globalVariables.HANA == true)
                        {
                            strQuery = "Select * from (SELECT T4.\"IntrSerial\" \"BatchSerial\"";
                            strQuery += "\n from OWTR T0 inner join WTR1 T1 on T0.\"DocEntry\"=T1.\"DocEntry\"";
                            strQuery += "\n left outer join SRI1 I1 on T1.\"ItemCode\"=I1.\"ItemCode\" and (T1.\"DocEntry\"=I1.\"BaseEntry\" and T1.\"ObjType\"=I1.\"BaseType\") and T1.\"LineNum\"=I1.\"BaseLinNum\"";
                            strQuery += "\n left outer join OSRI T4 on T4.\"ItemCode\"=I1.\"ItemCode\" and I1.\"SysSerial\"=T4.\"SysSerial\" and I1.\"WhsCode\" = T4.\"WhsCode\" ";
                            strQuery += "\n Where T1.\"DocEntry\" ='"+ InvDocEntry + "'  and T1.\"ItemCode\"='" + Convert.ToString(Recordset.Fields.Item("ItemCode").Value) + "' and T4.\"Status\"=0";
                            strQuery += "\n ) A Where A.\"BatchSerial\" <>''";
                        }
                        else
                        {
                            strQuery = "Select * from (SELECT T4.IntrSerial BatchSerial";
                            strQuery += "\n from OWTR T0 inner join WTR1 T1 on T0.DocEntry=T1.DocEntry";
                            strQuery += "\n left outer join SRI1 I1 on T1.ItemCode=I1.ItemCode and (T1.DocEntry=I1.BaseEntry and T1.ObjType=I1.BaseType) and T1.LineNum=I1.BaseLinNum";
                            strQuery += "\n left outer join OSRI T4 on T4.ItemCode=I1.ItemCode and I1.SysSerial=T4.SysSerial and I1.WhsCode = T4.WhsCode ";
                            strQuery += "\n Where T1.DocEntry ='" + InvDocEntry + "'  and T1.ItemCode='" + Convert.ToString(Recordset.Fields.Item("ItemCode").Value) + "' and T4.Status=0";
                            strQuery += "\n ) A Where A.BatchSerial <>''";
                        }
                        objRs = (SAPbobsCOM.Recordset)objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        objRs.DoQuery(strQuery);
                        if (objRs.RecordCount > 0)
                        {
                            for (int Rec = 0; Rec < objRs.RecordCount; Rec++)
                            {
                                oGoodsIssue.Lines.SerialNumbers.InternalSerialNumber = Convert.ToString(objRs.Fields.Item("BatchSerial").Value);
                                oGoodsIssue.Lines.SerialNumbers.Quantity = 1;
                                oGoodsIssue.Lines.SerialNumbers.Add();
                                objRs.MoveNext();
                            }                                
                        }                            
                    }
                    oGoodsIssue.Lines.Add();
                    Recordset.MoveNext();
                }

               int Retval = oGoodsIssue.Add();

                if(Retval!=0)
                {
                    objcompany.GetLastError(out Retval,out strQuery);
                    globalMethods.WriteErrorLog("Goods Issue Error Code: " + Retval + " Error Desc: " + strQuery); //objcompany.GetLastErrorDescription().ToString()
                    InvTransfer_To_GoodsIssue_Logs(InvDocEntry,InvDocNum,"67","0","N",Retval,strQuery);
                    return false;
                }
                else
                {
                    DocEntry = objcompany.GetNewObjectKey();                    
                    globalMethods.WriteErrorLog("Goods Issue DocEntry: " + DocEntry + " Created Successfully..." + " Inventory Transfer DocNum: " + InvDocNum +" DocEntry: " + InvDocEntry);
                    if (globalVariables.HANA == true)
                        strQuery = "Update OWTR Set \"U_AT_GIEntry\"='" + DocEntry + "' Where \"DocEntry\"='" + InvDocEntry + "'";
                    else
                        strQuery = "Update OWTR Set U_AT_GIEntry='" + DocEntry + "' Where DocEntry='" + InvDocEntry + "'";
                    Recordset.DoQuery(strQuery);
                    InvTransfer_To_GoodsIssue_Logs(InvDocEntry, InvDocNum, "67", DocEntry, "Y", 0, "");
                    return true;
                }                             
                
            }
            catch (Exception ex)
            {
                globalMethods.WriteErrorLog("Create_GoodsIssue: " + ex.Message.ToString());
                return false;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oGoodsIssue);
                GC.Collect();
            }
        }

    }
}
