using System;
using System.IO;

namespace Tools
{

    public class CustomLogger
    {
        public CustomLogger(string _logFilePath)
        {
            if (!_logFilePath.EndsWith(".log"))
                _logFilePath += ".log";
            LogFilePath = _logFilePath;
            if (!File.Exists(LogFilePath))
                File.Create(LogFilePath).Close();
            WriteLine("New Session Started");
        }

        public string LogFilePath
        {
            get; private set;
        }

        public void WriteLine(object _message)
        {
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                writer.WriteLine(DateTime.Now.ToString() + ": " + _message.ToString());
        }
    }

}