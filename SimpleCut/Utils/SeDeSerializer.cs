using System.IO;
using System.Xml.Serialization;

namespace SimpleCut.Utils
{
    public class SeDeSerializer
    {
        static string ConfigFileName = "Configuration.xml";

        public static void SerializeConfig(Configuration config)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Configuration));
            TextWriter writer = new StreamWriter(ConfigFileName);
            ser.Serialize(writer, config);
            writer.Close();
        }

        public static Configuration DeSerializeConfig()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Configuration));
            StreamReader reader = new StreamReader(ConfigFileName);
            Configuration config = ser.Deserialize(reader) as Configuration;
            reader.Close();

            return config;
        }

    }
}
