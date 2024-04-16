using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CRM_Sample.Common
{
    public static class ImportOfx
    {
        public static XElement ToXElement(string pathToOfxFile)
        {
            if (!File.Exists(pathToOfxFile))
            {
                throw new FileNotFoundException();
            }

            var tags = from line in File.ReadAllLines(pathToOfxFile)
                       where
                       line.Contains("<STMTTRN>") ||
                       line.Contains("<TRNTYPE>") ||
                       line.Contains("<ORG>") ||
                       line.Contains("<DTPOSTED>") ||
                       line.Contains("<TRNAMT>") ||
                       line.Contains("<FITID>") ||
                       line.Contains("<CHECKNUM>") ||
                       line.Contains("<MEMO>")
                       select line;

            XElement element = new("root");
            XElement transaction = null;
            XElement bank = new("BANK")
            {
                Value = GetBankName(tags)
            };
            element.Add(bank);

            foreach (var l in tags)
            {
                if (l.IndexOf("<STMTTRN>") != -1)
                {
                    transaction = new XElement("STMTTRN");
                    element.Add(transaction);
                    continue;
                }
                if (l.IndexOf("<ORG>") > -1) continue;

                var tagName = GetTagName(l);
                var transactionElement = new XElement(tagName)
                {
                    Value = GetTagValue(l, tagName)
                };
                transaction.Add(transactionElement);
            }
            return element;
        }

        public static string GetBankName(IEnumerable<string> tags)
        {
            foreach (var line in tags)
            {
                if (line.IndexOf("<ORG>") != -1)
                {
                    int pos_init = line.IndexOf(">") + 1;
                    int pos_end = line.IndexOf("<", pos_init);
                    int length;
                    if (pos_end == -1)
                    {
                        length = line.Length - pos_init;
                    }
                    else
                    {
                        length = pos_end - pos_init;
                    }

                    var substring = line.Substring(pos_init, length).Trim();
                    return substring;
                }
            }
            return "";
        }

        /// <summary>
        /// Get the Tag name to create an Xelement
        /// </summary>
        /// <param name="line">One line from the file</param>
        /// <returns></returns>
        private static string GetTagName(string line)
        {
            int pos_init = line.IndexOf("<") + 1;
            int pos_end = line.IndexOf(">");
            var length = pos_end - pos_init;
            var substring = line.Substring(pos_init, length).Trim();
            return substring;
        }
        /// <summary>
        /// Get the value of the tag to put on the Xelement
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns></returns>
        private static string GetTagValue(string line, string tagName)
        {
            int pos_init = line.IndexOf(">") + 1;
            int pos_end = line.IndexOf("<", pos_init);
            int length;
            if (tagName == "DTPOSTED")
            {
                length = 8;
            }
            else if (pos_end == -1)
            {
                length = line.Length - pos_init;
            }
            else
            {
                length = pos_end - pos_init;
            }

            var substring = line.Substring(pos_init, length).Trim();
            return substring;
        }
    }
}
