using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using WebServer;
using System.Dynamic;
using System.Threading;

namespace OWIN_SignalR.Controller
{
    public class SignalRController : ApiController
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");
        public static readonly log4net.ILog logerr = log4net.LogManager.GetLogger("logerr");
            
        static int process = 0;
        static bool syncFlag = false;
        static bool syncPartFlag = false;
        static int partprocess = 0;
        private int DownLoadUserInfoTask(object index)
        {
            int id = Convert.ToInt32(index);
            WebServer.WebApiApplication.users[id - 1].BtnDownloadUserInfo_Click();
            //System.Diagnostics.Debug.WriteLine("download user info successfull!");
            loginfo.InfoFormat("download user info successfull!");
            return 1;
        }
        async Task AsyncGetUserInfo(int index)
        {
            var task = Task<int>.Factory.StartNew(new Func<object, int>(DownLoadUserInfoTask), index);
            await task;
        }
        [HttpGet]
        public void GetUserInfo(string id)
        {
            loginfo.InfoFormat("GetUserInfo id={0}", id);
            if (id == null)
            {
                id = "2";
            }
            int index = int.Parse(id);
            if (index > WebServer.WebApiApplication.users.Length || index < 1)
            {
                //System.Diagnostics.Debug.WriteLine("has no machine number");
                logerr.Error ("has no machine number");
                return;
            }
            AsyncGetUserInfo(index);
        }
   
        private int BatchUpLoadUserInfoTask(object index)
        {
            int id = Convert.ToInt32(index);
            WebServer.WebApiApplication.users[id - 1].BtnBatchUpdate_Click("");
            //System.Diagnostics.Debug.WriteLine("batchUpLoad user info successfull!");
            loginfo.InfoFormat("batchUpLoad user info successfull!");
            return 1;
        }
        async Task AsyncBatchUserInfo(int index)
        {
            var task = Task<int>.Factory.StartNew(new Func<object, int>(BatchUpLoadUserInfoTask), index);
            await task;
        }
        [HttpPut]
        public void PutBatchUserInfo(string id)
        {
            loginfo.InfoFormat("PutBatchUserInfo id={0}", id);
            if (id == null)
            {
                id = "1";
            }
            int index = int.Parse(id);
            if (index > WebServer.WebApiApplication.users.Length || index < 1)
            {
                //System.Diagnostics.Debug.WriteLine("has no machine number");
                logerr.Error ("has no machine number");
                return;
            }
            AsyncBatchUserInfo(index);
        }

        [HttpGet]
        public HttpResponseMessage GetSyncStatus()
        {
            loginfo.InfoFormat("GetSyncStatus process={0}", process);
            return new HttpResponseMessage()
            {
                Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":{\"process\":"+ process +"}}", Encoding.UTF8, "application/json"),
            };
        }
        private int SyncUserInfoTask(object index)
        {
            int id = Convert.ToInt32(index);
            int total = WebServer.WebApiApplication.users.Length;
            int i = 0;
            try
            {
                WebServer.WebApiApplication.users[id - 1].BtnDownloadUserInfo_Click();
                process = 20;
                //System.Diagnostics.Debug.WriteLine("down load success");
                loginfo.InfoFormat("down load success");
                if (id == 1)
                {
                    for (i = 1; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process += 10;
                    }
                }
                else if (id == 9)
                {
                    for (i = 0; i < total - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process += 10;
                    }
                }
                else
                {
                    for (i = 0; i < id - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process += 10;
                    }
                    for (i = id; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process += 10;
                    }
                }
                //System.Diagnostics.Debug.WriteLine("upload success");
                loginfo.InfoFormat("upload success");
                process = 100;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error (e.Message);
            }
            syncFlag = false;
            return 1;
        }
        async Task AsyncSyncUserInfo(int index)
        {
            var task = Task<int>.Factory.StartNew(new Func<object, int>(SyncUserInfoTask), index);
            await task;
        }
        [HttpPost]
        public HttpResponseMessage Sync(dynamic obj)
        {
            loginfo.InfoFormat("Sync obj={0}", obj);
            if(syncFlag == true)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + "Synching" + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            process = 0;
            int id = 2; //default id is 2:前台
            try
            {
                id = Convert.ToInt32(obj.id);  //mathine id 1~9
                if (obj.id == null)
                {
                    id = 2;
                }
                if (id > WebServer.WebApiApplication.users.Length)
                {
                    //System.Diagnostics.Debug.WriteLine("has no machine number");
                    logerr.Error("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"has no such machine number\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            syncFlag = true;
            AsyncSyncUserInfo(id);
            return new HttpResponseMessage()
            {
                Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
            };
        }

        [HttpGet]
        public HttpResponseMessage GetSyncPartStatus()
        {
            loginfo.InfoFormat("GetSyncPartStatus partprocess={0}", partprocess);
            return new HttpResponseMessage()
            {
                Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":{\"process\":" + partprocess + "}}", Encoding.UTF8, "application/json"),
            };
        }
        private int SyncPartUserInfoTask(object index)
        {
            try { 
                string str = Convert.ToString(index);
                string[] sArray = str.Split(new char[2] { ':', ',' });
                int id = Convert.ToInt16(sArray[0]);
                string[] user_list = new string[sArray.Length-1];
                int i = 0;
                for (i = 0; i < user_list.Length; i++)
                {
                    user_list[i] = sArray[i+1];
                }
                WebServer.WebApiApplication.users[id - 1].BtnGetUserInfo_Click(user_list);
                //System.Diagnostics.Debug.WriteLine("download user success");
                loginfo.InfoFormat("download user success");
                partprocess = 10;
                Thread.Sleep(1000);
                int total = WebServer.WebApiApplication.users.Length;

                if (id == 1)
                {
                    for (i = 1; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess += 10;
                    }
                }
                else if (id == 9)
                {
                    for (i = 0; i < total - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess += 10;
                    }
                }
                else
                {
                    for (i = 0; i < id - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess += 10;
                    }
                    for (i = id; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess += 10;
                    }
                }   
                //System.Diagnostics.Debug.WriteLine("upload user success");
                loginfo.InfoFormat("upload user success");
                partprocess = 100;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
            }
            syncPartFlag = false;
            return 1;
        }
        
        async Task AsyncSyncPartUserInfo(int index, string user_id_list)
        {
            var task = Task<int>.Factory.StartNew(new Func<object, int>(SyncPartUserInfoTask), index.ToString() + ":" + user_id_list);
            await task;
        }
        [HttpPost]
        public HttpResponseMessage SyncPart(dynamic obj)
        {
            loginfo.InfoFormat("SyncPart obj={0}", obj);
            if (syncPartFlag == true)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + "Synching" + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            partprocess = 0;
            int id = 2; //default id is 2:前台
            string user_id_list = "";
            try
            {
                user_id_list = Convert.ToString(obj.user_id_list);
                id = Convert.ToInt32(obj.id);  //mathine id 1~9
                if (obj.user_id_list == null || obj.id == null)
                {
                    //System.Diagnostics.Debug.WriteLine("need user_id_list and id input parameter");
                    logerr.Error("need user_id_list and id input parameter");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"need user_id_list and id input parameter\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                if (id > WebServer.WebApiApplication.users.Length)
                {
                    //System.Diagnostics.Debug.WriteLine("has no machine number");
                    logerr.Error("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"has no such machine number\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            syncPartFlag = true;
            AsyncSyncPartUserInfo(id, user_id_list);
            return new HttpResponseMessage()
            {
                Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
            };
        }

        [HttpPost]
        public HttpResponseMessage AddUser(dynamic obj)
        {
            loginfo.InfoFormat("AddUser obj={0}", obj);
            try
            {
                string user_id = Convert.ToString(obj.user_id);
                string user_name = Convert.ToString(obj.user_name);
                string card_number = Convert.ToString(obj.card_number);
                if (obj.user_id == null || obj.user_name == null || obj.card_number == null)
                {
                    logerr.Error("need user_id, user_name, card_number input paremeter");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"need user_id, user_name, card_number input paremeter\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                for (int i = 0; i < WebServer.WebApiApplication.users.Length; i++)
                {
                    WebServer.WebApiApplication.users[i].BtnUploadUserInfo_Click(user_id, user_name, card_number);
                }
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
        }
        [HttpPost]
        public HttpResponseMessage UpdateUser(dynamic obj)
        {
            loginfo.InfoFormat("UpdateUser obj={0}", obj);
            try
            {
                string user_id = Convert.ToString(obj.user_id);
                string user_name = Convert.ToString(obj.user_name);
                string card_number = Convert.ToString(obj.card_number);
                if (obj.user_id == null || obj.user_name == null || obj.card_number == null)
                {
                    logerr.Error("need user_id, user_name, card_number input paremeter");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"need user_id, user_name, card_number input paremeter\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                for (int i = 0; i < WebServer.WebApiApplication.users.Length; i++)
                {
                    WebServer.WebApiApplication.users[i].BtnUpdateUserInfo_Click(user_id, user_name, card_number);
                }
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
        }
        [HttpDelete]
        public HttpResponseMessage DeleteUser(dynamic obj)
        {
            loginfo.InfoFormat("DeleteUser obj={0}", obj);
            try
            {
                int i = 0;
                string user_id = Convert.ToString(obj.user_id);
                if (obj.user_id == null)
                {
                    logerr.Error("no userid appoint");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"" + "no userid appoint" + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                for(i = 0; i < WebServer.WebApiApplication.users.Length; i++)
                {
                    WebServer.WebApiApplication.users[i].BtnDeleteEnrollData_Click(user_id);
                }
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }  
        }
        [HttpPost]
        public HttpResponseMessage PostUserState(dynamic obj)
        {
            loginfo.InfoFormat("PostUserState obj={0}", obj);
            int id = 2; //default id is 2:前台
            string user_id_list = "";
            try
            {
                user_id_list = Convert.ToString(obj.user_id_list);
                id = Convert.ToInt32(obj.id);  //mathine id 1~9
                if (obj.user_id_list == null || obj.id == null)
                {
                    //System.Diagnostics.Debug.WriteLine("need user_id_list and id input parameter");
                    logerr.Error("need user_id_list and id input parameter");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"need user_id_list and id input parameter\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                if (id > WebServer.WebApiApplication.users.Length)
                {
                    //System.Diagnostics.Debug.WriteLine("has no machine number");
                    logerr.Error("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"has no such machine number\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            string[] sArray = user_id_list.Split(',');
            string data = "";
            data = WebServer.WebApiApplication.users[id - 1].BtnGetUserInfo_Click(sArray);
            return new HttpResponseMessage()
            {
                Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":"+data+"}", Encoding.UTF8, "application/json"),
            };
        }
        [HttpPost]
        public HttpResponseMessage RestartDevice(string id)
        {
            loginfo.InfoFormat("RestartDevice obj={0}", id);
            try
            {
                if (id == null)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"" + "no machie id appoint" + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                int index = int.Parse(id);
                if (index > WebServer.WebApiApplication.users.Length || index < 1)
                {
                    logerr.Error("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"" + "has no such machine number" + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }            
                WebServer.WebApiApplication.users[index - 1].BtnRestartDevice_Click();
                loginfo.InfoFormat("restart "+ id + " machie successfull");
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
        }
        [HttpPost]
        public HttpResponseMessage RestartHZDevice()
        {
            loginfo.InfoFormat("RestartHZDevice ....");
            try
            {
                for (int i = 1; i < WebServer.WebApiApplication.users.Length; i++)
                {
                    WebServer.WebApiApplication.users[i].BtnRestartDevice_Click();
                    loginfo.InfoFormat("restart " + i + " machie successfull");
                }                
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":0,\"msg\":\"success\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.Message);
                logerr.Error(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
        }

    }
}
