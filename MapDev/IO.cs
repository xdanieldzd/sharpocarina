using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharpOcarina
{
    static class IO
    {
        public static void Export<T>(T Object, string Filename)
        {
            XmlWriterSettings XWS = new XmlWriterSettings();
            XWS.NewLineChars = Environment.NewLine;
            XWS.Indent = true;

            XmlSerializer XS = new XmlSerializer(Object.GetType());
            StreamWriter SW = new StreamWriter(Filename);
            XmlWriter XW = XmlWriter.Create(SW, XWS);

            XW.WriteStartDocument();
            XW.WriteComment("Created with " + Program.ApplicationTitle);
            XS.Serialize(XW, Object);
            XW.WriteEndDocument();
            XW.Flush();

            SW.Close();
        }

        public static T Import<T>(string Filename)
        {
            XmlSerializer XS = new XmlSerializer(typeof(T));
            StreamReader SR = new StreamReader(Filename);

            return (T)XS.Deserialize(SR);
        }
        /*
        public static void BinExport<T>(T Object, string Filename)
        {
            Stream stream = File.Open(Filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, Object);
            stream.Close();
        }*/
    }
}
