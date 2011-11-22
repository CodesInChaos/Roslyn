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
		private VariableDeclarationSyntax declaration;
		private ICodeActionEditFactory editFactory;
		private string typeName;

		public ReplaceVarByConcreteAction(ICodeActionEditFactory editFactory, IDocument document, VariableDeclarationSyntax declaration)
		{
			this.editFactory = editFactory;
			this.document = document;
			this.declaration = declaration;

			var semanticModel = (SemanticModel)document.GetSemanticModel();
			var syntaxTree = (SyntaxTree)document.GetSyntaxTree();

			ILocation location = syntaxTree.GetLocation(declaration.Type);
			var variable = declaration.Variables.Single();
			ITypeSymbol variableType = semanticModel.GetSemanticInfo(declaration.Type).Type;
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
										  declaration.Type.GetLeadingTrivia())
									 .WithTrailingTrivia(
										 declaration.Type.GetTrailingTrivia());

			return editFactory.CreateTreeTransformEdit(document.Project.Solution, syntaxTree, syntaxTree.Root.ReplaceNode(declaration.Type, newDeclarationType));
		}

		public System.Windows.Media.ImageSource Icon
		{
			get { return null; }
		}
	}
}
