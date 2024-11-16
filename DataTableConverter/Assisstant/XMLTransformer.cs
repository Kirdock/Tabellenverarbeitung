using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using DataTableConverter.Classes;
using System.Windows.Forms;
using DataTableConverter.View;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Data.Entity;

namespace DataTableConverter.Assisstant
{
    internal static class XMLTransformer
    {
        internal static string SCHEMA_DIRECTORY = Path.Combine(ExportHelper.ProjectPath, "XML-Schemas");

        internal static string[] GetSchemas() => Directory.GetFiles(SCHEMA_DIRECTORY);

        internal static string SelectFile(Form1 form1)
        {
            string fileName = string.Empty;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "XML Dateien|*.xml|Alle Dateien (*.*)|*.*",
                RestoreDirectory = true,
            };
            form1.Invoke(new MethodInvoker(() =>
            {
                if (dialog.ShowDialog(form1) == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
            }));
            return fileName;
        }

        internal static string Transform(Form1 invokeForm, string savePath, bool hidden = false)
        {

            string xmlDocumentPath = SelectFile(invokeForm);
            if (xmlDocumentPath == string.Empty) {
                return null;
            }
            string[] schemas = GetSchemas();

            if (schemas.Length == 0)
            {
                MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Warning, $"Kein XML Schema gefunden.\nBitte hinterlegen Sie eines unter {SCHEMA_DIRECTORY}");
                return null;
            }
            string selectedSchema = string.Empty;
            if(schemas.Length > 1)
            {
                Dictionary<string, string> namePathMapping = schemas.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => x);
                SelectHeader schemaForm = new SelectHeader(namePathMapping, "Auswahl des XML-Schemas", "XML-Schema");
                if(schemaForm.ShowDialog() == DialogResult.OK)
                {
                    selectedSchema = schemaForm.Column;
                } else
                {
                    return null;
                }
            } else
            {
                selectedSchema = schemas[0];
            }

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };

            using (XmlReader xsltReader = XmlReader.Create(selectedSchema))
            {
                var transformer = new XslCompiledTransform();
                transformer.Load(xsltReader);
                using (XmlReader oldDocumentReader = XmlReader.Create(xmlDocumentPath, settings))
                {
                    using (StreamWriter newDocumentWriter = new StreamWriter(savePath))
                    {
                        transformer.Transform(oldDocumentReader, null, newDocumentWriter);
                    }
                }
            }
            if (hidden)
            {
                File.SetAttributes(savePath, FileAttributes.Hidden);
            }
            return xmlDocumentPath;
        }
    }
}
