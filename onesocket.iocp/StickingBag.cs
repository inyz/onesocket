using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onesocket.iocp
{
  public class StickingBag
  {
    /// <summary>大内存粘包处理
    /// </summary>
    /// <param name="reciveByteArr">未做粘包处理原始字节数组</param>
    /// <param name="dicBufferIndex">粘包的缓存区索引（目前使用终结点）</param>
    /// <param name="dicBufferDic">粘包的缓存区字典</param>
    /// <returns>一个或者多个完整的数据包集合</returns>
    public static List<byte[]> MakeStickingBag(byte[] reciveByteArr, string dicBufferIndex, ConcurrentDictionary<string, DynamicBufferManager> dicBufferDic)
    {

      var completeByteArrList = new List<byte[]>();//存放完整的数据包

      try
      {

        var byteRecive = reciveByteArr;

        //判断粘包缓存区是否存在数据
        if (dicBufferDic.ContainsKey(dicBufferIndex))
        {
          dicBufferDic[dicBufferIndex].WriteBuffer(byteRecive);

          var tempByteArr = dicBufferDic[dicBufferIndex].Buffer;

          int commandLen = BitConverter.ToInt32(tempByteArr, 0);

          while (dicBufferDic[dicBufferIndex].GetDataCount() >= (commandLen + 4) && dicBufferDic[dicBufferIndex].GetDataCount() > 0)
          {

            var tempArr = new byte[commandLen];

            Array.Copy(tempByteArr, sizeof(int), tempArr, 0, commandLen);

            dicBufferDic[dicBufferIndex].Clear(commandLen + 4);

            completeByteArrList.Add(tempArr);

            if (dicBufferDic[dicBufferIndex].GetDataCount() <= 4)
            {
              break;
            }

            commandLen = BitConverter.ToInt32(tempByteArr, 0); //取出命令长度

          }
        }
        else
        {

          var dy = new DynamicBufferManager(300);//新建一个DynamicBufferManager，如果处理完后有不完整的包就放入到缓存区

          dy.WriteBuffer(byteRecive);

          var tempByteArr = dy.Buffer;

          int commandLen = BitConverter.ToInt32(tempByteArr, 0);

          while (dy.GetDataCount() >= (commandLen + 4) && dy.GetDataCount() > 0)
          {

            var tempArr = new byte[commandLen];

            Array.Copy(tempByteArr, sizeof(int), tempArr, 0, commandLen);

            dy.Clear(commandLen + 4);

            completeByteArrList.Add(tempArr);

            if (dy.GetDataCount() <= 4)
            {
              break;
            }

            commandLen = BitConverter.ToInt32(tempByteArr, 0); //取出命令长度

          }

          //如果当前的 byte 数组 里面有剩余字节，就加到粘包的缓存区
          if (dy.GetDataCount() > 0)
          {
            dicBufferDic.TryAdd(dicBufferIndex, dy);
          }

        }

        return completeByteArrList;

      }
      catch (Exception ex)
      {

        Logger.WriteLog("粘包处理:" + ex.Message);
        return completeByteArrList;
      }

    }

    #region 注释的


    /// <summary> /// 处理粘包信息
    /// </summary>
    /// <param name="PrimitiveArr">未做粘包处理原始字节消息</param>
    /// <param name="dicBufferIndex">粘包的缓存区索引（目前使用终结点）</param>
    /// <returns>一个或者多个完整的数据包集合</returns>
    //public static List<byte[]> MakeStickingBag(byte[] PrimitiveArr, string dicBufferIndex, ConcurrentDictionary<string, ArrayList> dicBuffer)
    //{
    //    List<byte[]> completebt = new List<byte[]>();//完整的数据包存放变量

    //    try
    //    {
    //        //粘包
    //        byte[] byteRecive = PrimitiveArr;

    //        //判断粘包缓存区是否存在数据
    //        if (dicBuffer.ContainsKey(dicBufferIndex))
    //        {
    //            dicBuffer[dicBufferIndex].AddRange(byteRecive);

    //            byte[] recive = new byte[dicBuffer[dicBufferIndex].Count];
    //            dicBuffer[dicBufferIndex].CopyTo(recive);
    //            while ((BitConverter.ToInt32(recive, 0) + 4) <= dicBuffer[dicBufferIndex].Count && dicBuffer[dicBufferIndex].Count > 0)
    //            {
    //                int commandLen = BitConverter.ToInt32(recive, 0); //取出命令长度
    //                byte[] tempArr = new byte[commandLen];
    //                dicBuffer[dicBufferIndex].CopyTo(sizeof(int), tempArr, 0, commandLen);
    //                dicBuffer[dicBufferIndex].RemoveRange(0, commandLen + 4);
    //                recive = new byte[dicBuffer[dicBufferIndex].Count];
    //                dicBuffer[dicBufferIndex].CopyTo(recive);
    //                completebt.Add(tempArr);

    //                if (dicBuffer[dicBufferIndex].Count <= 4)
    //                {
    //                    break;
    //                }

    //            }
    //        }
    //        else
    //        {

    //            ArrayList arrlist = new ArrayList();//新建一个arraylist，粘包缓存区arraylist，如果处理完后有不完整的包就放入到缓存区
    //            arrlist.AddRange(byteRecive);
    //            byte[] recive = new byte[arrlist.Count];
    //            arrlist.CopyTo(recive);

    //            while ((BitConverter.ToInt32(recive, 0) + 4) <= arrlist.Count && arrlist.Count > 0)
    //            {
    //                int commandLen = BitConverter.ToInt32(recive, 0); //取出命令长度
    //                byte[] tempArr = new byte[commandLen];
    //                arrlist.CopyTo(sizeof(int), tempArr, 0, commandLen);
    //                arrlist.RemoveRange(0, commandLen + 4);
    //                recive = new byte[arrlist.Count];
    //                arrlist.CopyTo(recive);
    //                completebt.Add(tempArr);

    //                if (arrlist.Count <= 4)
    //                {
    //                    break;
    //                }

    //            }
    //            //如果当前的arrylist里面有剩余字节，就加到粘包的缓存区,没有说明当前处理的是完整的命令
    //            if (arrlist.Count > 0)
    //            {
    //                dicBuffer.TryAdd(dicBufferIndex, arrlist);
    //            }

    //        }
    //        return completebt;
    //    }
    //    catch (Exception ex)
    //    {

    //        Logger.WriteLog(ex.Message);
    //        return completebt;
    //    }

    //}

    #endregion


  }
}
