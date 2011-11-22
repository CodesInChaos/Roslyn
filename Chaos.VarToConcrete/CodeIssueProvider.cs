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

namespace Chaos.VarToConcrete
{
	[ExportSyntaxNodeCodeIssueProvider("Chaos.VarToConcrete", LanguageNames.CSharp, typeof(TypeSyntax))]
	class CodeIssueProvider : ICodeIssueProvider
	{
		private readonly ICodeActionEditFactory editFactory;

		[ImportingConstructor]
		public CodeIssueProvider(ICodeActionEditFactory editFactory)
		{
			this.editFactory = editFactory;
		}

		public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
		{
			var typeSyntax = (TypeSyntax)node;
			if (!typeSyntax.IsVar)
				yield break;

			ISemanticModel semanticModel = document.GetSemanticModel();
			ITypeSymbol variableType = semanticModel.GetSemanticInfo(typeSyntax).Type;

			if (variableType is ErrorTypeSymbol)
				yield break;

			yield return new CodeIssue(CodeIssue.Severity.Info, typeSyntax.Span, new ICodeAction[] { new ReplaceVarByConcreteAction(editFactory, document, typeSyntax) });
		}

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
