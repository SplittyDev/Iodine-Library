﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public abstract class AstNode : IEnumerable<AstNode>
	{
		private List<AstNode> children = new List<AstNode> ();

		public Location Location {
			private set;
			get;
		}

		public IList<AstNode> Children {
			get {
				return this.children;
			}
		}
			
		public abstract void Visit (IAstVisitor visitor);

		public AstNode (Location location) {
			this.Location = location;
		}

		public void Add (AstNode node)
		{
			this.children.Add (node);
		}

		public IEnumerator<AstNode> GetEnumerator ()
		{
			foreach (AstNode node in this.children) {
				yield return node;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}

	public class AstRoot : AstNode
	{
		public AstRoot (Location location)
			: base (location)
		{
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstRoot Parse (TokenStream inputStream)
		{
			AstRoot root = new AstRoot (inputStream.Location);
			while (!inputStream.EndOfStream) {
				root.Add (NodeStmt.Parse (inputStream));
			}
			return root;
		}
	}
}
