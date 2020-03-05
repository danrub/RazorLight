using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using RazorLight.Caching;
using RazorLight.Compilation;

namespace RazorLight
{
	public interface IEngineHandler
	{
		ICachingProvider Cache { get; }
		IRazorTemplateCompiler Compiler { get; }
		ITemplateFactoryProvider FactoryProvider { get; }

		RazorLightOptions Options { get; }
		bool IsCachingEnabled { get; }

		Task<ITemplatePage> CompileTemplateAsync(string key);

		Task<string> CompileRenderAsync<T>(string key, T model, IDynamicMetaObjectProvider viewBag = null);
		Task<string> CompileRenderStringAsync<T>(string key, string content, T model, IDynamicMetaObjectProvider viewBag = null);

		Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, IDynamicMetaObjectProvider viewBag = null);
		Task RenderTemplateAsync<T>(ITemplatePage templatePage, T model, TextWriter textWriter, IDynamicMetaObjectProvider viewBag = null);
		Task RenderIncludedTemplateAsync<T>(ITemplatePage templatePage, T model, TextWriter textWriter, IDynamicMetaObjectProvider viewBag, TemplateRenderer templateRenderer);
	}
}