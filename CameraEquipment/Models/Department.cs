using System.Xml;
using ToolSetsCore;

namespace CameraEquipment.Models
{
    public class Department
    {
        public Department(XmlNode node)
        {
            name = node.GetAttribte("name");
        }

        public string name { get; set; }
    }
}