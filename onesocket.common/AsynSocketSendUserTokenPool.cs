using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.common
{
  public class AsynSocketSendUserTokenPool
  {
    private Stack<AsynSocketSendUserToken> m_pool;

    public AsynSocketSendUserTokenPool(int capacity)
    {
      m_pool = new Stack<AsynSocketSendUserToken>(capacity);
    }

    public void Push(AsynSocketSendUserToken item)
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

    public AsynSocketSendUserToken Pop()
    {
      lock (m_pool)
      {
        if (m_pool.Count > 0)
        {
          return m_pool.Pop();
        }
        else
        {
          return new AsynSocketSendUserToken();
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
