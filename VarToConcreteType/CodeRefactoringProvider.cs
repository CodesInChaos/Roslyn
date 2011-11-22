using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace VarToConcreteType
{
	[ExportCodeRefactoringProvider("VarToConcreteType", LanguageNames.CSharp)]
	class CodeRefactoringProvider : ICodeRefactoringProvider
	{
		private readonly ICodeActionEditFactory editFactory;

		[ImportingConstructor]
		public CodeRefactoringProvider(ICodeActionEditFactory editFactory)
		{
			this.editFactory = editFactory;
		}

		public CodeRefactoring GetRefactoring(IDocument document, TextSpan textSpan, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
