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
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Iodine.Compiler.Ast
{
	public class UseStatement : AstNode
	{
		public string Module {
			private set;
			get;
		}

		public List<string> Imports {
			private set;
			get;
		}

		public bool Wildcard {
			private set;
			get;
		}

		public bool Relative {
			private set;
			get;
		}

		public UseStatement (Location location, string module, bool relative = false)
			: base (location)
		{
			Module = module;
			Relative = relative;
			Imports = new List<string> ();
		}

		public UseStatement (Location location,string module, List<string> imports, bool wildcard,
		                         bool relative = false)
			: base (location)
		{
			Module = module;
			Imports = imports;
			Wildcard = wildcard;
			Relative = relative;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static UseStatement Parse (TokenStream stream)
		{
			stream.Expect (TokenClass.Keyword, "use");
			bool relative = stream.Accept (TokenClass.Operator, ".");
			string ident = "";
			if (!stream.Match (TokenClass.Operator, "*"))
				ident = ParseModuleName (stream);
			if (stream.Match (TokenClass.Keyword, "from") || stream.Match (TokenClass.Comma) ||
			    stream.Match (TokenClass.Operator, "*")) {
				List<string> items = new List<string> ();
				bool wildcard = false;
				if (!stream.Accept (TokenClass.Operator, "*")) {
					items.Add (ident);
					stream.Accept (TokenClass.Comma);
					while (!stream.Match (TokenClass.Keyword, "from")) {
						Token item = stream.Expect (TokenClass.Identifier);
						items.Add (item.Value);
						if (!stream.Accept (TokenClass.Comma)) {
							break;
						}
					}
				} else {
					wildcard = true;
				}
				stream.Expect (TokenClass.Keyword, "from");

				relative = stream.Accept (TokenClass.Operator, ".");
				string module = ParseModuleName (stream);
				return new UseStatement (stream.Location, module, items, wildcard, relative);
			}
			return new UseStatement (stream.Location, ident, relative);
		}

		private static string ParseModuleName (TokenStream stream)
		{
			Token initIdent = stream.Expect (TokenClass.Identifier);

			if (stream.Match (TokenClass.Operator, ".")) {
				StringBuilder accum = new StringBuilder ();
				accum.Append (initIdent.Value);
				while (stream.Accept (TokenClass.Operator, ".")) {
					Token ident = stream.Expect (TokenClass.Identifier);
					accum.Append (Path.DirectorySeparatorChar);
					accum.Append (ident.Value);
				}
				return accum.ToString ();

			} else {
				return initIdent.Value;
			}
		}
	}
}

