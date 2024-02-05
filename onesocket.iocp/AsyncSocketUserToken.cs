using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.iocp
{
  /// <summary>  异步对象消息类：
  /// 功能：消息队列存储单位，用于异步对象和消息的传递。
  /// 包含：SocketAsyncEventArgs对象,Socket对象句柄,本次接收到的消息数组,所属队列id
  /// </summary>
  public class AsyncSocketUserToken
  {
    public string issystemorder = "0";//默认不是系统
                                    /// <summary> socket异步对象
                                    /// </summary>
    private SocketAsyncEventArgs m_receiveEventArgs;
    public SocketAsyncEventArgs ReceiveEventArgs { get { return m_receiveEventArgs; } set { m_receiveEventArgs = value; } }

    /// <summary> 缓存接收到的数据的类
    /// </summary>
    private byte[] m_receiveBuffer;
    public byte[] ReceiveBuffer { get { return m_receiveBuffer; } set { m_receiveBuffer = value; } }

    /// <summary> socket连接句柄
    /// </summary>
    private int m_connectSocketHandle;
    public int ConnectSocketHandle { get { return m_connectSocketHandle; } set { m_connectSocketHandle = value; } }

    /// <summary> 所属队列id
    /// </summary>
    private int m_queueId;
    public int QueueId
    {
      get { return m_queueId; }
      set { m_queueId = value; }
    }

    /// <summary> socket的ip和端口号
    /// </summary>
    private string m_ipportStr;
    public string IpportStr
    {
      get { return m_ipportStr; }
      set { m_ipportStr = value; }
    }

    public AsyncSocketUserToken()
    {
      m_connectSocketHandle = 0;

      m_receiveEventArgs = null;

      m_receiveBuffer = null;

      m_queueId = -1;

      m_ipportStr = "";

    }

  }
}
