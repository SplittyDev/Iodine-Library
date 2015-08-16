﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Iodine
{
	public class IodineEntry
	{
		class IodineOptions
		{
			public string FileName { get; set; }

			public bool DisplayAst { set; get; }

			public IodineList Arguments { set; get; }

			public static IodineOptions Parse (string[] args)
			{
				IodineOptions ret = new IodineOptions ();
				int i;
				for (i = 0; i < args.Length; i++) {
					if (args [i].StartsWith ("-")) {
						switch (args [i]) {
						default:
							Panic ("Unknown command line argument '{0}'", args [i]);
							break;
						}
					} else {
						ret.FileName = args [i++];
						if (!System.IO.File.Exists (ret.FileName)) {
							Panic ("Could not find file {0}!", ret.FileName);
						}
						break;
					}
				}
				IodineObject[] arguments = new IodineObject [args.Length - i];
				int start = i;
				for (; i < args.Length; i++) {
					arguments [i - start] = new IodineString (args [i]);
				}
				ret.Arguments = new IodineList (arguments);
				return ret;
			}
		}

		public static void Main (string[] args)
		{
			if (args.Length == 0) {
				ReplShell shell = new ReplShell ();
				shell.Run ();
				Environment.Exit (0);
			}

			IodineOptions options = IodineOptions.Parse (args);
			ErrorLog errorLog = new ErrorLog ();
			IodineModule module = IodineModule.LoadModule (errorLog, options.FileName);

			if (module == null) {
				DisplayErrors (errorLog);
				Panic ("Compilation failed with {0} errors!", errorLog.ErrorCount);
			} else {
				VirtualMachine vm = new VirtualMachine ();
				try {
					module.Invoke (vm, new IodineObject[] { });
					if (module.HasAttribute ("main")) {
						module.GetAttribute ("main").Invoke (vm, new IodineObject[] { options.Arguments });
					}
				} catch (UnhandledIodineExceptionException ex) {
					Console.Error.WriteLine ("An unhandled {0} has occured!", ex.OriginalException.TypeDef.Name);
					Console.Error.WriteLine ("\tMessage: {0}", ex.OriginalException.Message);
					Console.WriteLine ();
					ex.PrintStack ();
					Console.Error.WriteLine ();
					Panic ("Program terminated.");
				} catch (Exception e) {
					Console.Error.WriteLine ("Fatal exception has occured!");
					Console.Error.WriteLine (e.Message);
					Console.Error.WriteLine ("Stack trace: \n{0}", e.StackTrace);
					Console.Error.WriteLine ("\nIodine stack trace \n{0}", vm.Stack.Trace ());
					Panic ("Program terminated.");
				}

			}

		}

		private static void DisplayErrors (ErrorLog errorLog)
		{
			foreach (Error err in errorLog) {
				Location loc = err.Location;
				Console.Error.WriteLine ("{0} ({1}:{2}) error: {3}", Path.GetFileName (loc.File),
					loc.Line, loc.Column, err.Text);
			}
		}

		private static void DisplayUsage ()
		{
			Console.WriteLine ("usage: [option] ... [file] [arg] ...");
			Environment.Exit (0);
		}

		private static void Panic (string format, params object[] args)
		{
			Console.Error.WriteLine (format, args);
			Environment.Exit (-1);
		}
	}
}
