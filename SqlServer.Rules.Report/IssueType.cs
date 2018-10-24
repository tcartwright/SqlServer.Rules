using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlServer.Rules.Report
{
    [Serializable]
    public class IssueType
    {
        [XmlAttribute]
        public string Severity { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public string Category { get; set; }

        [XmlAttribute]
        public string Id { get; set; }
    }

    public class IssueTypeComparer : IEqualityComparer<IssueType>
    {
        public bool Equals(IssueType x, IssueType y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(IssueType obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}