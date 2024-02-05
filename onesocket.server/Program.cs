using onesocket.iocp;
using onesocket.common;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;


namespace onesocket.server
{
    class Program
    {
        public static ConcurrentDictionary<string, AsyncSocketUserToken> ABTEST = new ConcurrentDictionary<string, AsyncSocketUserToken>();
        public static ConcurrentDictionary<string, string> USERID = new ConcurrentDictionary<string, string>();

        public static IoServer socketServer;
        static void Main(string[] args)
        {
            socketServer = new IoServer(600, 1024);
            socketServer.Start(6789);
            socketServer.ReceiveEvent = new ReceiveEventHandler(Receive);//处理接收的消息
            socketServer.DownLineEvent = new DownLineHandler(DownLine);//下线通知
            socketServer.Name = "RegClean";//开启定时清理僵尸连接
            Console.WriteLine("Started Server");
        }
        private static void Receive(AsyncSocketUserToken SocketArg, byte[] byteArr)
        {
            //这是一个tcp 打洞的demo，服务端的代码。配合多个client适用，只约定了两个简单的命令头
            try
            {
                string strAll = Encoding.UTF8.GetString(byteArr);
                //不符合应用约定规范，舍弃
                if (strAll.Length < 6)
                {
                    return;//丢弃
                }
                Console.WriteLine("接受消息(" + SocketArg.IpportStr + "):" + strAll);
                string commandStr = strAll.Substring(0, 6);
                string[] strs = strAll.Split(new string[] { "[f.ff]" }, StringSplitOptions.RemoveEmptyEntries);
                switch (commandStr)
                {
                    case "000001":
                        if (strs.Length == 2)
                        {
                            if (!USERID.Values.Contains(strs[1]) && !USERID.ContainsKey(strs[1]) && !USERID.ContainsKey(SocketArg.IpportStr))//有并发问题，先不考虑
                            {
                                USERID.TryAdd(SocketArg.IpportStr, strs[1]);
                                USERID.TryAdd(strs[1], SocketArg.IpportStr);
                                socketServer.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("注册成功！"));
                                ABTEST.TryAdd(strs[1], SocketArg);
                                //注册成功，移除僵尸链接队列。为通过认证的连接会自动清理掉
                                socketServer.RemoveZombieSocketAsyncEventArgs(SocketArg.ReceiveEventArgs);
                            }
                            else
                            {
                                socketServer.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("注册失败--id被重复或者已经注册其他id"));

                            }
                        }
                        break;
                    case "000002":
                        if (strs.Length == 2)
                        {
                            if (USERID.ContainsKey(strs[1]) && USERID.ContainsKey(SocketArg.IpportStr) && ABTEST.ContainsKey(USERID[SocketArg.IpportStr]) && ABTEST.ContainsKey(strs[1]))//存在要找的id
                            {
                                socketServer.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes(USERID[strs[1]]));
                                socketServer.PushSendQue(ABTEST[strs[1]].ReceiveEventArgs, Encoding.UTF8.GetBytes(SocketArg.IpportStr));
                            }
                            else
                            {
                                socketServer.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("连接失败---id可能不存在"));
                            }
                        }
                        break;
                    default:
                        break;
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("exp:" + e.Message);

            }


        }
        private static void DownLine(AsyncSocketUserToken SocketArg)
        {
            try
            {
                var ipStr = SocketArg.IpportStr;

                if (USERID.ContainsKey(SocketArg.IpportStr))
                {
                    string str = null;
                    AsyncSocketUserToken o = null;
                    USERID.TryRemove(USERID[SocketArg.IpportStr], out str);
                    ABTEST.TryRemove(USERID[SocketArg.IpportStr], out o);
                    USERID.TryRemove(SocketArg.IpportStr, out str);

                }


                Console.WriteLine("掉线客户端：" + ipStr);


            }
            catch (Exception e)
            {
                Console.WriteLine("exp:" + e.Message);

            }

        }

    }
}
