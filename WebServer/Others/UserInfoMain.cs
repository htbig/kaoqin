/**********************************************************
 * Demo for Standalone SDK.Created by Darcy on Oct.15 2009*
***********************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Net;

using System.Threading;
using System.Timers;
using WebServer;
namespace UserInfo
{
    public partial class UserInfoMain
    {
        public UserInfoMain(int machieNumber)
        {
            iMachineNumber = machieNumber;
            //InitializeComponent();
        }
        public static string[] names = {"测试", "前台", "研发大办公室门口", "洗手间门口", "货梯门口", "采购", "新租办公区", "生产1", "生产2" };
        //Create Standalone SDK class dynamicly.
        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
        private string logPath = "D:\\kaoqin\\WebServer-api\\";/*"C:\\ustar\\WebServer-api\\";*/

        /*************************************************************************************************
        * Before you refer to this demo,we strongly suggest you read the development manual deeply first.*
        * This part is for demonstrating the communication with your device.                             *
        * ************************************************************************************************/
        #region Communication
        private bool bIsConnected = false;//the boolean value identifies whether the device is connected
        private int iMachineNumber;//the serial number of the device.After connecting the device ,this value will be changed.
        private Thread check_online;

        private void Tfn_check_online()
        {
            while (true)
            {
                lock (axCZKEM1)
                {
                    if ((false == axCZKEM1.EnableDevice(iMachineNumber, false)) || (bIsConnected == false))// disable the device,if return value is false,it means the machine has disconnectted,need reconnect 
                    {
                        axCZKEM1.Disconnect();
                        //axCZKEM1.OnAttTransactionEx -= new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(AxCZKEM1_OnAttTransactionEx);
                        bIsConnected = false;
                        BtnConnect_Click(WebServer.WebApiApplication.ips[iMachineNumber - 1]);
                    }

                }
                Thread.Sleep(30000);
            }
        }
      
        public void StartUpTickJob()
        {
            check_online = new Thread(Tfn_check_online);
            check_online.Start();
        }
        //If your device supports the TCP/IP communications, you can refer to this.
        //when you are using the tcp/ip communication,you can distinguish different devices by their IP address.
        public void BtnConnect_Click(string ip_addr/*object sender, EventArgs e*/)
        {            
            int idwErrorCode = 0;

            axCZKEM1.PullMode = 1;            
            bIsConnected = axCZKEM1.Connect_Net(ip_addr, 4370);
            if (bIsConnected == true)
            {              
                //iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
                if (axCZKEM1.RegEvent(iMachineNumber, 65535))
                {//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                    //this.axCZKEM1.OnFinger += new zkemkeeper._IZKEMEvents_OnFingerEventHandler(axCZKEM1_OnAttTransactionEx);
                    //axCZKEM1.OnVerify += new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                    //axCZKEM1.OnAttTransactionEx += new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(AxCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnFingerFeature += new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnEnrollFingerEx += new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnDeleteTemplate += new zkemkeeper._IZKEMEvents_OnDeleteTemplateEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnNewUser += new zkemkeeper._IZKEMEvents_OnNewUserEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnHIDNum += new zkemkeeper._IZKEMEvents_OnHIDNumEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnAlarm += new zkemkeeper._IZKEMEvents_OnAlarmEventHandler(axCZKEM1_OnAlarm);
                    //this.axCZKEM1.OnDoor += new zkemkeeper._IZKEMEvents_OnDoorEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnWriteCard += new zkemkeeper._IZKEMEvents_OnWriteCardEventHandler(axCZKEM1_OnAttTransactionEx);
                    //this.axCZKEM1.OnEmptyCard += new zkemkeeper._IZKEMEvents_OnEmptyCardEventHandler(axCZKEM1_OnAttTransactionEx);
                    //axCZKEM1.OnDisConnected += new zkemkeeper._IZKEMEvents_OnDisConnectedEventHandler(axCZKEM1_OnDisConnected);
                }
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
            }
        }

        #endregion

        /*************************************************************************************************
        * Before you refer to this demo,we strongly suggest you read the development manual deeply first.*
        * This part is for demonstrating operations with user(download/upload/delete/clear/modify).      *
        * ************************************************************************************************/
        #region UserInfo

        //Download user's 9.0 or 10.0 arithmetic fingerprint templates(in strings)
        //Only TFT screen devices with firmware version Ver 6.60 version later support function "GetUserTmpExStr" and "GetUserTmpEx".
        //'While you are using 9.0 fingerprint arithmetic and your device's firmware version is under ver6.60,you should use the functions "SSR_GetUserTmp" or 
        //"SSR_GetUserTmpStr" instead of "GetUserTmpExStr" or "GetUserTmpEx" in order to download the fingerprint templates.
        public void BtnDownloadUserInfo_Click()
        {
            if (bIsConnected == false)
            {
                System.Console.Write("Please connect the device first!", "Error");
                return;
            }

            string sdwEnrollNumber = "";
            string sName = "";
            string sPassword = "";
            int iPrivilege = 0;
            bool bEnabled = false;

            int idwFingerIndex;
            string sTmpData = "";
            int iTmpLength = 0;
            int iFlag = 0;
            int iFaceIndex = 50;// 'the only possible parameter value
            int iFaceLength = 128 * 1024; //initialize the length(cannot be zero)
            byte [] sTmpFaceData = new byte[iFaceLength];
            string sCardnumber = "";
            string S = "";
            string S_tmp = "";//for figer templates
            S = "工号," + "姓名," + "指纹索引," + "指纹序列," + "等级," + "密码," + "使能," + "标记," + "人脸索引," + "人脸序列," + "人脸字节数," + "卡号";
            FileStream fs = new FileStream(logPath +"userInfo.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(S);
            bool bHasFg = false;
            bool bHasFg_tmp = false;
            bool bHasFc = false;
            axCZKEM1.EnableDevice(iMachineNumber, false);
            axCZKEM1.ReadAllUserID(iMachineNumber);//read all the user information to the memory
            axCZKEM1.ReadAllTemplate(iMachineNumber);//read all the users' fingerprint templates to the memory
            while (axCZKEM1.SSR_GetAllUserInfo(iMachineNumber, out sdwEnrollNumber, out sName, out sPassword, out iPrivilege, out bEnabled))//get all the users' information from the memory
            {
                for (idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
                {
                    if (axCZKEM1.GetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, out iFlag, out sTmpData, out iTmpLength))//get the corresponding templates string and length from the memory
                    {
                        if (iFlag == 0)
                            continue;
                        if (bHasFg == true)
                        {
                            bHasFg_tmp = true;
                            S_tmp = sdwEnrollNumber + "," + sName + "," + idwFingerIndex + "," + sTmpData + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                        }
                        else
                        {
                            bHasFg = true;
                            S = sdwEnrollNumber + "," + sName + "," + idwFingerIndex + "," + sTmpData + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                        } 
                    }
                }
                Array.Clear(sTmpFaceData, 0, sTmpFaceData.Length);
                iFaceLength = 128 * 1024;//can't be zero
                if (axCZKEM1.GetUserFace(iMachineNumber, sdwEnrollNumber, iFaceIndex, ref sTmpFaceData[0], ref iFaceLength))
                {// 'get the face templates from the memory
                    if (bHasFg == false)
                    {
                        S = sdwEnrollNumber + "," + sName + "," + "" + "," + "" + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                    }
                    if(bHasFg_tmp == true)
                    {
                        S_tmp = S_tmp + "," + iFaceIndex + "," + Convert.ToBase64String(sTmpFaceData) + "," + iFaceLength;
                    }
                    S = S + "," + iFaceIndex + "," + Convert.ToBase64String(sTmpFaceData) + "," + iFaceLength;
                    //S = S + "," + iFaceIndex + "," + sTmpFaceData + "," + iFaceLength;
                    bHasFc = true;
                }
                if (axCZKEM1.GetStrCardNumber(out sCardnumber)) {// 'get the card number from the memory
                    if (bHasFg == false && bHasFc == false) {
                        S = sdwEnrollNumber + "," + sName + "," + "" + "," + "" + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag + "," + "" + "," + "" + "," + "";
                    } else if (bHasFc == false) {
                        if (bHasFg_tmp == true)
                        {
                            S_tmp = S_tmp + "," + "" + "," + "" + "," + "";
                        }
                        S = S + "," + "" + "," + "" + "," + "";
                    }
                    long lNumber = Convert.ToInt64 (sCardnumber);
                    string sHexNumner = Convert.ToString(lNumber, 16);
                    if (bHasFg_tmp == true)
                    {
                        S_tmp = S_tmp + "," + sHexNumner;
                    }
                    S = S + "," + sHexNumner;
                 }
                sw.WriteLine(S);
                if (bHasFg_tmp == true)
                {
                    sw.WriteLine(S_tmp);
                }
                bHasFg_tmp = false;
                bHasFg = false;
                bHasFc = false;
            }
            sw.Close();
            fs.Close();
            axCZKEM1.EnableDevice(iMachineNumber, true);
        }

        //Upload the 9.0 or 10.0 fingerprint arithmetic templates to the device(in strings) in batches.
        //Only TFT screen devices with firmware version Ver 6.60 version later support function "SetUserTmpExStr" and "SetUserTmpEx".
        //While you are using 9.0 fingerprint arithmetic and your device's firmware version is under ver6.60,you should use the functions "SSR_SetUserTmp" or 
        //"SSR_SetUserTmpStr" instead of "SetUserTmpExStr" or "SetUserTmpEx" in order to upload the fingerprint templates.
        public void BtnBatchUpdate_Click(string filename)
        {
            if (bIsConnected == false)
            {
                System.Console.Write("Please connect the device first!", "Error");
                return;
            }
            int idwErrorCode = 0;
            string sdwEnrollNumber = "";
            string sName = "";
            int idwFingerIndex = 0;
            string sTmpData = "";
            int iPrivilege = 0;
            string sPassword = "";
            string sEnabled = "";
            bool bEnabled = true;
            int iFlag = 1;
            bool bHasFace = false;
            int iFaceIndex = 0;
            string sTmpFaceData = "";
            int iTmpLength = 0;
            string[] sArray;
            StreamReader objReader;
            if (filename == "")
            {
                objReader = new StreamReader(logPath + "userInfo.csv");
            }
            else
            {
                objReader = new StreamReader(logPath + filename );
            }
            string sLine = "";
            sLine = objReader.ReadLine();
            sLine = objReader.ReadLine();
            int iUpdateFlag = 1;
            axCZKEM1.EnableDevice(iMachineNumber, false);
            if (axCZKEM1.BeginBatchUpdate(iMachineNumber, iUpdateFlag))//create memory space for batching data
            {
                string sLastEnrollNumber = "";//the former enrollnumber you have upload(define original value as 0)
                while (sLine != null)
                {
                    sArray = sLine.Split(',');
                    if (sArray.Length != 12)
                    {
                        System.Diagnostics.Debug.WriteLine("csv file not right");
                        return;
                    }
                    sdwEnrollNumber = sArray[0];
                    sName = sArray[1];
                    idwFingerIndex = int.Parse(sArray[2] == "" ? "0" : sArray[2]);
                    sTmpData = sArray[3];
                    iPrivilege = int.Parse(sArray[4]);
                    sPassword = sArray[5];
                    sEnabled = sArray[6];
                    iFlag = int.Parse(sArray[7]);
                    if (sArray[8] == "50")
                    {
                        bHasFace = true;
                        iFaceIndex = int.Parse(sArray[8]);
                        sTmpFaceData = sArray[9];
                        iTmpLength = int.Parse(sArray[10]);
                    }
                    //sCardnumber = long.Parse(sArray[11]);
                    if (sdwEnrollNumber != sLastEnrollNumber)
                    {
                        string hexString = sArray[11];
                        int num = Int32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
                        axCZKEM1.SetStrCardNumber(num.ToString()); //Before you using function SetUserInfo,set the card number to make sure you can upload it to the device
                        if (axCZKEM1.SSR_SetUserInfo(iMachineNumber, sdwEnrollNumber, sName, sPassword, iPrivilege, bEnabled))
                        {//upload user information to the device
                            axCZKEM1.SetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, iFlag, sTmpData);// upload templates information to the device
                            if (bHasFace == true)
                            {
                                byte[] decBytes = Convert.FromBase64String(sTmpFaceData);
                                axCZKEM1.SetUserFace(iMachineNumber, sdwEnrollNumber, iFaceIndex, ref decBytes[0], iTmpLength);//upload face templates information to the device
                                bHasFace = false;
                            }
                        }
                        else
                        {
                            axCZKEM1.GetLastError(ref idwErrorCode);
                            System.Diagnostics.Debug.WriteLine("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
                            axCZKEM1.EnableDevice(iMachineNumber, true);
                            return;
                        }
                    }
                    else//the current fingerprint and the former one belongs the same user,that is ,one user has more than one template
                    {
                        axCZKEM1.SetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, iFlag, sTmpData);
                    }
                    sLastEnrollNumber = sdwEnrollNumber;//change the value of iLastEnrollNumber dynamicly
                    sLine = objReader.ReadLine();
                }
            }
            objReader.Close();
            axCZKEM1.BatchUpdate(iMachineNumber);//upload all the information in the memory
            axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
            axCZKEM1.EnableDevice(iMachineNumber, true);
            System.Diagnostics.Debug.WriteLine("Successfully upload fingerprint templates in batches , " + "total:" + "Success");
        }

        //Upload the 9.0 or 10.0 fingerprint arithmetic templates one by one(in strings)
        //Only TFT screen devices with firmware version Ver 6.60 version later support function "SetUserTmpExStr" and "SetUserTmpEx".
        //While you are using 9.0 fingerprint arithmetic and your device's firmware version is under ver6.60,you should use the functions "SSR_SetUserTmp" or 
        //"SSR_SetUserTmpStr" instead of "SetUserTmpExStr" or "SetUserTmpEx" in order to upload the fingerprint templates.
        public void BtnUploadUserInfo_Click(string user_id, string user_name, string card_number)
        {
            if (bIsConnected == false)
            {
                System.Console.Write("Please connect the device first!"+iMachineNumber.ToString(), "Error");
                return;
            }
            int idwErrorCode = 0;
            string sdwEnrollNumber = user_id;
            string sName = user_name;    
            int iPrivilege = 0;
            string sPassword = "";
            bool bEnabled = true;
            
            axCZKEM1.EnableDevice(iMachineNumber, false);    
            string hexString = card_number;
            int num = Int32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            axCZKEM1.SetStrCardNumber(num.ToString()); //Before you using function SetUserInfo,set the card number to make sure you can upload it to the device
            if (axCZKEM1.SSR_SetUserInfo(iMachineNumber, sdwEnrollNumber, sName, sPassword, iPrivilege, bEnabled)) {//upload user information to the device
                System.Diagnostics.Debug.WriteLine("Successfully Upload user info!!!");
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                System.Diagnostics.Debug.WriteLine("set userinfo failed!!!");
                axCZKEM1.EnableDevice(iMachineNumber, true);
                return;
            }
            axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
            axCZKEM1.EnableDevice(iMachineNumber, true);
        }
        //Delete a certain user's fingerprint template of specified index
        //You shuold input the the user id and the fingerprint index you will delete
        //The difference between the two functions "SSR_DelUserTmpExt" and "SSR_DelUserTmp" is that the former supports 24 bits' user id.
        //private void btnSSR_DelUserTmpExt_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }

        //    if (cbUserIDTmp.Text.Trim() == "" || cbFingerIndex.Text.Trim() == "")
        //    {
        //        MessageBox.Show("Please input the UserID and FingerIndex first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    string sUserID = cbUserIDTmp.Text.Trim();
        //    int iFingerIndex = Convert.ToInt32(cbFingerIndex.Text.Trim());

        //    Cursor = Cursors.WaitCursor;
        //    if (axCZKEM1.SSR_DelUserTmpExt(iMachineNumber, sUserID, iFingerIndex))
        //    {
        //        axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
        //        MessageBox.Show("SSR_DelUserTmpExt,UserID:" + sUserID + " FingerIndex:" + iFingerIndex.ToString(), "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }
        //    Cursor = Cursors.Default;
        //}

        //Clear all the fingerprint templates in the device(While the parameter DataFlag  of the Function "ClearData" is 2 )
        //private void btnClearDataTmps_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    int iDataFlag = 2;

        //    Cursor = Cursors.WaitCursor;
        //    if (axCZKEM1.ClearData(iMachineNumber, iDataFlag))
        //    {
        //        axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
        //        MessageBox.Show("Clear all the fingerprint templates!", "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }
        //    Cursor = Cursors.Default;
        //}

        //Delete all the user information in the device,while the related fingerprint templates will be deleted either. 
        //(While the parameter DataFlag  of the Function "ClearData" is 5 )
        //private void btnClearDataUserInfo_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    int iDataFlag = 5;

        //    Cursor = Cursors.WaitCursor;
        //    if (axCZKEM1.ClearData(iMachineNumber, iDataFlag))
        //    {
        //        axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
        //        MessageBox.Show("Clear all the UserInfo data!", "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }
        //    Cursor = Cursors.Default;
        //}

        //Delete a kind of data that some user has enrolled
        //The range of the Backup Number is from 0 to 9 and the specific meaning of Backup number is described in the development manual,pls refer to it.
        public void BtnDeleteEnrollData_Click(string userId)
        {
            if (bIsConnected == false)
            {
                System.Console.Write("Please connect the device first!", "Error");
                return;
            }
            int idwErrorCode = 0;
            axCZKEM1.EnableDevice(iMachineNumber, false);
            if (axCZKEM1.SSR_DeleteEnrollData(iMachineNumber, userId, 12)) //12 means delete user info
            {
                axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
                System.Diagnostics.Debug.WriteLine("Successfully delete user info");
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                System.Diagnostics.Debug.WriteLine("Operation failed,ErrorCode=" + idwErrorCode.ToString());
            }
            axCZKEM1.EnableDevice(iMachineNumber, true);
        }

        //Clear all the administrator privilege(not clear the administrators themselves)
        //private void btnClearAdministrators_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    Cursor = Cursors.WaitCursor;
        //    if (axCZKEM1.ClearAdministrators(iMachineNumber))
        //    {
        //        axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
        //        MessageBox.Show("Successfully clear administrator privilege from teiminal!", "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }
        //    Cursor = Cursors.Default;
        //}

        //Download users' face templates(in strings)(For TFT screen IFace series devices only)
        //private void btnDownLoadFace_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }

        //    string sUserID = "";
        //    string sName = "";
        //    string sPassword = "";
        //    int iPrivilege = 0;
        //    bool bEnabled = false;
        //    int iFaceIndex = 50;//the only possible parameter value
        //    string sTmpData = "";
        //    int iLength = 0;

        //    lvFace.Items.Clear();
        //    lvFace.BeginUpdate();

        //    Cursor = Cursors.WaitCursor;
        //    axCZKEM1.EnableDevice(iMachineNumber, false);
        //    axCZKEM1.ReadAllUserID(iMachineNumber);//read all the user information to the memory

        //    while (axCZKEM1.SSR_GetAllUserInfo(iMachineNumber, out sUserID, out sName, out sPassword, out iPrivilege, out bEnabled))//get all the users' information from the memory
        //    {
        //        if (axCZKEM1.GetUserFaceStr(iMachineNumber, sUserID, iFaceIndex, ref sTmpData, ref iLength))//get the face templates from the memory
        //        {
        //            ListViewItem list = new ListViewItem();
        //            list.Text = sUserID;
        //            list.SubItems.Add(sName);
        //            list.SubItems.Add(sPassword);
        //            list.SubItems.Add(iPrivilege.ToString());
        //            list.SubItems.Add(iFaceIndex.ToString());
        //            list.SubItems.Add(sTmpData);
        //            list.SubItems.Add(iLength.ToString());
        //            if (bEnabled == true)
        //            {
        //                list.SubItems.Add("true");
        //            }
        //            else
        //            {
        //                list.SubItems.Add("false");
        //            }
        //            lvFace.Items.Add(list);
        //        }
        //    }
        //    axCZKEM1.EnableDevice(iMachineNumber, true);
        //    lvFace.EndUpdate();
        //    Cursor = Cursors.Default;
        //}

        //Upload users' face template(in strings)(For TFT screen IFace series devices only)
        //Uploading the face templates in batches is not supported temporarily.
        //private void btnUploadFace_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    string sUserID = "";
        //    string sName = "";
        //    int iFaceIndex = 0;
        //    string sTmpData = "";
        //    int iLength = 0;
        //    int iPrivilege = 0;
        //    string sPassword = "";
        //    string sEnabled = "";
        //    bool bEnabled = false;

        //    Cursor = Cursors.WaitCursor;
        //    axCZKEM1.EnableDevice(iMachineNumber, false);
        //    for (int i = 0; i < lvFace.Items.Count; i++)
        //    {
        //        sUserID = lvFace.Items[i].SubItems[0].Text;
        //        sName = lvFace.Items[i].SubItems[1].Text;
        //        sPassword = lvFace.Items[i].SubItems[2].Text;
        //        iPrivilege = Convert.ToInt32(lvFace.Items[i].SubItems[3].Text);
        //        iFaceIndex = Convert.ToInt32(lvFace.Items[i].SubItems[4].Text);
        //        sTmpData = lvFace.Items[i].SubItems[5].Text;
        //        iLength = Convert.ToInt32(lvFace.Items[i].SubItems[6].Text);
        //        sEnabled = lvFace.Items[i].SubItems[7].Text;
        //        if (sEnabled == "true")
        //        {
        //            bEnabled = true;
        //        }
        //        else
        //        {
        //            bEnabled = false;
        //        }

        //        if (axCZKEM1.SSR_SetUserInfo(iMachineNumber, sUserID, sName, sPassword, iPrivilege, bEnabled))//face templates are part of users' information
        //        {
        //            axCZKEM1.SetUserFaceStr(iMachineNumber, sUserID, iFaceIndex, sTmpData, iLength);//upload face templates information to the device
        //        }
        //        else
        //        {
        //            axCZKEM1.GetLastError(ref idwErrorCode);
        //            MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //            Cursor = Cursors.Default;
        //            axCZKEM1.EnableDevice(iMachineNumber, true);
        //            return;
        //        }
        //    }

        //    axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
        //    Cursor = Cursors.Default;
        //    axCZKEM1.EnableDevice(iMachineNumber, true);
        //    MessageBox.Show("Successfully Upload the face templates, " + "total:" + lvFace.Items.Count.ToString(), "Success");
        //}


        //Delete a certain user's face template according to its id
        //private void btnDelUserFace_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }

        //    if (cbUserID3.Text.Trim() == "")
        //    {
        //        MessageBox.Show("Please input the UserID first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    string sUserID = cbUserID3.Text.Trim();
        //    int iFaceIndex = 50;

        //    Cursor = Cursors.WaitCursor;
        //    if (axCZKEM1.DelUserFace(iMachineNumber, sUserID, iFaceIndex))
        //    {
        //        axCZKEM1.RefreshData(iMachineNumber);
        //        MessageBox.Show("DelUserFace,UserID=" + sUserID, "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }
        //    Cursor = Cursors.Default;

        //}

        //Download specified user's face template (in bytes array)    
        //You can refer to the part of "Udisk data Management" to learn how to manage the user's binary template(Get or Set)
        //private void btnGetUserFace_Click(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        MessageBox.Show("Please connect the device first!", "Error");
        //        return;
        //    }

        //    if (cbUserID3.Text.Trim() == "")
        //    {
        //        MessageBox.Show("Please input the UserID first!", "Error");
        //        return;
        //    }
        //    int idwErrorCode = 0;

        //    string sUserID = cbUserID3.Text.Trim();
        //    int iFaceIndex = 50;//the only possible parameter value
        //    int iLength = 128 * 1024;//initialize the length(cannot be zero)
        //    byte[] byTmpData = new byte[iLength];

        //    Cursor = Cursors.WaitCursor;
        //    axCZKEM1.EnableDevice(iMachineNumber, false);

        //    if (axCZKEM1.GetUserFace(iMachineNumber, sUserID, iFaceIndex, ref byTmpData[0], ref iLength))
        //    {
        //        //Here you can manage the information of the face templates according to your request.(for example,you can sava them to the database)
        //        MessageBox.Show("GetUserFace,the  length of the bytes array byTmpData is " + iLength.ToString(), "Success");
        //    }
        //    else
        //    {
        //        axCZKEM1.GetLastError(ref idwErrorCode);
        //        MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
        //    }

        //    axCZKEM1.EnableDevice(iMachineNumber, true);
        //    Cursor = Cursors.Default;
        //}
        //Download specified user's info
        public string BtnGetUserInfo_Click(string[] users_id)
        {
            if (bIsConnected == false)
            {
                System.Console.Write("Please connect the device first!", "Error");
                return "[]";
            }
            string data = "[";
            int idwErrorCode = 0;
            int iFaceIndex = 50;//the only possible parameter value
            int iFaceLength = 128 * 1024; //initialize the length(cannot be zero)
            byte[] sTmpFaceData = new byte[iFaceLength];
            
            string S = "";
            string S_tmp = "";//for figer templates
            S = "工号," + "姓名," + "指纹索引," + "指纹序列," + "等级," + "密码," + "使能," + "标记," + "人脸索引," + "人脸序列," + "人脸字节数," + "卡号";
            FileStream fs = new FileStream(logPath + "partuser.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(S);
            bool bHasFg = false;
            bool bHasFg_tmp = false;
            bool bHasFc = false;
            int idwFingerIndex;
            
            int i = 0;
            bool ret = false;
            //axCZKEM1.EnableDevice(iMachineNumber, false);
            for (i=0; i< users_id.Length; i++)
            {
                string sdwEnrollNumber = "";
                string sTmpData = "";
                int iTmpLength = 0;
                int iFlag = 0; 
                Array.Clear(sTmpFaceData, 0, sTmpFaceData.Length);
                sdwEnrollNumber = users_id[i];
                
                ret = axCZKEM1.SSR_GetUserInfo(iMachineNumber, sdwEnrollNumber, out string sName, out string sPassword, out int iPrivilege, out bool bEnabled);
                if (ret == false)
                {
                    data += "{},";
                    continue;
                } 
                axCZKEM1.GetStrCardNumber(out string sCardnumber);
                data += "{\"UserId\":\"" + sdwEnrollNumber + "\",\"Name\":\"" + sName + "\",\"Cardnumber\":" + sCardnumber;
                for (idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
                {
                    if (axCZKEM1.GetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, out iFlag, out sTmpData, out iTmpLength))//get the corresponding templates string and length from the memory
                    {
                        if (iFlag == 0)
                            continue;                      
                        if (bHasFg == true)
                        {
                            data += ",\"FingerIndex2\":" + idwFingerIndex.ToString();
                            bHasFg_tmp = true;
                            S_tmp = sdwEnrollNumber + "," + sName + "," + idwFingerIndex + "," + sTmpData + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                        }
                        else
                        {
                            data += ",\"FingerIndex1\":" + idwFingerIndex.ToString();
                            bHasFg = true;
                            S = sdwEnrollNumber + "," + sName + "," + idwFingerIndex + "," + sTmpData + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                        }
                    }
                }
                iFaceLength = 128 * 1024;
                if (axCZKEM1.GetUserFace(iMachineNumber, sdwEnrollNumber, iFaceIndex, ref sTmpFaceData[0], ref iFaceLength))
                //if (axCZKEM1.GetUserFaceStr(iMachineNumber, sdwEnrollNumber, iFaceIndex, ref sFaceData, ref iFaceLength))
                {
                    data += ",\"HasFace\":true";
                    if (bHasFg == false)
                    {
                        S = sdwEnrollNumber + "," + sName + "," + "" + "," + "" + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag;
                    }
                    if (bHasFg_tmp == true)
                    {
                        S_tmp = S_tmp + "," + iFaceIndex + "," + Convert.ToBase64String(sTmpFaceData) + "," + iFaceLength;
                        //S_tmp = S_tmp + "," + iFaceIndex + "," + sFaceData + "," + iFaceLength;
                    }
                    S = S + "," + iFaceIndex + "," + Convert.ToBase64String(sTmpFaceData) + "," + iFaceLength;
                    //S = S + "," + iFaceIndex + "," + sFaceData + "," + iFaceLength;
                    bHasFc = true;
                }
                else
                {
                    axCZKEM1.GetLastError( idwErrorCode);
                }
                //if (axCZKEM1.GetStrCardNumber(out sCardnumber))
                {// 'get the card number from the memory
                    if (bHasFg == false && bHasFc == false)
                    {
                        S = sdwEnrollNumber + "," + sName + "," + "" + "," + "" + "," + iPrivilege + "," + sPassword + "," + bEnabled + "," + iFlag + "," + "" + "," + "" + "," + "";
                    }
                    else if (bHasFc == false)
                    {
                        if (bHasFg_tmp == true)
                        {
                            S_tmp = S_tmp + "," + "" + "," + "" + "," + "";
                        }
                        S = S + "," + "" + "," + "" + "," + "";
                    }
                    long lNumber = Convert.ToInt64(sCardnumber);
                    string sHexNumner = Convert.ToString(lNumber, 16);
                    if (bHasFg_tmp == true)
                    {
                        S_tmp = S_tmp + "," + sHexNumner;
                    }
                    S = S + "," + sHexNumner;
                }
                data += "},";
                sw.WriteLine(S);
                if (bHasFg_tmp == true)
                {
                    sw.WriteLine(S_tmp);
                }
                bHasFg_tmp = false;
                bHasFg = false;
                bHasFc = false;
            }
            //axCZKEM1.EnableDevice(iMachineNumber, true);
            if (data.Length > 1)
            {
                data = data.Substring(0, (data.Length - 1));
            }
            data += "]";
            sw.Close();
            fs.Close();
            return data;
            
        }
        //add by Darcy on Nov.23 2009
        //Add the existed userid to DropDownLists.
        //bool bAddControl = true;
        //private void UserIDTimer_Tick(object sender, EventArgs e)
        //{
        //    if (bIsConnected == false)
        //    {
        //        //cbUserIDDE.Items.Clear();
        //        //cbUserIDTmp.Items.Clear();
        //        bAddControl = true;
        //        return;
        //    }
        //    else
        //    {
        //        if (bAddControl == true)
        //        {
        //            string sEnrollNumber = "";
        //            string sName = "";
        //            string sPassword = "";
        //            int iPrivilege = 0;
        //            bool bEnabled = false;

        //            axCZKEM1.EnableDevice(iMachineNumber, false);
        //            axCZKEM1.ReadAllUserID(iMachineNumber);//read all the user information to the memory
        //            while (axCZKEM1.SSR_GetAllUserInfo(iMachineNumber, out sEnrollNumber, out sName, out sPassword, out iPrivilege, out bEnabled))
        //            {
        //                //cbUserIDDE.Items.Add(sEnrollNumber);
        //                //cbUserIDTmp.Items.Add(sEnrollNumber);
        //            }
        //            bAddControl = false;
        //            axCZKEM1.EnableDevice(iMachineNumber, true);
        //        }
        //        return;
        //    }
        //}
        #endregion
    }
}