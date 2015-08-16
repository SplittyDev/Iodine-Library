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
using Iodine.Compiler;
using Iodine.Compiler.Ast;
using Iodine.Runtime;

namespace Iodine
{
	public class IodineEngine
	{
		private IodineModule defaultModule;
		private StackFrame stackFrame;

		public VirtualMachine VirtualMachine {
			private set;
			get;
		}

		public IodineEngine ()
		{
			this.VirtualMachine = new VirtualMachine ();
			this.defaultModule = new IodineModule ("__main__");
			this.stackFrame = new StackFrame (this.defaultModule.Initializer, null, null, 1024);
		}

		public dynamic this [string name] {
			get {
				IodineObject obj = null;
				if (this.VirtualMachine.Globals.ContainsKey (name)) {
					obj = this.VirtualMachine.Globals [name];
				} else if (this.defaultModule.HasAttribute (name)) {
					obj = this.defaultModule.GetAttribute (name);
				}
				Object ret = null;
				if (!IodineTypeConverter.Instance.ConvertToPrimative (obj, out ret)) {
					ret = IodineTypeConverter.Instance.CreateDynamicObject (this, obj);
				}
				return ret;
			}
			set {
				IodineObject obj;
				IodineTypeConverter.Instance.ConvertFromPrimative (value, out obj);
				if (this.defaultModule.HasAttribute (name)) {
					this.defaultModule.SetAttribute (name, obj);
				} else {
					this.VirtualMachine.Globals [name] = obj;
				}
			}
		}

		public dynamic DoString (string source)
		{
			return doString (defaultModule, source);
		}

		public dynamic DoFile (string file)
		{
			IodineModule main = new IodineModule (Path.GetFileNameWithoutExtension (file));
			doString (main, File.ReadAllText (file));
			return new IodineDynamicObject (main, VirtualMachine);
		}

		private dynamic doString (IodineModule module, string source)
		{
			ErrorLog errorLog = new ErrorLog ();
			Lexer lex = new Lexer (errorLog, source);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			Parser parser = new Parser (lex.Scan ());
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			AstRoot root = parser.Parse ();
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			SemanticAnalyser analyser = new SemanticAnalyser (errorLog);
			SymbolTable symTab = analyser.Analyse (root);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);
			IodineCompiler compiler = new IodineCompiler (errorLog, symTab, "");
			module.Initializer.Body.Clear ();
			compiler.CompileAst (module, root);
			if (errorLog.ErrorCount > 0)
				throw new SyntaxException (errorLog);

			IodineObject result = this.VirtualMachine.InvokeMethod (module.Initializer,
				                      null, new IodineObject[] { });
			object ret = null;
			if (!IodineTypeConverter.Instance.ConvertToPrimative (result, out ret)) {
				ret = IodineTypeConverter.Instance.CreateDynamicObject (this, result);
			}
			return ret;
		}
	}

	public class SyntaxException : Exception
	{
		public ErrorLog ErrorLog {
			private set;
			get;
		}

		public SyntaxException (ErrorLog errLog)
			: base ("Syntax Error")
		{
			this.ErrorLog = errLog;
		}
	}
}

