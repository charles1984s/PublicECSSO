using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace ECSSO.Library
{
    public class XMLHelper
    {
        public static string Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer serializer = new XmlSerializer(o.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            serializer.Serialize(writer, o, ns);
            return sb.ToString();
        }
        public static T Deserialize<T>(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return default(T);
            }

            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.LoadXml(s);
                XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);

                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }
        public static XDocument CreateXDocument(XmlHeader Header, List<object> Infos, string Type)
        {
            try
            {
                XDocument xml = new XDocument();
                switch (Type)
                {
                    case "1":
                        {
                            xml =
                               new XDocument(
                                   new XDeclaration("1.0", "utf-8", null),
                                   new XElement("Infos",
                                       Infos.Select(s => new XElement("Info",
                                           JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(s))
                                           .Select(o => new XElement(o.Key, ((o.Value == null || o.Value == "0" || o.Value == "0.0") ? "" : o.Value)))
                                       ))
                                   )
                               );
                            break;
                        }
                    default:
                        {
                            xml =
                               new XDocument(
                                   new XDeclaration("1.0", "utf-8", null),
                                   new XElement("XML_Head",
                                       JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(Header))
                                       .Select(h => new XAttribute(h.Key, ((h.Value == null) ? "" : h.Value))),
                                           new XElement("Infos",
                                               Infos.Select(s => new XElement("Info",
                                                   JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(s))
                                                   .Select(o => new XAttribute(o.Key, ((o.Value == null || o.Value == "0" || o.Value == "0.0") ? "" : o.Value)))
                                               ))
                                           )
                                   )
                               );
                            break;
                        }
                }
                return xml;
            }
            catch (Exception ex)
            {
                XDocument xml =
                new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("XML_Head",
                        new XElement("Infos",
                            new XElement("Info", ex.ToString())
                        )
                    )
                );
                return xml;
            }
        }
    }

    public class XmlHeader
    {
        public string Listname { get; set; }
        public string Language { get; set; }
        public string Orgname { get; set; }
        public string Updatetime { get; set; }
    }
}