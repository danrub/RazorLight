using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace RazorLight.Compilation
{
	public class DefaultMetadataReferenceManager : IMetadataReferenceManager
	{
		public HashSet<MetadataReference> AdditionalMetadataReferences { get; }
		public HashSet<string> ExcludedAssemblies { get; }
		public ICompilationAssemblyResolver CustomCompilationAssemblyResolver { get; }

		public DefaultMetadataReferenceManager()
		{
			AdditionalMetadataReferences = new HashSet<MetadataReference>();
			ExcludedAssemblies = new HashSet<string>();
		}

		public DefaultMetadataReferenceManager(HashSet<MetadataReference> metadataReferences)
		{
			AdditionalMetadataReferences = metadataReferences ?? throw new ArgumentNullException(nameof(metadataReferences));
			ExcludedAssemblies = new HashSet<string>();
		}

		public DefaultMetadataReferenceManager(HashSet<MetadataReference> metadataReferences, HashSet<string> excludedAssemblies, IEnumerable<ICompilationAssemblyResolver> customResolvers = default)
		{
			AdditionalMetadataReferences = metadataReferences ?? throw new ArgumentNullException(nameof(metadataReferences));
			ExcludedAssemblies = excludedAssemblies ?? throw new ArgumentNullException(nameof(excludedAssemblies));
			CustomCompilationAssemblyResolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]
														{
																new AppBaseCompilationAssemblyResolver(),
																new ReferenceAssemblyPathResolver(),
																new PackageCompilationAssemblyResolver()
														}
														.Concat(customResolvers ?? Enumerable.Empty<ICompilationAssemblyResolver>())
														.ToArray());
		}

		public IReadOnlyList<MetadataReference> Resolve(Assembly assembly)
		{
			DependencyContext dependencyContext = DependencyContext.Load(assembly);

			return Resolve(assembly, dependencyContext);
		}

		internal IReadOnlyList<MetadataReference> Resolve(Assembly assembly, DependencyContext dependencyContext)
		{
			HashSet<string> libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IEnumerable<string> references = null;
			if (dependencyContext == null)
			{
				HashSet<string> context = new HashSet<string>();
				Assembly[] x = GetReferencedAssemblies(assembly, ExcludedAssemblies, context).Union(new Assembly[] { assembly }).ToArray();
				references = x.Select(p => AssemblyDirectory(p));
			}
			else
			{
				references = dependencyContext.CompileLibraries.SelectMany(library => CustomCompilationAssemblyResolver is null ? library.ResolveReferencePaths() : library.ResolveReferencePaths(CustomCompilationAssemblyResolver));

				if (!references.Any())
				{
					throw new RazorLightException("Can't load metadata reference from the entry assembly. " +
												  "Make sure PreserveCompilationContext is set to true in *.csproj file");
				}
			}

			List<MetadataReference> metadataRerefences = new List<MetadataReference>();

			foreach (string reference in references)
			{
				if (libraryPaths.Add(reference))
				{
					using (FileStream stream = File.OpenRead(reference))
					{
						ModuleMetadata moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
						AssemblyMetadata assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

						metadataRerefences.Add(assemblyMetadata.GetReference(filePath: reference));
					}
				}
			}

			if (AdditionalMetadataReferences.Any())
			{
				metadataRerefences.AddRange(AdditionalMetadataReferences);
			}

			return metadataRerefences;
		}

		private static IEnumerable<Assembly> GetReferencedAssemblies(Assembly a, IEnumerable<string> excludedAssemblies, HashSet<string> visitedAssemblies = null)
		{
			visitedAssemblies = visitedAssemblies ?? new HashSet<string>();
			if (!visitedAssemblies.Add(a.GetName().EscapedCodeBase))
			{
				yield break;
			}

			foreach (AssemblyName assemblyRef in a.GetReferencedAssemblies())
			{
				if (visitedAssemblies.Contains(assemblyRef.EscapedCodeBase)) { continue; }

				if (excludedAssemblies.Any(s => s.Contains(assemblyRef.Name))) { continue; }
				Assembly loadedAssembly = Assembly.Load(assemblyRef);
				yield return loadedAssembly;
				foreach (Assembly referenced in GetReferencedAssemblies(loadedAssembly, excludedAssemblies, visitedAssemblies))
				{
					yield return referenced;
				}

			}
		}

		private static string AssemblyDirectory(Assembly assembly) => assembly.Location;
	}
}
