using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace PermissionManagement.IoC
{
    #region "MailSettingsSectionHandler"

    /// <summary>
    /// This class implements IConfigurationSectionHandler and allows use of user-defined XML 
    /// nodes inside the Web.Config file.  Specfically, it allows the appSettings section
    /// to be read.
    /// </summary>
     public class MunqConfigurationReaderSectionHandler : IConfigurationSectionHandler
    {

        #region "IConfigurationSectionHandler Members"

        /// <summary>
        /// Create a new BaseSettings object populates with data taken from the "mailSettings" node 
        /// in the config file.
        ///	</summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section">The XML section we will iterate against</param>
        /// <returns></returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            MunqConfigurationReader r = new MunqConfigurationReader(section);
            return r;
        }

        #endregion

    }

    #endregion

    public sealed class MunqConfigurationReader
    {

        public const string ConfigSectionName = "munqConfiguration";
        #region "getcurrent"

        /// <summary>
        /// A factory method used to create an instance of the settings.
        /// </summary>
        /// <returns></returns>
        public static MunqConfigurationReader GetCurrent()
        {
            var settings = (MunqConfigurationReader)ConfigurationManager.GetSection(MunqConfigurationReader.ConfigSectionName);
            if (settings == null)
            {
                throw new Exception("Config file does not appear to have an entry for the munqConfiguration node");
            }
            return settings;
        }

        #endregion

        #region "constructor"

        /// <summary>
        /// Simple constructor
        /// </summary>
        public MunqConfigurationReader()
        {
        }

        /// <summary>
        /// Overloaded constructor that is given the Settings node from the Config file and 
        /// populates the settings object.
        /// </summary>
        /// <param name="node"></param>
        public MunqConfigurationReader(XmlNode node)
        {
            string message = "'{0}' attribute not found in munqConfiguration section of Web.config";

            // try to find a node with the name of the server.
            var serverName = Environment.MachineName.ToLower();
            var serverNode = node.SelectSingleNode(serverName);

            // if no server node exists, look for a "default" node.
            if (serverNode == null)
            {
                serverNode = node.SelectSingleNode("default");
            }

            RegisteredTypeList = new List<MunqRegisteredType>();

            var typesNode = serverNode.SelectSingleNode("types");

            foreach (XmlNode typeNode in typesNode.ChildNodes)
            {
                foreach (string item in new string[] { "type", "mapTo" })
                {
                    if (typeNode.Attributes[item] == null || string.IsNullOrEmpty(typeNode.Attributes[item].Value.Trim()))
                    {
                        throw new Exception(string.Format(message, item));
                    }
                }
                var newType = new MunqRegisteredType();
                newType.HeaderInfo = new MunqRegisteredHeader();
                newType.HeaderInfo.TypeName = typeNode.Attributes["type"].Value;
                newType.HeaderInfo.MapTo = typeNode.Attributes["mapTo"].Value; ;

                var parameterNodes = typeNode.SelectSingleNode("constructor");
                var parameterList = new List<MunqRegisteredTypeParameter>();
                foreach (XmlNode parameterNode in parameterNodes.ChildNodes)
                {
                    foreach (string item in new string[] { "name", "parameterType", "dependencyname" })
                    {
                        if (parameterNode.Attributes[item] == null || string.IsNullOrEmpty(parameterNode.Attributes[item].Value.Trim()))
                        {
                            throw new Exception(string.Format(message, item));
                        }
                    }
                    var newParameter = new MunqRegisteredTypeParameter();
                    newParameter.ParameterName = parameterNode.Attributes["name"].Value;
                    newParameter.ParameterType = parameterNode.Attributes["parameterType"].Value;
                    newParameter.DependencyName = parameterNode.Attributes["dependencyname"].Value;
                    parameterList.Add(newParameter);
                    newType.ParameterList = parameterList;
                }

                RegisteredTypeList.Add(newType);
            }
        }

        #endregion

        #region "properties"

        public IList<MunqRegisteredType> RegisteredTypeList { get; set; }

        #endregion
    }

    #region "type class"

    public class MunqRegisteredType
    {
        public MunqRegisteredHeader HeaderInfo { get; set; }
        public IList<MunqRegisteredTypeParameter> ParameterList { get; set; }
    }

    #endregion

    #region "type main properties"

    public class MunqRegisteredHeader
    {
        public string TypeName { get; set; }
        public string MapTo { get; set; }
    }

    #endregion

    #region "type constructor"

    public class MunqRegisteredTypeParameter
    {
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
        public string DependencyName { get; set; }
    }

    #endregion
}
