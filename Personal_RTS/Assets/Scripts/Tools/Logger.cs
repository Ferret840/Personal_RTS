using System;
using System.IO;

namespace Tools
{

public class CustomLogger
{
    public CustomLogger(string logFilePath)
    {
        if (!logFilePath.EndsWith(".log"))
            logFilePath += ".log";
        LogFilePath = logFilePath;
        if (!File.Exists(LogFilePath))
            File.Create(LogFilePath).Close();
        WriteLine("New Session Started");
    }

    public string LogFilePath
    {
        get; private set;
    }

    public void WriteLine(object message)
    {
        using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            writer.WriteLine(DateTime.Now.ToString() + ": " + message.ToString());
    }
}

}