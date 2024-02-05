using System;
using System.Collections.Generic;
using System.Text;

namespace onesocket.common
{
   public static class LogHelper
    {
    public static void WriteLog(string mes)
    {

      Console.WriteLine(mes + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
  }
}
