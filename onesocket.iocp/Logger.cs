using onesocket.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace onesocket.iocp
{
   public static class Logger
    {
    public static void WriteLog(string logmes)
    {
      LogHelper.WriteLog(logmes);
    }
  }
}
