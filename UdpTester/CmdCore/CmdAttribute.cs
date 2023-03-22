using System;

namespace UdpTester.CmdCore
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CmdAttribute : Attribute
    {
        public CmdAttribute(string name, Type defineType)
        {
            Name = name;
            DefineType = defineType;
        }

        public string Name { get; set; }
        public Type DefineType { get; set; }
    }
}