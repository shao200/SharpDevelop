﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.SharpDevelop.Parser;

namespace ICSharpCode.SharpDevelop.Dom
{
	/// <summary>
	/// Observable model for a type definition.
	/// </summary>
	public interface ITypeDefinitionModel : IEntityModel
	{
		FullTypeName FullTypeName { get; }
		IModelCollection<ITypeDefinitionModel> NestedTypes { get; }
		IModelCollection<IMemberModel> Members { get; }
		
		/// <summary>
		/// Resolves the type definition in the current solution snapshot.
		/// Returns null if the type definition could not be resolved.
		/// </summary>
		new ITypeDefinition Resolve();
		
		/// <summary>
		/// Resolves the type definition in the specified solution snapshot.
		/// Returns null if the type definition could not be resolved.
		/// </summary>
		new ITypeDefinition Resolve(ISolutionSnapshotWithProjectMapping solutionSnapshot);
	}
}