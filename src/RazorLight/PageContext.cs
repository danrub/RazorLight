using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace RazorLight
{
	public class PageContext : IPageContext
	{
		public PageContext()
		{
			ViewBag = new DefaultViewBag();
			Writer = new StringWriter();
		}

		public PageContext(IDynamicMetaObjectProvider viewBag)
		{
			ViewBag = viewBag ?? new DefaultViewBag();
			Writer = new StringWriter();
		}

		public TextWriter Writer { get; set; }

		public dynamic ViewBag { get; }

		public string ExecutingPageKey { get; set; }

		public ModelTypeInfo ModelTypeInfo { get; set; }

		public object Model { get; set; }


		private class DefaultViewBag : DynamicObject
		{
			private readonly IDictionary<string, object> bag = new Dictionary<string,object>();

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
}
