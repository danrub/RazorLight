using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RazorLight.Extensions
{
	public static class TypeExtensions
	{
		public static DefaultViewBag ToExpando(this object anonymousObject)
		{
			if (anonymousObject is IDictionary<string, object> exp)
			{
				return new DefaultViewBag(exp);
			}

			IDictionary<string, object> expando = new Dictionary<string, object>();
			foreach (var propertyDescriptor in anonymousObject.GetType().GetTypeInfo().GetProperties())
			{
				var obj = propertyDescriptor.GetValue(anonymousObject);
				if (obj != null && obj.GetType().IsAnonymousType())
				{
					obj = obj.ToExpando();
				}
				expando.Add(propertyDescriptor.Name, obj);
			}

			return new DefaultViewBag(expando);
		}

		public static bool IsAnonymousType(this Type type)
		{
			bool hasCompilerGeneratedAttribute = type.GetTypeInfo()
				.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false)
				.Any();

			bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
			bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

			return isAnonymousType;
		}

	}
}
