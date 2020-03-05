using RazorLight.Extensions;
using RazorLight.Tests.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using Xunit;

namespace RazorLight.Tests.Extensions
{
	public class TypeExtensionsTest
	{
		[Fact]
		public void ToExpando_Returns_ExpandoObject_If_Passed()
		{
			var expando = new DefaultViewBag();

			var obj = expando.ToExpando();

			Console.WriteLine(obj.GetType() == typeof(DefaultViewBag));
		}

		[Fact]
		public void ToExpando_Returns_All_Properties_Of_AnonymousObject()
		{
			var obj = new
			{
				Name = "Test",
				Age = 18
			};

			ReadOnlyDictionary<string, object> expando = obj.ToExpando();

			Assert.True(expando.ContainsKey("Name"));
			Assert.True(expando.ContainsKey("Age"));

			Assert.Equal("Test", expando["Name"]);
			Assert.Equal(18, Convert.ToInt32(expando["Age"]));
		}

		[Fact]
		public void Returns_True_For_Anynymous_Objects()
		{
			var obj = new { Name = "Test" };

			Assert.True(TypeExtensions.IsAnonymousType(obj.GetType()));
		}

		[Fact]
		public void Returns_False_For_Strong_Types()
		{
			Assert.False(TypeExtensions.IsAnonymousType(typeof(TestViewModel)));
		}
	}
}
