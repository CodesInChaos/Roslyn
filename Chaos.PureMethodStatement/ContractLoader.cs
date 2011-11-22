using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Mono.Cecil;
using System.IO;

namespace Chaos.PureMethodStatement
{
	static class ContractLoader
	{
		static bool IsPure(IMemberDefinition member)
		{
			return member.CustomAttributes.Any(attr => attr.AttributeType.Name == ("PureAttribute"));
		}

		public static IEnumerable<string> PureItemsFromCodeContractAssemblies()
		{
			string installPath = Environment.GetEnvironmentVariable("CodeContractsInstallDir");
			if (installPath == null)
				yield break;

			foreach (string filename in Directory.GetFiles(installPath + @"Contracts\.NETFramework\v4.0\", "*.Contracts.dll"))
			{
				AssemblyDefinition sourceAssembly = AssemblyDefinition.ReadAssembly(filename);
				var types = sourceAssembly.Modules.SelectMany(module => module.Types);
				foreach (var type in types)
				{
					if (IsPure(type))
						yield return type.FullName;
					foreach (var method in type.Methods)
					{
						if (IsPure(method))
						{
							yield return method.FullName;
						}
					}
				}
			}
		}
	}
}
