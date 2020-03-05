using System.Collections.Generic;
using System.Dynamic;

namespace RazorLight
{
	public class DefaultViewBag : DynamicObject
	{
		private readonly IDictionary<string, object> bag;

		public DefaultViewBag(IDictionary<string, object> inherit = default)
		{
			bag = inherit ?? new Dictionary<string, object>();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			bag.TryGetValue(binder.Name, out result);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			bag[binder.Name] = value;
			return true;
		}
	}
}
