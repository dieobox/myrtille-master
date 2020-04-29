/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2020 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Myrtille.Helpers
{
    public static class XmlTools
    {
        public static XmlNode GetNode(
            XmlNode parentNode,
            string name)
        {
            XmlNode theNode = null;

            if ((parentNode != null) &&
                (parentNode.ChildNodes != null) &&
                (parentNode.ChildNodes.Count > 0))
            {
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if (node.Name.ToUpper().Equals(name.ToUpper()))
                    {
                        theNode = node;
                        break;
                    }
                }
            }

            return theNode;
        }

        public static XmlNode GetNode(
            XPathNavigator navigator,
            string path)
        {
            XmlNode node = null;

            var iterator = navigator.Select(path);
            if (iterator.Count == 1)
            {
                iterator.MoveNext();
                node = ((IHasXmlNode)iterator.Current).GetNode();
            }

            return node;
        }

        public static string ReadConfigKey(
            XmlNode parentNode,
            string key)
        {
            if ((parentNode != null) &&
                (parentNode.ChildNodes != null) &&
                (parentNode.ChildNodes.Count > 0))
            {
                XmlNode theNode = null;

                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if ((node.Name.ToUpper().Equals("ADD")) &&
                        (node.Attributes != null) &&
                        (node.Attributes["key"] != null) &&
                        (node.Attributes["key"].Value.ToUpper().Equals(key.ToUpper())))
                    {
                        theNode = node;
                        break;
                    }
                }

                if ((theNode != null) &&
                    (theNode.Attributes != null) &&
                    (theNode.Attributes["value"] != null))
                {
                    var theNodeValue = theNode.Attributes["value"];
                    return theNodeValue.Value;
                }
            }

            return null;
        }

        public static void WriteConfigKey(
            XmlNode parentNode,
            string key,
            string value)
        {
            if ((parentNode != null) &&
                (parentNode.ChildNodes != null) &&
                (parentNode.ChildNodes.Count > 0))
            {
                XmlNode theNode = null;

                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if ((node.Name.ToUpper().Equals("ADD")) &&
                        (node.Attributes != null) &&
                        (node.Attributes["key"] != null) &&
                        (node.Attributes["key"].Value.ToUpper().Equals(key.ToUpper())))
                    {
                        theNode = node;
                        break;
                    }
                }

                if ((theNode != null) &&
                    (theNode.Attributes != null) &&
                    (theNode.Attributes["value"] != null))
                {
                    var theNodeValue = theNode.Attributes["value"];
                    theNodeValue.Value = value;
                }
            }
        }

        public static void CommentNode(
            XmlDocument document,
            XmlNode parentNode,
            XmlNode node)
        {
            if (node is XmlComment)
                return;

            try
            {
                var commentContent = node.OuterXml;
                var commentNode = document.CreateComment(commentContent);
                parentNode.ReplaceChild(commentNode, node);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to comment xml node ({0}))", exc);
                throw;
            }
        }

        public static void UncommentNode(
            XmlDocument document,
            XmlNode parentNode,
            XmlNode node)
        {
            if (!(node is XmlComment))
                return;

            try
            {                
                var nodeReader = XmlReader.Create(new StringReader(node.Value));
                var uncommentNode = document.ReadNode(nodeReader);
                parentNode.ReplaceChild(uncommentNode, node);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to uncomment xml node ({0}))", exc);
                throw;
            }
        }

        public static void CommentConfigKey(
            XmlDocument document,
            XmlNode parentNode,
            string key)
        {
            if ((parentNode != null) &&
                (parentNode.ChildNodes != null) &&
                (parentNode.ChildNodes.Count > 0))
            {
                XmlNode theNode = null;

                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if ((node.Name.ToUpper().Equals("ADD")) &&
                        (node.Attributes != null) &&
                        (node.Attributes["key"] != null) &&
                        (node.Attributes["key"].Value.ToUpper().Equals(key.ToUpper())))
                    {
                        theNode = node;
                        break;
                    }
                }

                if (theNode != null)
                {
                    CommentNode(document, parentNode, theNode);
                }
            }
        }

        public static void UncommentConfigKey(
            XmlDocument document,
            XmlNode parentNode,
            string key)
        {
            if ((parentNode != null) &&
                (parentNode.ChildNodes != null) &&
                (parentNode.ChildNodes.Count > 0))
            {
                XmlNode theNode = null;

                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if ((node is XmlComment) &&
                        (node.Value.ToUpper().StartsWith(string.Format("<ADD KEY=\"{0}\"", key.ToUpper()))))
                    {
                        theNode = node;
                        break;
                    }
                }

                if (theNode != null)
                {
                    UncommentNode(document, parentNode, theNode);
                }
            }
        }
    }
}