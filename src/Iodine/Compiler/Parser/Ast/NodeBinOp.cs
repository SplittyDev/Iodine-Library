﻿using System;

namespace Iodine.Compiler.Ast
{
	public class NodeBinOp : AstNode
	{
		public BinaryOperation Operation {
			private set;
			get;
		}

		public AstNode Left {
			get {
				return this.Children[0];
			}
		}

		public AstNode Right {
			get {
				return this.Children[1];
			}
		}

		public NodeBinOp (Location location, BinaryOperation op, AstNode left, AstNode right)
			: base (location)
		{
			this.Operation = op;
			this.Add (left);
			this.Add (right);
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}
	}
}
