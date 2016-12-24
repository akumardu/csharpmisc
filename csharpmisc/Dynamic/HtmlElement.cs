using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.Dynamic
{
    public class HtmlElement : DynamicObject
    {
        private readonly Dictionary<string, object> attributes = new Dictionary<string, object>();

        public string TagName { get; set; }

        public HtmlElement(string tagName)
        {
            TagName = tagName;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string key = binder.Name;
            attributes[key] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return attributes.TryGetValue(binder.Name, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return attributes.Keys.ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"<{TagName} ");
            foreach(var attrib in attributes)
            {
                sb.Append($"{attrib.Key}='{attrib.Value}' ");
            }

            sb.Append("/>");
            return sb.ToString();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if(binder.Name == "Render")
            {
                result = ToString();
                return true;
            }

            result = null;
            return false;
        }
    }
}
