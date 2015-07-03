﻿using System;

namespace Iodine
{
	public class IodineBool : IodineObject
	{
		public static readonly IodineBool True = new IodineBool (true);
		public static readonly IodineBool False = new IodineBool (false);

		private static readonly IodineTypeDefinition BoolTypeDef = new IodineTypeDefinition ("Bool"); 

		public bool Value {
			private set;
			get;
		}

		public IodineBool (bool val)
			: base (BoolTypeDef)
		{
			this.Value = val;
		}

		public override IodineObject PerformBinaryOperation (VirtualMachine vm, BinaryOperation binop, IodineObject rvalue)
		{
			IodineBool boolVal = rvalue as IodineBool;
			switch (binop) {
			case BinaryOperation.Equals:
				return new IodineBool (boolVal.Value == Value);
			case BinaryOperation.NotEquals:
				return new IodineBool (boolVal.Value == Value);
			case BinaryOperation.BoolAnd:
				return new IodineBool (boolVal.Value && Value);
			case BinaryOperation.BoolOr:
				return new IodineBool (boolVal.Value || Value);
			}
			return base.PerformBinaryOperation (vm, binop, rvalue);
		}

		public override IodineObject PerformUnaryOperation (VirtualMachine vm, UnaryOperation op)
		{
			switch (op) {
			case UnaryOperation.BoolNot:
				return new IodineBool (!this.Value);
			}
			return null;
		}

		public override bool IsTrue ()
		{
			return this.Value;
		}

		public override string ToString ()
		{
			return this.Value.ToString ();
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}

