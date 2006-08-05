// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Andrea Paatz" email="andrea@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Collections;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
//using ICSharpCode.NRefactory.Parser;

namespace VBNetBinding.Parser
{
	public class TParser : IParser
	{
		///<summary>IParser Interface</summary>
		string[] lexerTags;
		
		public string[] LexerTags {
			get {
				return lexerTags;
			}
			set {
				lexerTags = value;
			}
		}
		
		public LanguageProperties Language {
			get {
				return LanguageProperties.VBNet;
			}
		}
		
		public IExpressionFinder CreateExpressionFinder(string fileName)
		{
			return new ExpressionFinder();
		}
		
		public bool CanParse(string fileName)
		{
			return Path.GetExtension(fileName).Equals(".VB", StringComparison.OrdinalIgnoreCase);
		}
		
		public bool CanParse(IDomProject project)
		{
			return project.Language == "VBNet";
		}
		
		void RetrieveRegions(ICompilationUnit cu, ICSharpCode.NRefactory.Parser.SpecialTracker tracker)
		{
			for (int i = 0; i < tracker.CurrentSpecials.Count; ++i)
			{
				ICSharpCode.NRefactory.PreprocessingDirective directive = tracker.CurrentSpecials[i] as ICSharpCode.NRefactory.PreprocessingDirective;
				if (directive != null)
				{
					if (directive.Cmd.Equals("#region", StringComparison.OrdinalIgnoreCase))
					{
						int deep = 1;
						for (int j = i + 1; j < tracker.CurrentSpecials.Count; ++j)
						{
							ICSharpCode.NRefactory.PreprocessingDirective nextDirective = tracker.CurrentSpecials[j] as ICSharpCode.NRefactory.PreprocessingDirective;
							if (nextDirective != null)
							{
								switch (nextDirective.Cmd.ToLowerInvariant())
								{
									case "#region":
										++deep;
										break;
									case "#end":
										if (nextDirective.Arg.Equals("region", StringComparison.OrdinalIgnoreCase)) {
											--deep;
											if (deep == 0) {
												cu.FoldingRegions.Add(new FoldingRegion(directive.Arg.Trim('"'), new DomRegion(directive.StartPosition, nextDirective.EndPosition)));
												goto end;
											}
										}
										break;
								}
							}
						}
						end: ;
					}
				}
			}
		}
		
		public ICompilationUnit Parse(IProjectContent projectContent, string fileName, string fileContent)
		{
			using (ICSharpCode.NRefactory.IParser p = ICSharpCode.NRefactory.ParserFactory.CreateParser(ICSharpCode.NRefactory.SupportedLanguage.VBNet, new StringReader(fileContent))) {
				return Parse(p, fileName, projectContent);
			}
		}
		
		ICompilationUnit Parse(ICSharpCode.NRefactory.IParser p, string fileName, IProjectContent projectContent)
		{
			p.Lexer.SpecialCommentTags = lexerTags;
			p.ParseMethodBodies = false;
			p.Parse();
			
			NRefactoryASTConvertVisitor visitor = new NRefactoryASTConvertVisitor(projectContent);
			visitor.Specials = p.Lexer.SpecialTracker.CurrentSpecials;
			visitor.VisitCompilationUnit(p.CompilationUnit, null);
			visitor.Cu.FileName = fileName;
			visitor.Cu.ErrorsDuringCompile = p.Errors.Count > 0;
			RetrieveRegions(visitor.Cu, p.Lexer.SpecialTracker);
			AddCommentTags(visitor.Cu, p.Lexer.TagComments);
			
			string rootNamespace = null;
			ParseProjectContent ppc = projectContent as ParseProjectContent;
			if (ppc != null) {
				rootNamespace = ppc.Project.RootNamespace;
			}
			if (rootNamespace != null && rootNamespace.Length > 0) {
				foreach (IClass c in visitor.Cu.Classes) {
					c.FullyQualifiedName = rootNamespace + "." + c.FullyQualifiedName;
				}
			}
			
			return visitor.Cu;
		}
		
		void AddCommentTags(ICompilationUnit cu, System.Collections.Generic.List<ICSharpCode.NRefactory.Parser.TagComment> tagComments)
		{
			foreach (ICSharpCode.NRefactory.Parser.TagComment tagComment in tagComments)
			{
				DomRegion tagRegion = new DomRegion(tagComment.StartPosition.Y, tagComment.StartPosition.X);
				ICSharpCode.SharpDevelop.Dom.TagComment tag = new ICSharpCode.SharpDevelop.Dom.TagComment(tagComment.Tag, tagRegion);
				tag.CommentString = tagComment.CommentText;
				cu.TagComments.Add(tag);
			}
		}
		
		public IResolver CreateResolver()
		{
			return new ICSharpCode.SharpDevelop.Dom.NRefactoryResolver.NRefactoryResolver(ParserService.CurrentProjectContent);
		}
		///////// IParser Interface END
	}
}
