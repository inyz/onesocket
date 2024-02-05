using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.common
{
  public class SendWorkQueue<T>
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
    //public event UserWorkEventHandler<T> UserWork;
    #endregion

    /// <summary>
    /// 初始化 System.Collections.Generic.Queue<T> 类的新实例，该实例为空并且具有指定的初始容量。
    /// </summary>
    /// <param name="n"></param>
    public SendWorkQueue(int n)
    {
      queue = new Queue<T>(n);
    }

    /// <summary>
    /// 初始化 System.Collections.Generic.Queue<T> 类的新实例
    /// </summary>
    public SendWorkQueue()
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


    public void work()
    {

    }

    /// <summary> /// 处理队列中的消息
    /// </summary>
    /// <param name="obj">队列约定编号</param>
    //public void DoWorkForQueue(object obj)
    //{
    //    SendWorkQueue<AsynSocketSendUserTokenPool> que = _workQueueList[workQueueIndex];

    //    while (true)
    //    {
    //        try
    //        {


    //            if (que.GetQueeuCount() > 0)
    //            {

    //                AsyncSocketUserToken asyncUserToken = que.DequeueItem();
    //                if (asyncUserToken != null)
    //                {
    //                    List<byte[]> listcompletebt = StickingBag.MakeStickingBag(asyncUserToken.ReceiveBuffer, asyncUserToken.IpportStr, GetDicBuffer(asyncUserToken.QueueId));

    //                    for (int i = 0; i < listcompletebt.Count; i++)
    //                    {
    //                        if (ReceiveEvent != null)
    //                        {
    //                            ReceiveEvent(asyncUserToken, listcompletebt[i]);
    //                        }
    //                    }

    //                    GiveBackAsyncSocketUserToken(asyncUserToken);

    //                }
    //            }
    //            else
    //            {
    //                Thread.Sleep(100);
    //            }
    //        }
    //        catch (Exception ex)
    //        {

    //            Mfg.Comm.Log.LogHelper.LogHelper.getLogByConfigLogName("LogFileAppenderByDate").Info(ex.Message);
    //        }



    //    }

    //}
  }
}
