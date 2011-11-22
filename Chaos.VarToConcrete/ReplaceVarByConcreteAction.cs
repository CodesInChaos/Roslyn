using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services.Editor;
using System.Threading;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;

namespace Chaos.VarToConcrete
{
	public class ReplaceVarByConcreteAction : ICodeAction
	{
		private IDocument document;
		private TypeSyntax typeSyntax;
		private ICodeActionEditFactory editFactory;
		private string typeName;

		public ReplaceVarByConcreteAction(ICodeActionEditFactory editFactory, IDocument document, TypeSyntax typeSyntax)
		{
			this.editFactory = editFactory;
			this.document = document;
			this.typeSyntax = typeSyntax;

			var semanticModel = (SemanticModel)document.GetSemanticModel();
			var syntaxTree = (SyntaxTree)document.GetSyntaxTree();

			ILocation location = syntaxTree.GetLocation(typeSyntax);

			ITypeSymbol variableType = semanticModel.GetSemanticInfo(typeSyntax).Type;
			this.typeName = variableType.ToMinimalDisplayString((Location)location, semanticModel);
		}
		public string Description
		{
			get { return string.Format("Replace with concrete type ({0})", typeName); }
		}

		public ICodeActionEdit GetEdit(CancellationToken cancellationToken)
		{
			var syntaxTree = (SyntaxTree)document.GetSyntaxTree();

			TypeSyntax newDeclarationType =
							   Syntax.IdentifierName(typeName)
									 .WithLeadingTrivia(
										  typeSyntax.GetLeadingTrivia())
									 .WithTrailingTrivia(
										 typeSyntax.GetTrailingTrivia());

			return editFactory.CreateTreeTransformEdit(document.Project.Solution, syntaxTree, syntaxTree.Root.ReplaceNode(typeSyntax, newDeclarationType));
		}

		public System.Windows.Media.ImageSource Icon
		{
			get { return null; }
		}
	}
}
