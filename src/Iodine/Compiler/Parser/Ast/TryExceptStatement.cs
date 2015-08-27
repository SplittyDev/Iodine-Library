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

namespace Iodine.Compiler.Ast
{
	public class TryExceptStatement : AstNode
	{
		public string ExceptionIdentifier {
			private set;
			get;
		}

		public AstNode TryBody {
			get {
				return Children [0];
			}
		}

		public AstNode ExceptBody {
			get {
				return Children [1];
			}
		}

		public AstNode TypeList {
			get {
				return Children [2];
			}
		}

		public TryExceptStatement (Location location, string ident)
			: base (location)
		{
			this.ExceptionIdentifier = ident;
		}

		public override void Visit (IAstVisitor visitor)
		{
			visitor.Accept (this);
		}

		public static AstNode Parse (TokenStream stream)
		{
			TryExceptStatement retVal = null;
			stream.Expect (TokenClass.Keyword, "try");
			AstNode tryBody = Statement.Parse (stream);
			AstNode typeList = new ArgumentList (stream.Location);
			stream.Expect (TokenClass.Keyword, "except");
			if (stream.Accept (TokenClass.OpenParan)) {
				Token ident = stream.Expect (TokenClass.Identifier);
				if (stream.Accept (TokenClass.Keyword, "as")) {
					typeList = ParseTypeList (stream);
				}
				stream.Expect (TokenClass.CloseParan);
				retVal = new TryExceptStatement (stream.Location, ident.Value);
			} else {
				retVal = new TryExceptStatement (stream.Location, null);
			}
			retVal.Add (tryBody);
			retVal.Add (Statement.Parse (stream));
			retVal.Add (typeList);
			return retVal;
		}

		private static ArgumentList ParseTypeList (TokenStream stream)
		{
			ArgumentList argList = new ArgumentList (stream.Location);
			while (!stream.Match (TokenClass.CloseParan)) {
				argList.Add (Expression.Parse (stream));
				if (!stream.Accept (TokenClass.Comma)) {
					break;
				}
			}
			return argList;
		}
	}
}
