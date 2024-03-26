using onesocket.common;
using onesocket.iocp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace onesocket.websocket
{
    class Program
    {
        public static ConcurrentDictionary<string, SocketAsyncEventArgs> USERLIST = new ConcurrentDictionary<string, SocketAsyncEventArgs>();

        public static ConcurrentDictionary<string, byte[]> WEBSOCKETCACHE = new ConcurrentDictionary<string, byte[]>();
        public static ConcurrentDictionary<string, string> USERIP = new ConcurrentDictionary<string, string>();
        /// <summary>发送的socket
        /// </summary>
        public static IoServer socketserver;
        static void Main(string[] args)
        {
            //这是一个websocket的简单demo， 需要开一个当前应用实例，然后在用浏览器打开websocket.html作为client，即可连接。
            socketserver = new IoServer(60, 1024);
            //////////socketserver.Start("127.0.0.1", 6789);
            socketserver.Start(6789);
            socketserver.ReceiveEvent = new ReceiveEventHandler(MyFuncion);
            socketserver.DownLineEvent = new DownLineHandler(DownLine);
            socketserver.Name = "RegClean";
            Console.WriteLine("start");
        }

        private static void DownLine(AsyncSocketUserToken SocketArg)
        {
            var ipstr = SocketArg.IpportStr;
            var aaaa = new byte[0];
            WEBSOCKETCACHE.TryRemove(SocketArg.IpportStr, out aaaa);
            Console.WriteLine("掉线客户端：" + ipstr);

            if (USERIP.ContainsKey(ipstr))
            {
                foreach (var item in USERLIST)
                {
                    if (item.Key != USERIP[ipstr])
                    {
                        DataFrame dfff = new DataFrame("用户：" + USERIP[ipstr] + "已经掉线");
                        socketserver.PushSendQue(item.Value, dfff.GetBytes());
                        DataFrame dfff1 = new DataFrame("USERLIST," + string.Join(",", USERLIST.Select(a => a.Key).Where(b => b != USERIP[ipstr])));
                        socketserver.PushSendQue(item.Value, dfff1.GetBytes());
                    }
                }
                var o = new SocketAsyncEventArgs();
                USERLIST.TryRemove(USERIP[ipstr], out o);
                var stemp = "";
                USERIP.TryRemove(ipstr, out stemp);

            }

        }
        private static byte[] CombomBinaryArray(byte[] srcArray1, byte[] srcArray2)
        {
            //根据要合并的两个数组元素总数新建一个数组
            byte[] newArray = new byte[srcArray1.Length + srcArray2.Length];

            //把第一个数组复制到新建数组
            Array.Copy(srcArray1, 0, newArray, 0, srcArray1.Length);

            //把第二个数组复制到新建数组
            Array.Copy(srcArray2, 0, newArray, srcArray1.Length, srcArray2.Length);

            return newArray;
        }
        private static void MyFuncion(AsyncSocketUserToken SocketArg, byte[] byteArr)
        {
            // socketserver.ShutdownSocket(SocketArg.ReceiveEventArgs.UserToken as Socket, SocketArg.ReceiveEventArgs);//-------掉线操作

            try
            {

                if (byteArr.Length < 1)
                {
                    return;
                }
                //连接

                //socketserver.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type:text/html;charset=utf-8\r\nContent-Length:18\r\n\r\nWelcome to tinyweb"));
                //   return;

                // Console.WriteLine("queueid:" + SocketArg.QueueId);
                string strAll = Encoding.UTF8.GetString(byteArr);

                //Console.WriteLine(strAll);

                string commandStr = "";
                if (strAll.Length > 5)
                {
                    commandStr = strAll.Substring(0, 6);
                }
                string str = strAll.Substring(6);
                string[] strs = str.Split(new string[] { "[f.ff]" }, StringSplitOptions.RemoveEmptyEntries);

                //上面【000001，001】是自定义协议（测试使用），default里面按照websocket协议解析数据格式，
                switch (commandStr)
                {
                    case "000001"://注册身份命令 001[f.f]黎明

                        socketserver.RemoveZombieSocketAsyncEventArgs(SocketArg.ReceiveEventArgs);
                        break;

                    case "001"://注册身份命令 001[f.f]黎明

                        socketserver.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("你好"));
                        //不用占包，toserver内部处理

                        break;

                    default:

                        #region Sec-WebSocket-Key WebSocketConnect

                        if (strAll.Contains("Sec-WebSocket-Key"))
                        {
                            //socketserver.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept:xsOSgr30aKL2GNZKNHKmeT1qYjA = "));

                            string header = strAll;

                            try
                            {
                                Regex webSocketKeyRegex = new Regex("Sec-WebSocket-Key: (.*)");
                                Regex webSocketVersionRegex = new Regex("Sec-WebSocket-Version: (.*)");

                                // check the version. Support version 13 and above
                                const int WebSocketVersion = 13;
                                int secWebSocketVersion = Convert.ToInt32(webSocketVersionRegex.Match(header).Groups[1].Value.Trim());
                                if (secWebSocketVersion < WebSocketVersion)
                                {
                                    //throw new WebSocketVersionNotSupportedException(string.Format("WebSocket Version {0} not suported. Must be {1} or above", secWebSocketVersion, WebSocketVersion));
                                }
                                string secWebSocketKey = webSocketKeyRegex.Match(header).Groups[1].Value.Trim();
                                SHA1 sha1 = new SHA1CryptoServiceProvider();
                                byte[] bytes_sha1_in = Encoding.UTF8.GetBytes(secWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                                byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
                                string str_sha1_out = Convert.ToBase64String(bytes_sha1_out);
                                string setWebSocketAccept = str_sha1_out;

                                string response = ("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                                   + "Connection: Upgrade" + Environment.NewLine
                                                   + "Upgrade: websocket" + Environment.NewLine
                                                   + "Sec-WebSocket-Accept: " + setWebSocketAccept + Environment.NewLine + Environment.NewLine);
                                // Console.WriteLine(response);
                                socketserver.PushSendQue(SocketArg.ReceiveEventArgs, Encoding.UTF8.GetBytes(response));
                                return;
                            }

                            catch (Exception ex)
                            {

                                Console.WriteLine("default case exp:" + ex.Message);

                            }

                            return;
                        }
                        #endregion


                        if (!WEBSOCKETCACHE.ContainsKey(SocketArg.IpportStr))
                        {
                            WEBSOCKETCACHE.TryAdd(SocketArg.IpportStr, byteArr);
                        }
                        else
                        {
                            WEBSOCKETCACHE[SocketArg.IpportStr] = CombomBinaryArray(WEBSOCKETCACHE[SocketArg.IpportStr], byteArr);

                        }

                        List<string> slist = AnalyzeClientData(SocketArg.IpportStr);
                        foreach (var s in slist)
                        {
                            string messageReceived = string.Empty;

                            if (s.Trim().StartsWith("name:") && s.Trim().Length > 5)
                            {
                                var name = s.Trim().Substring(5);
                                if (USERLIST.ContainsKey(name))
                                {
                                    DataFrame dfff = new DataFrame("你注册的用户名已存在，请换个名字试试！");

                                    socketserver.PushSendQue(SocketArg.ReceiveEventArgs, dfff.GetBytes());
                                    continue;
                                }
                                else
                                {
                                    if (USERIP.ContainsKey(SocketArg.IpportStr))//已经注册过
                                    {
                                        DataFrame dfff = new DataFrame("你已经注册过用户名，你的用户名为：" + USERIP[SocketArg.IpportStr]);

                                        socketserver.PushSendQue(SocketArg.ReceiveEventArgs, dfff.GetBytes());
                                        continue;
                                    }
                                    socketserver.RemoveZombieSocketAsyncEventArgs(SocketArg.ReceiveEventArgs);
                                    USERLIST.TryAdd(name, SocketArg.ReceiveEventArgs);
                                    USERIP.TryAdd(SocketArg.IpportStr, name);

                                    foreach (var item in USERLIST.Values)
                                    {
                                        DataFrame dfff = new DataFrame("用户：" + name + "已经登录");
                                        socketserver.PushSendQue(item, dfff.GetBytes());

                                        DataFrame dfff1 = new DataFrame("USERLIST," + string.Join(",", USERLIST.Select(a => a.Key)));
                                        socketserver.PushSendQue(item, dfff1.GetBytes());
                                    }

                                }
                                continue;
                            }

                            string fromname = "未知用户";
                            if (USERIP.ContainsKey(SocketArg.IpportStr))
                            {
                                fromname = USERIP[SocketArg.IpportStr];
                            }

                            if (s.Trim().StartsWith("@") && s.Trim().Length > 1)
                            {
                                var name = s.Trim();
                                var toname = "";
                                var content = "";
                                if (name.IndexOf(" ") > -1)
                                {
                                    toname = name.Substring(1, name.IndexOf(" ") - 1);
                                    content = name.Replace("@" + toname + " ", "");
                                }
                                else
                                {
                                    toname = name.Substring(1);
                                }
                                if (USERLIST.ContainsKey(toname))
                                {

                                    DataFrame dfff = new DataFrame("消息来自：" + fromname + "，内容：" + content);

                                    socketserver.PushSendQue(USERLIST[toname], dfff.GetBytes());

                                    DataFrame dfff2 = new DataFrame("消息\"" + content + "\"已发送给" + toname + "！");

                                    socketserver.PushSendQue(SocketArg.ReceiveEventArgs, dfff2.GetBytes());
                                    continue;
                                }
                                else
                                {
                                    DataFrame dfff = new DataFrame("你发送的对象不在线！");

                                    socketserver.PushSendQue(SocketArg.ReceiveEventArgs, dfff.GetBytes());
                                }


                                continue;
                            }

                            if (s.Trim() != "")
                            {
                                foreach (var item in USERLIST.Values)
                                {
                                    DataFrame dfff = new DataFrame("消息来自：" + fromname + "，内容：" + s);
                                    socketserver.PushSendQue(item, dfff.GetBytes());
                                }
                            }








                        }

                        break;

                }



            }
            catch (Exception ex)
            {

                Console.WriteLine("exp ssss" + ex.Message);
            }



        }


        #region 处理命令符在命令符的头部添加命令长度
        /// <summary>处理命令符在命令符的头部添加命令长度
        /// </summary>
        /// <param name="dataStr">需要处理的命令</param>
        /// <returns></returns>
        //private static byte[] GetByte(string dataStr)
        //{
        //  byte[] oldByteArr = System.Text.Encoding.UTF8.GetBytes(dataStr);
        //  return oldByteArr;
        //  byte[] newByteArr = new byte[sizeof(int) + oldByteArr.Length];
        //  byte[] lengthArr = BitConverter.GetBytes(oldByteArr.Length);

        //  Array.Copy(lengthArr, 0, newByteArr, 0, lengthArr.Length);
        //  Array.Copy(oldByteArr, 0, newByteArr, lengthArr.Length, oldByteArr.Length);
        //  return newByteArr;
        //}
        #endregion

        #region 打包请求连接数据
        /// <summary>
        /// 打包请求连接数据
        /// </summary>
        /// <param name="handShakeBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "")
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            byte[] secKeyBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }
        #endregion

        #region 处理接收的数据
        /// <summary>
        /// 处理接收的数据
        /// 参考 http://www.cnblogs.com/smark/archive/2012/11/26/2789812.html
        /// </summary>
        /// <param name="recBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string AnalyzeClientData(byte[] recBytes, int length, string strip)
        {


            int start = 0;
            byte[] mDataPackage;
            try
            {

                // 如果有数据则至少包括3位
                if (length < 2) return "";
                // 判断是否为结束针
                bool IsEof = (recBytes[start] >> 7) > 0;
                // 暂不处理超过一帧的数据
                if (!IsEof) return "";
                start++;
                // 是否包含掩码
                bool hasMask = (recBytes[start] >> 7) > 0;
                // 不包含掩码的暂不处理
                if (!hasMask) return "";
                // 获取数据长度
                UInt64 mPackageLength = (UInt64)recBytes[start] & 0x7F;
                start++;
                // 存储4位掩码值
                byte[] Masking_key = new byte[4];
                // 存储数据

                if (mPackageLength == 126)
                {
                    // 等于126 随后的两个字节16位表示数据长度
                    mPackageLength = (UInt64)(recBytes[start] << 8 | recBytes[start + 1]);
                    start += 2;
                }
                //if (mPackageLength == 127)---------这个地方有case
                //{
                //  // 等于127 随后的八个字节64位表示数据长度
                //  mPackageLength = (UInt64)(recBytes[start] << (8 * 7) | recBytes[start] << (8 * 6) | recBytes[start] << (8 * 5) | recBytes[start] << (8 * 4) | recBytes[start] << (8 * 3) | recBytes[start] << (8 * 2) | recBytes[start] << 8 | recBytes[start + 1]);
                //  start += 8;
                //}
                mDataPackage = new byte[mPackageLength];
                for (UInt64 i = 0; i < mPackageLength; i++)
                {
                    mDataPackage[i] = recBytes[i + (UInt64)start + 4];
                }
                Buffer.BlockCopy(recBytes, start, Masking_key, 0, 4);
                for (UInt64 i = 0; i < mPackageLength; i++)
                {
                    mDataPackage[i] = (byte)(mDataPackage[i] ^ Masking_key[i % 4]);
                }
            }
            catch (Exception exp)
            {

                return "";
            }

            byte[] s = new byte[recBytes.Length - (4 + start) - mDataPackage.Length];
            Buffer.BlockCopy(recBytes, (4 + start) + mDataPackage.Length, s, 0, recBytes.Length - (4 + start) - mDataPackage.Length);
            WEBSOCKETCACHE[strip] = s;

            return Encoding.UTF8.GetString(mDataPackage);
        }



        private static List<string> AnalyzeClientData(string strip)
        {
            List<string> ret = new List<string>();
            while (true)
            {
                var st = AnalyzeClientData(WEBSOCKETCACHE[strip], WEBSOCKETCACHE[strip].Length, strip);
                if (st == "")
                {
                    break;
                }
                ret.Add(st);
            }
            return ret;
        }
        #endregion

        #region 发送数据
        /// <summary>
        /// 把发送给客户端消息打包处理（拼接上谁什么时候发的什么消息）
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        private static byte[] PackageServerData(string message)
        {
            byte[] contentBytes = null;
            byte[] temp = Encoding.UTF8.GetBytes(message);

            if (temp.Length < 126)
            {
                contentBytes = new byte[temp.Length + 2];
                contentBytes[0] = 0x81;
                contentBytes[1] = (byte)temp.Length;
                Array.Copy(temp, 0, contentBytes, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                contentBytes = new byte[temp.Length + 4];
                contentBytes[0] = 0x81;
                contentBytes[1] = 126;
                contentBytes[2] = (byte)(temp.Length & 0xFF);
                contentBytes[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, contentBytes, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容
            }
            return contentBytes;
        }
        #endregion

    }
}
