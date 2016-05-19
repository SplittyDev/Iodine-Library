/**
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
    public class GivenStatement : AstNode
    {
        public AstNode GivenValue {
            private set;
            get;
        }

        public AstNode DefaultStatement {
            private set;
            get;
        }

        public readonly List<WhenStatement> WhenStatements = new List<WhenStatement> ();

        public GivenStatement (SourceLocation location, AstNode givenValue, IEnumerable<WhenStatement> whenStatements, AstNode defaultStatement)
            : base (location)
        {
            GivenValue = givenValue;
            DefaultStatement = defaultStatement;
            WhenStatements.AddRange (whenStatements);
        }

        public void AddCase (WhenStatement statement)
        {
            WhenStatements.Add (statement);
        }

        public override void Visit (AstVisitor visitor)
        {
            visitor.Accept (this);
        }

        public override void VisitChildren (AstVisitor visitor)
        {
            GivenValue.Visit (visitor);
            WhenStatements.ForEach (p => p.Visit (visitor));
            DefaultStatement.Visit (visitor);
        }
    }
}

