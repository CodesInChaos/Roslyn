using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Chaos.PureMethodStatement
{
	[ExportSyntaxNodeCodeIssueProvider("Chaos.PureMethodStatement", LanguageNames.CSharp)]
	class CodeIssueProvider : ICodeIssueProvider
	{
		static void LoadKnownPure()
		{
			if (knownPureFunctions != null)
				return;
			knownPureFunctions = new HashSet<string>();

			foreach (var s in ContractLoader.PureItemsFromCodeContractAssemblies())
			{
				knownPureFunctions.Add(s);
			}

			//System.IO.File.WriteAllLines(@"KnownPure.txt",knownPureFunctions.OrderBy(s => s).ToArray());
		}

		private readonly ICodeActionEditFactory editFactory;

		[ImportingConstructor]
		public CodeIssueProvider(ICodeActionEditFactory editFactory)
		{
			this.editFactory = editFactory;
		}

		string GetTypeNameCecilStyle(ITypeSymbol type)
		{
			return type.ContainingNamespace + "." + type.Name;
		}

		string GetMethodNameCecilStyle(IMethodSymbol symbol)
		{
			string result = "";
			result += GetTypeNameCecilStyle(symbol.ReturnType) + " ";

			result += GetTypeNameCecilStyle(symbol.ContainingType) + "::";
			result += symbol.Name + "(";
			result += string.Join(",", symbol.Parameters.Select(p => GetTypeNameCecilStyle(p.Type)));
			result += ")";
			return result;
		}

		public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
		{
			LoadKnownPure();
			var methodCall = node as InvocationExpressionSyntax;//Must be method call
			if (methodCall == null)
				yield break;

			if (!(methodCall.Parent is ExpressionStatementSyntax))//Method call must be the outermost in a statement
				yield break;

			var semanticModel = document.GetSemanticModel();
			var symbol = semanticModel.GetSemanticInfo(methodCall.Expression).Symbol as IMethodSymbol;

			if (symbol == null)//Symbol for method unavailable
				yield break;

			if (knownPureFunctions.Contains(GetMethodNameCecilStyle(symbol)) ||
				symbol.GetAttributes().Any(attr => attr.AttributeClass.Name == "PureAttribute"))
			{
				var issueDescription = string.Format("Call to pure method '{0}' used as a statement.", symbol);
				yield return new CodeIssue(CodeIssue.Severity.Warning, methodCall.Span, issueDescription);
			}

			var typeSymbol = symbol.ContainingType;

			if (typeSymbol.GetAttributes().Any(attr => attr.AttributeClass.Name == "PureAttribute"))
			{
				var issueDescription = string.Format("Call to method '{0}' on pure type '{1}' used as a statement.", symbol.Name, typeSymbol);
				yield return new CodeIssue(CodeIssue.Severity.Warning, methodCall.Span, issueDescription);
			}
		}

		static HashSet<string> knownPureFunctions;

		#region Unimplemented ICodeIssueProvider members

		public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxTrivia trivia, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
