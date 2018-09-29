﻿using System;
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
        static int process = 0;
        static bool syncFlag = false;
        static bool syncPartFlag = false;
        static int partprocess = 0;
        private int DownLoadUserInfoTask(object index)
        {
            int id = Convert.ToInt32(index);
            WebServer.WebApiApplication.users[id - 1].BtnDownloadUserInfo_Click();
            System.Diagnostics.Debug.WriteLine("download user info successfull!");
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
            if (id == null)
            {
                id = "2";
            }
            int index = int.Parse(id);
            if (index > WebServer.WebApiApplication.users.Length || index < 1)
            {
                System.Diagnostics.Debug.WriteLine("has no machine number");
                return;
            }
            AsyncGetUserInfo(index);
        }
   
        private int BatchUpLoadUserInfoTask(object index)
        {
            int id = Convert.ToInt32(index);
            WebServer.WebApiApplication.users[id - 1].BtnBatchUpdate_Click("");
            System.Diagnostics.Debug.WriteLine("batchUpLoad user info successfull!");
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
            if (id == null)
            {
                id = "1";
            }
            int index = int.Parse(id);
            if (index > WebServer.WebApiApplication.users.Length || index < 1)
            {
                System.Diagnostics.Debug.WriteLine("has no machine number");
                return;
            }
            AsyncBatchUserInfo(index);
        }

        [HttpGet]
        public HttpResponseMessage GetSyncStatus()
        {
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
                System.Diagnostics.Debug.WriteLine("down load success");
                if (id == 1)
                {
                    for (i = 1; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process = +10;
                    }
                }
                else if (id == 9)
                {
                    for (i = 0; i < total - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process = +10;
                    }
                }
                else
                {
                    for (i = 0; i < id - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process = +10;
                    }
                    for (i = id; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("");
                        process = +10;
                    }
                }
                System.Diagnostics.Debug.WriteLine("upload success");
                process = 100;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                    System.Diagnostics.Debug.WriteLine("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"has no such machine number\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                
                WebServer.WebApiApplication.users[id - 1].BtnGetUserInfo_Click(sArray);
                System.Diagnostics.Debug.WriteLine("download user success");
                Thread.Sleep(1000);
                int total = WebServer.WebApiApplication.users.Length;
                int i = 0;
                if (id == 1)
                {
                    for (i = 1; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess = +10;
                    }
                }
                else if (id == 9)
                {
                    for (i = 0; i < total - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess = +10;
                    }
                }
                else
                {
                    for (i = 0; i < id - 1; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess = +10;
                    }
                    for (i = id; i < total; i++)
                    {
                        WebServer.WebApiApplication.users[i].BtnBatchUpdate_Click("partuser.csv");
                        partprocess = +10;
                    }
                }   
                System.Diagnostics.Debug.WriteLine("upload user success");
                partprocess = 100;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                    System.Diagnostics.Debug.WriteLine("need user_id_list and id input parameter");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"need user_id_list and id input parameter\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
                if (id > WebServer.WebApiApplication.users.Length)
                {
                    System.Diagnostics.Debug.WriteLine("has no machine number");
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("{\"code\":1,\"msg\":\"has no such machine number\",\"output\":[]}", Encoding.UTF8, "application/json"),
                    };
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
            try
            {
                string user_id = Convert.ToString(obj.user_id);
                string user_name = Convert.ToString(obj.user_name);
                string card_number = Convert.ToString(obj.card_number);
                if (obj.user_id == null || obj.user_name == null || obj.card_number == null)
                {
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
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteUser(dynamic obj)
        {
            try
            {
                int i = 0;
                string user_id = Convert.ToString(obj.user_id);
                if (obj.user_id == null)
                {
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
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new HttpResponseMessage()
                {
                    Content = new StringContent("{\"code\":1,\"msg\":\"" + e.Message + "\",\"output\":[]}", Encoding.UTF8, "application/json"),
                };
            }  
        }
        
    }
}
