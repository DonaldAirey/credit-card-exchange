namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;

	/// <summary>
	/// Wraps a regular IL generator and provides additional functionality we need
	/// </summary>
	/// <remarks></remarks>
	internal class FleeILGenerator
	{

		private ILGenerator MyILGenerator;
		private int MyLength;
		private int MyLabelCount;
		private Dictionary<Type, LocalBuilder> MyTempLocals;
		private bool MyIsTemp;

		public FleeILGenerator(ILGenerator ilg, int startLength, bool isTemp)
		{
			MyILGenerator = ilg;
			MyTempLocals = new Dictionary<Type, LocalBuilder>();
			MyIsTemp = isTemp;
			MyLength = startLength;
		}

		public int GetTempLocalIndex(Type localType)
		{
			LocalBuilder local = null;

			if (MyTempLocals.TryGetValue(localType, out local) == false)
			{
				local = MyILGenerator.DeclareLocal(localType);
				MyTempLocals.Add(localType, local);
			}

			return local.LocalIndex;
		}

		public void Emit(OpCode op)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op);
		}

		public void Emit(OpCode op, Type arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, ConstructorInfo arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, MethodInfo arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, FieldInfo arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, byte arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, sbyte arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, short arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, int arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, long arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, float arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, double arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, string arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void Emit(OpCode op, Label arg)
		{
			this.RecordOpcode(op);
			MyILGenerator.Emit(op, arg);
		}

		public void MarkLabel(Label lbl)
		{
			MyILGenerator.MarkLabel(lbl);
		}

		public Label DefineLabel()
		{
			MyLabelCount += 1;
			return MyILGenerator.DefineLabel();
		}

		public LocalBuilder DeclareLocal(Type localType)
		{
			return MyILGenerator.DeclareLocal(localType);
		}

		private void RecordOpcode(OpCode op)
		{
			//Trace.WriteLine(String.Format("{0:x}: {1}", MyLength, op.Name))
			int operandLength = GetOpcodeOperandSize(op.OperandType);
			MyLength += op.Size + operandLength;
		}

		private static int GetOpcodeOperandSize(OperandType operand)
		{
			switch (operand)
			{
			case OperandType.InlineNone:
				return 0;
			case OperandType.ShortInlineBrTarget:
			case OperandType.ShortInlineI:
			case OperandType.ShortInlineVar:
				return 1;
			case OperandType.InlineVar:
				return 2;
			case OperandType.InlineBrTarget:
			case OperandType.InlineField:
			case OperandType.InlineI:
			case OperandType.InlineMethod:
			case OperandType.InlineSig:
			case OperandType.InlineString:
			case OperandType.InlineTok:
			case OperandType.InlineType:
			case OperandType.ShortInlineR:
				return 4;
			case OperandType.InlineI8:
			case OperandType.InlineR:
				return 8;
			default:
				Debug.Fail("Unknown operand type");
				break;
			}
			return 0;
		}

		public void ValidateLength()
		{
			Debug.Assert(this.Length == this.ILGeneratorLength, "ILGenerator length mismatch");
		}

		public int Length
		{
			get { return MyLength; }
		}

		public int LabelCount
		{
			get { return MyLabelCount; }
		}

		private int ILGeneratorLength
		{
			get { return Utility.GetILGeneratorLength(MyILGenerator); }
		}

		public bool IsTemp
		{
			get { return MyIsTemp; }
		}
	}

}