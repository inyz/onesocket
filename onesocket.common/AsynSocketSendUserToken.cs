using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.common
{
  public class AsynSocketSendUserToken
  {
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





  }
}
