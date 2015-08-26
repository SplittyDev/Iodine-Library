﻿/**
  * Copyright (c) 2015, GruntTheDivine All rights reserved.

  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  * 
  *  * Redistributions of source code must retain the above copyright notice, this list
  *    of conditions and the following disclaimer.
  * 
  *  * Redistributions in binary form must reproduce the above copyright notice, this
  *    list of conditions and the following disclaimer in the documentation and/or
  *    other materials provided with the distribution.

  * Neither the name of the copyright holder nor the names of its contributors may be
  * used to endorse or promote products derived from this software without specific
  * prior written permission.
  * 
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
  * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
  * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
  * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
  * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
  * BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
  * CONTRACT ,STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
  * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
  * DAMAGE.
**/

using System;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public class EnumDeclaration : AstNode
	{
		public string Name {
			private set;
			get;
		}

		public Dictionary<string, int> Items {
			private set;
			get;
		}

		public EnumDeclaration (Location location, string name)
			: base (location)
		{
			this.Name = name;
			this.Items = new Dictionary<string, int> ();
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "enum");
			string name = stream.Expect (TokenClass.Identifier).Value;
			EnumDeclaration decl = new EnumDeclaration (stream.Location, name);

			stream.Expect (TokenClass.OpenBrace);
			int defaultVal = -1;

			while (!stream.Match (TokenClass.CloseBrace)) {
				string ident = stream.Expect (TokenClass.Identifier).Value;
				if (stream.Accept (TokenClass.Operator, "=")) {
					string val = stream.Expect (TokenClass.IntLiteral).Value;
					int numVal = 0;
					if (val != "") {
						numVal = Int32.Parse (val);
					}
					decl.Items [ident] = numVal;
				} else {
					decl.Items [ident] = defaultVal--;
				}
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}

			stream.Expect (TokenClass.CloseBrace);

			return decl;
		}
	}
}

