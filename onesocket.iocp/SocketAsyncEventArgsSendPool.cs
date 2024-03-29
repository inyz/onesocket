﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace onesocket.iocp
{
  /// <summary>
  /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.socketasynceventargs.aspx
  /// Represents a collection of reusable SocketAsyncEventArgs objects.  
  /// </summary>
  internal sealed class SocketAsyncEventArgsSendPool
  {
    /// <summary>
    /// SocketAsyncEventArgs栈
    /// </summary>
    Stack<SocketAsyncEventArgs> pool;

    /// <summary>
    /// 初始化SocketAsyncEventArgs池
    /// </summary>
    /// <param name="capacity">最大可能使用的SocketAsyncEventArgs对象.</param>
    internal SocketAsyncEventArgsSendPool(Int32 capacity)
    {
      this.pool = new Stack<SocketAsyncEventArgs>(capacity);
    }

    /// <summary>
    /// 返回SocketAsyncEventArgs池中的 数量
    /// </summary>
    internal Int32 Count
    {
      get { return this.pool.Count; }
    }

    /// <summary>
    /// 弹出一个SocketAsyncEventArgs
    /// </summary>
    /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
    internal SocketAsyncEventArgs Pop()
    {
      lock (this.pool)
      {
        if (this.pool.Count > 0)
        {
          return this.pool.Pop();
        }
        else
        {
          return new SocketAsyncEventArgs();
          //return null;
        }
      }
    }

    /// <summary>
    /// 添加一个 SocketAsyncEventArgs
    /// </summary>
    /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
    internal void Push(SocketAsyncEventArgs item)
    {
      if (item != null)
      {
        lock (this.pool)
        {
          this.pool.Push(item);
        }
      }

    }
  }
}
