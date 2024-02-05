using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace onesocket.iocp
{
  public delegate void UserWorkEventHandler<T>(object sender, WorkQueue<T>.EnqueueEventArgs e);
  public class WorkQueue<T>
  {
    #region 队列配置变量

    /// <summary>
    /// 对IsWorking的同步对象
    /// </summary>
    private object lockIsWorking = new object();//对IsWorking的同步对象
    private Queue<T> queue; //实际的队列
                            /// <summary>
                            /// 队列同步对象
                            /// </summary>
    private object lockObj = new object(); //队列同步对象

    /// <summary>
    /// 队列处理是否需要单线程顺序执行
    /// ture表示单线程处理队列的T对象
    /// false表明按照顺序出队，但是多线程处理item
    /// </summary>
    // private bool isOneThread = true;

    /// <summary>
    /// 绑定用户需要对队列中的item对象
    /// 施加的操作的事件
    /// </summary>
    public event UserWorkEventHandler<T> UserWork;
    #endregion

    /// <summary>
    /// 初始化 System.Collections.Generic.Queue<T> 类的新实例，该实例为空并且具有指定的初始容量。
    /// </summary>
    /// <param name="n"></param>
    public WorkQueue(int n)
    {
      queue = new Queue<T>(n);
    }

    /// <summary>
    /// 初始化 System.Collections.Generic.Queue<T> 类的新实例
    /// </summary>
    public WorkQueue()
    {
      queue = new Queue<T>();

    }

    /// <summary>
    /// 谨慎使用此函数，
    /// 只保证此瞬间，队列值为空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
      lock (lockObj)
      {
        return queue.Count == 0;
      }
    }

    /// <summary>
    /// 获取队列中的元素数量
    /// </summary>
    /// <returns></returns>
    public int GetQueeuCount()
    {
      lock (lockObj)
      {
        return queue.Count;
      }
    }

    ///// <summary>
    ///// 队列处理是否需要单线程顺序执行
    ///// ture表示单线程处理队列的T对象
    ///// false表明按照顺序出队，但是多线程处理item
    ///// *****注意不要频繁改变此项****
    ///// </summary>
    //public bool WorkSequential
    //{
    //    get
    //    {
    //        return isOneThread;
    //    }
    //    set
    //    {
    //        isOneThread = value;
    //    }

    //}



    /// <summary>
    /// 向工作队列添加对象，
    /// 对象添加以后，如果已经绑定工作的事件
    /// 会触发事件处理程序，对item对象进行处理
    /// </summary>
    /// <param name="item">添加到队列的对象</param>
    public void EnqueueItem(T item)
    {
      lock (lockObj)
      {
        queue.Enqueue(item);
      }


    }
    /// <summary>
    /// 取出对象
    /// </summary>
    /// <returns>返回队列中对象，如果为空，或者异常则返回null</returns>
    public T DequeueItem()
    {

      lock (lockObj)
      {
        if (queue.Count > 0)
        {
          return queue.Dequeue();
        }
        else
        {
          return default(T);
        }
      }

    }



    /// <summary>
    /// 开启线程任务，注意要先注册UserWork，否则开启失败
    /// </summary>
    public bool StartUserWork()
    {
      try
      {
        if (UserWork != null)
        {
          ThreadPool.QueueUserWorkItem(DoUserWork);
          return true;
        }
        return false;
      }
      catch (Exception ex)
      { Logger.WriteLog(ex.Message); return false; }
    }
    /// <summary>
    /// 处理队列中对象的函数
    /// </summary>
    /// <param name="o"></param>
    private void DoUserWork(object o)
    {
      try
      {
        T item = default(T);

        while (true)
        {
          lock (lockObj)
          {
            if (queue.Count > 0)
            {
              item = queue.Dequeue();

              if (item != null && !item.Equals(default(T)))
              {
                if (UserWork != null)
                {
                  UserWork(this, new EnqueueEventArgs(item));
                }
                //if (isOneThread)
                //{
                //    if (UserWork != null)
                //    {
                //        UserWork(this, new EnqueueEventArgs(item));
                //    }
                //}
                //else
                //{
                //    ThreadPool.QueueUserWorkItem(obj =>
                //    {
                //        if (UserWork != null)
                //        {
                //            UserWork(this, new EnqueueEventArgs(obj));
                //        }
                //    }, item);
                //}


              }
            }
            else
            {
              Thread.Sleep(30);
            }
          }


        }
      }
      catch (Exception ex)
      {
        Logger.WriteLog(ex.Message);//写入日志
      }
    }

    /// <summary>
    /// UserWork事件的参数，包含item对象
    /// </summary>
    public class EnqueueEventArgs : EventArgs
    {
      public T Item { get; private set; }
      public EnqueueEventArgs(object item)
      {
        try
        {
          Item = (T)item;
        }
        catch (Exception)
        {

          throw new InvalidCastException("object to T 转换失败");
        }
      }
    }
  }

}
