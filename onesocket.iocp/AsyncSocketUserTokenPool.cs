using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.iocp
{

  /// <summary>  异步对象消息类池
  /// </summary>
  public class AsyncSocketUserTokenPool
  {
    private Stack<AsyncSocketUserToken> m_pool;

    public AsyncSocketUserTokenPool(int capacity)
    {
      m_pool = new Stack<AsyncSocketUserToken>(capacity);
    }

    public void Push(AsyncSocketUserToken item)
    {
      if (item == null)
      {
        //throw new ArgumentException("cannot be null");
      }
      lock (m_pool)
      {
        m_pool.Push(item);
      }
    }

    public AsyncSocketUserToken Pop()
    {
      lock (m_pool)
      {
        if (m_pool.Count > 0)
        {
          return m_pool.Pop();
        }
        else
        {
          return new AsyncSocketUserToken();
          //return null;
        }

      }
    }

    public int Count
    {
      get { return m_pool.Count; }
    }

  }

}
