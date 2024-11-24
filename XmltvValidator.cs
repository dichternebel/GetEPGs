using System.Xml;
using System.Xml.Schema;

namespace GetEPGs
{
    internal class XmltvValidator
    {
        internal static bool ValidateXmlAgainstDtd(string xmlFilePath, string dtdFilePath)
        {
            try
            {
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Parse,
                    ValidationType = ValidationType.DTD
                };
                settings.ValidationEventHandler += ValidationEventHandler;

                // Create a new XmlUrlResolver and set it as the XmlResolver
                XmlUrlResolver resolver = new();
                resolver.ResolveUri(new Uri(dtdFilePath), null);
                settings.XmlResolver = resolver;

                using (XmlReader reader = XmlReader.Create(xmlFilePath, settings))
                {
                    while (reader.Read()) { }
                }

                return true;
            }
            catch (Exception ex)
            {
                $"Validation failed: {ex.Message}".Warn(true);
                return false;
            }
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
