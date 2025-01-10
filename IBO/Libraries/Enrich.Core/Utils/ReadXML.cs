using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Enrich.Core.Ultils
{
    public class ReadXML
    {
        private XmlDocument doc = new XmlDocument();
        public ReadXML(string fileName)
        {
            doc.Load(fileName);
        }

        public XmlNode GetNode(string path)
        {

            XmlNode node = doc.DocumentElement.SelectSingleNode(path);
            return node;
        }

        public string GetElementValue(string path, string elementName)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode(path);
            var t4 = node[elementName].InnerText;
            return t4;
        }

    }
}
