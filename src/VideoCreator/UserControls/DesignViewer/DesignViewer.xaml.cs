using Sqllite_Library.Models;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace DesignViewerNp.controls
{
    /// <summary>
    /// Interaction logic for DesignViewer.xaml
    /// </summary>
    public partial class DesignViewer : UserControl
    {
        public DesignViewer()
        {
            InitializeComponent();
        }

        public void LoadDesign(DataTable dataTable)
        {
            container.Children.Clear();

            foreach (DataRow dataRow in dataTable.Rows)
            {
                AddElement((string)dataRow["xaml"]);
            }
        }

        public void LoadDesignForEdit(CBVDesign design)
        {
            if (container.Children.Count > 0)
                container.Children.Clear();

            if (null != design)
            {
                XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
                xmlDoc.LoadXml($"<root>{design.design_xml}</root>");
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    string text = node.OuterXml; //or loop through its children as well
                    if (text.ToLower().StartsWith("<image"))
                        continue;
                    AddElement(text);
                }
                //XmlReader rdr = XmlReader.Create(new System.IO.StringReader($"<root>{design.design_xml}</root>"));
                //while (rdr.Read())
                //{
                //    if (rdr.NodeType == XmlNodeType.Element && rdr.Depth == 1)
                //    {
                //        Console.WriteLine(rdr.LocalName);
                //        Console.WriteLine(rdr.ReadInnerXml());
                //        Console.WriteLine("---------------------------");
                //    }
                //}
                //var objects = design.design_xml?.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                //foreach (var item in objects)
                //{
                //    if (item.StartsWith("<Image"))
                //        continue;
                //    AddElement(item);
                //}
            }
        }

        private void AddElement(string xaml)
        {
            string canvasStart = @"<Canvas 
                                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string canvasEnd = "</Canvas>";
            string wrapped = canvasStart + xaml + canvasEnd;

            StringReader stringReader = new StringReader(wrapped);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Canvas canvas = (Canvas)XamlReader.Load(xmlReader);

            if (canvas.Children.Count == 1)
            {
                UIElement element = canvas.Children[0];
                canvas.Children.RemoveAt(0);
                container.Children.Add(element);
            }
            else
            {
                while (canvas.Children.Count > 0)
                {
                    UIElement element = canvas.Children[0];
                    canvas.Children.RemoveAt(0);
                    container.Children.Add(element);
                }
            }
        }
    }
}
