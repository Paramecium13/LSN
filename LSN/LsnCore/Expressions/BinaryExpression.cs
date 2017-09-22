using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	[Serializable]
	public enum BinaryOperation : byte { Sum, Difference, Product, Quotient, Modulus, Power, LessThan, LessThanOrEqual, GreaterThan,GreaterThanOrEqual,Equal,NotEqual,And,Or,Xor}

	[Serializable]
	public enum BinaryOperationArgTypes : byte { Int_Int, Int_Double,Double_Double,Double_Int,String_String,String_Int,Bool_Bool}
	
	[Serializable]
	public class BinaryExpression : Expression
	{
		private IExpression _Left;

		/// <summary>
		/// The left hand side of this expression.
		/// </summary>
		public IExpression Left { get { return _Left; } set { _Left = value; } }

		private IExpression _Right;

		/// <summary>
		/// The right hand side of this expression.
		/// </summary>
		public IExpression Right { get { return _Right; } set { _Right = value; } }

		public override bool IsPure => _Left.IsPure && _Right.IsPure;

		public readonly BinaryOperation Operation;
		public readonly BinaryOperationArgTypes ArgumentTypes;

		public BinaryExpression(IExpression left,IExpression right, BinaryOperation operation, BinaryOperationArgTypes argTypes)
		{
			_Left = left; _Right = right; Operation = operation; ArgumentTypes = argTypes;
			switch (argTypes)
			{
				case BinaryOperationArgTypes.Int_Int:
					Type = LsnType.int_.Id;
					break;
				case BinaryOperationArgTypes.Int_Double:
				case BinaryOperationArgTypes.Double_Double:
				case BinaryOperationArgTypes.Double_Int:
					Type = LsnType.double_.Id;
					break;
				case BinaryOperationArgTypes.String_String:
				case BinaryOperationArgTypes.String_Int:
					Type = LsnType.string_.Id;
					break;
				case BinaryOperationArgTypes.Bool_Bool:
					Type = LsnType.Bool_.Id;
					break;
			}
		}


		public override LsnValue Eval(IInterpreter i)
		{
			var left = _Left.Eval(i);
			LsnValue right;
			switch (ArgumentTypes)
			{
				case BinaryOperationArgTypes.Int_Int:
					right = _Right.Eval(i);
					switch (Operation)
					{
						case BinaryOperation.Sum:					return LsnValue.IntSum(left, right);
						case BinaryOperation.Difference:			return LsnValue.IntDiff(left, right);
						case BinaryOperation.Product:				return LsnValue.IntProduct(left,right);
						case BinaryOperation.Quotient:				return LsnValue.IntQuotient(left, right);
						case BinaryOperation.Modulus:				return LsnValue.IntMod(left, right);
						case BinaryOperation.Power:					return LsnValue.IntPow(left, right);
						case BinaryOperation.LessThan:				return new LsnValue(left.IntValue <  right.IntValue);
						case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.IntValue <= right.IntValue);
						case BinaryOperation.GreaterThan:			return new LsnValue(left.IntValue >  right.IntValue);
							
						case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.IntValue >= right.IntValue);
						case BinaryOperation.Equal:					return new LsnValue(left.IntValue == right.IntValue);
						case BinaryOperation.NotEqual:				return new LsnValue(left.IntValue != right.IntValue);
						case BinaryOperation.And:					return new LsnValue(left.IntValue &  right.IntValue);
						case BinaryOperation.Or:					return new LsnValue(left.IntValue |  right.IntValue);
						case BinaryOperation.Xor:					return new LsnValue(left.IntValue ^  right.IntValue);
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgTypes.Int_Double:
					right = _Right.Eval(i);
					switch (Operation)
					{
						case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
						case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
						case BinaryOperation.Product:				return LsnValue.DoubleProduct(left,right);
						case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
						case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
						case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
						case BinaryOperation.LessThan:				return new LsnValue(left.IntValue <  right.DoubleValue);
						case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.IntValue <= right.DoubleValue);
						case BinaryOperation.GreaterThan:			return new LsnValue(left.IntValue >  right.DoubleValue);
							
						case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.IntValue >= right.DoubleValue);
						case BinaryOperation.Equal:					return new LsnValue((left.IntValue - right.DoubleValue) < double.Epsilon);
						case BinaryOperation.NotEqual:				return new LsnValue((left.IntValue - right.DoubleValue) >= double.Epsilon);
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgTypes.Double_Double:
					right = _Right.Eval(i);switch (Operation)
					{
						case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
						case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
						case BinaryOperation.Product:				return LsnValue.DoubleProduct(left,right);
						case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
						case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
						case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
						case BinaryOperation.LessThan:				return new LsnValue(left.DoubleValue <  right.DoubleValue);
						case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.DoubleValue <= right.DoubleValue);
						case BinaryOperation.GreaterThan:			return new LsnValue(left.DoubleValue >  right.DoubleValue);
						case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.DoubleValue >= right.DoubleValue);
						case BinaryOperation.Equal:					return new LsnValue((left.DoubleValue - right.DoubleValue) < double.Epsilon);
						case BinaryOperation.NotEqual:				return new LsnValue((left.DoubleValue - right.DoubleValue) >= double.Epsilon);
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgTypes.Double_Int:
					right = _Right.Eval(i);
					switch (Operation)
					{
						case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
						case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
						case BinaryOperation.Product:				return LsnValue.DoubleProduct(left,right);
						case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
						case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
						case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
						case BinaryOperation.LessThan:				return new LsnValue(left.DoubleValue <  right.IntValue);
						case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.DoubleValue <= right.IntValue);
						case BinaryOperation.GreaterThan:			return new LsnValue(left.DoubleValue >  right.IntValue);
							
						case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.DoubleValue >= right.IntValue);
						case BinaryOperation.Equal:					return new LsnValue((left.DoubleValue - right.IntValue) < double.Epsilon);
						case BinaryOperation.NotEqual:				return new LsnValue((left.DoubleValue - right.IntValue) >= double.Epsilon);
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgTypes.String_String:
					right = _Right.Eval(i);
					var lefts  = (left .Value as StringValue)?.Value;
					var rights = (right.Value as StringValue)?.Value;
					switch (Operation)
					{
						case BinaryOperation.Sum:					return new LsnValue(new StringValue(lefts + rights));
						case BinaryOperation.LessThan:				return new LsnValue(lefts?.Contains(rights) ?? false);
						case BinaryOperation.LessThanOrEqual:		return new LsnValue(lefts == rights || (lefts?.Contains(rights) ?? false));
						case BinaryOperation.GreaterThan:			return new LsnValue(rights?.Contains(lefts) ?? false);
						case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(rights == lefts|| (rights?.Contains(lefts) ?? false));
						case BinaryOperation.Equal:					return new LsnValue(lefts == rights);
						case BinaryOperation.NotEqual:				return new LsnValue(lefts != rights);
						case BinaryOperation.And:					return new LsnValue(new StringValue(new string(lefts.Intersect(rights).ToArray())));
						default:
							throw new InvalidOperationException();
					}
				case BinaryOperationArgTypes.String_Int:
					right = _Right.Eval(i);
					lefts = (left.Value as StringValue)?.Value ?? "";
					var righti = right.IntValue;
					switch (Operation)
					{
						case BinaryOperation.Product:		return new LsnValue(new StringValue(new StringBuilder().Append(lefts, 0, righti).ToString()));
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgTypes.Bool_Bool:
					switch (Operation)
					{
						case BinaryOperation.Equal:			return new LsnValue(left.BoolValue == _Right.Eval(i).BoolValue);
						case BinaryOperation.NotEqual:		return new LsnValue(left.BoolValue != _Right.Eval(i).BoolValue);
						case BinaryOperation.And:			return new LsnValue(left.BoolValue && _Right.Eval(i).BoolValue);
						case BinaryOperation.Or:			return new LsnValue(left.BoolValue || _Right.Eval(i).BoolValue);
						case BinaryOperation.Xor:			return new LsnValue(left.BoolValue ^  _Right.Eval(i).BoolValue);
						default:
							throw new InvalidOperationException();
					}
				default:
					throw new InvalidOperationException(ArgumentTypes.ToString());
			}
		}

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			var lVal = typeof(LsnValue).IsAssignableFrom(Left.GetType());
			var rVal = typeof(LsnValue).IsAssignableFrom(Right.GetType());
			if (lVal)
			{
				var left = (LsnValue)_Left;
				if (lVal && rVal)
				{
					var right = (LsnValue)_Right;
					switch (ArgumentTypes)
					{
						case BinaryOperationArgTypes.Int_Int:
							switch (Operation)
							{
								case BinaryOperation.Sum:					return LsnValue.IntSum(left, right);
								case BinaryOperation.Difference:			return LsnValue.IntDiff(left, right);
								case BinaryOperation.Product:				return LsnValue.IntProduct(left, right);
								case BinaryOperation.Quotient:				return LsnValue.IntQuotient(left, right);
								case BinaryOperation.Modulus:				return LsnValue.IntMod(left, right);
								case BinaryOperation.Power:					return LsnValue.IntPow(left, right);
								case BinaryOperation.LessThan:				return new LsnValue(left.IntValue < right.IntValue);
								case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.IntValue <= right.IntValue);
								case BinaryOperation.GreaterThan:			return new LsnValue(left.IntValue > right.IntValue);

								case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.IntValue >= right.IntValue);
								case BinaryOperation.Equal:					return new LsnValue(left.IntValue == right.IntValue);
								case BinaryOperation.NotEqual:				return new LsnValue(left.IntValue != right.IntValue);
								case BinaryOperation.And:					return new LsnValue(left.IntValue & right.IntValue);
								case BinaryOperation.Or:					return new LsnValue(left.IntValue | right.IntValue);
								case BinaryOperation.Xor:					return new LsnValue(left.IntValue ^ right.IntValue);
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgTypes.Int_Double:
							switch (Operation)
							{
								case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
								case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
								case BinaryOperation.Product:				return LsnValue.DoubleProduct(left, right);
								case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
								case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
								case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
								case BinaryOperation.LessThan:				return new LsnValue(left.IntValue <  right.DoubleValue);
								case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.IntValue <= right.DoubleValue);
								case BinaryOperation.GreaterThan:			return new LsnValue(left.IntValue >  right.DoubleValue);
								case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.IntValue >= right.DoubleValue);
								case BinaryOperation.Equal:					return new LsnValue(Math.Abs(left.IntValue - right.DoubleValue) < double.Epsilon);
								case BinaryOperation.NotEqual:				return new LsnValue(Math.Abs(left.IntValue - right.DoubleValue) >= double.Epsilon);
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgTypes.Double_Double:
							switch (Operation)
							{
								case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
								case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
								case BinaryOperation.Product:				return LsnValue.DoubleProduct(left, right);
								case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
								case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
								case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
								case BinaryOperation.LessThan:				return new LsnValue(left.DoubleValue < right.DoubleValue);
								case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.DoubleValue <= right.DoubleValue);
								case BinaryOperation.GreaterThan:			return new LsnValue(left.DoubleValue > right.DoubleValue);
								case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.DoubleValue >= right.DoubleValue);
								case BinaryOperation.Equal:					return new LsnValue(Math.Abs(left.DoubleValue - right.DoubleValue) < double.Epsilon);
								case BinaryOperation.NotEqual:				return new LsnValue(Math.Abs(left.DoubleValue - right.DoubleValue) >= double.Epsilon);
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgTypes.Double_Int:
							switch (Operation)
							{
								case BinaryOperation.Sum:					return LsnValue.DoubleSum(left, right);
								case BinaryOperation.Difference:			return LsnValue.DoubleDiff(left, right);
								case BinaryOperation.Product:				return LsnValue.DoubleProduct(left, right);
								case BinaryOperation.Quotient:				return LsnValue.DoubleQuotient(left, right);
								case BinaryOperation.Modulus:				return LsnValue.DoubleMod(left, right);
								case BinaryOperation.Power:					return LsnValue.DoublePow(left, right);
								case BinaryOperation.LessThan:				return new LsnValue(left.DoubleValue < right.IntValue);
								case BinaryOperation.LessThanOrEqual:		return new LsnValue(left.DoubleValue <= right.IntValue);
								case BinaryOperation.GreaterThan:			return new LsnValue(left.DoubleValue > right.IntValue);
								case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(left.DoubleValue >= right.IntValue);
								case BinaryOperation.Equal:					return new LsnValue(Math.Abs(left.DoubleValue - right.IntValue) < double.Epsilon);
								case BinaryOperation.NotEqual:				return new LsnValue(Math.Abs(left.DoubleValue - right.IntValue) >= double.Epsilon);
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgTypes.String_String:
							var lefts = (left.Value as StringValue)?.Value;
							var rights = (right.Value as StringValue)?.Value;
							switch (Operation)
							{
								case BinaryOperation.Sum:					return new LsnValue(new StringValue(lefts + rights));
								case BinaryOperation.LessThan:				return new LsnValue(lefts?.Contains(rights) ?? false);
								case BinaryOperation.LessThanOrEqual:		return new LsnValue(lefts == rights || (lefts?.Contains(rights) ?? false));
								case BinaryOperation.GreaterThan:			return new LsnValue(rights?.Contains(lefts) ?? false);
								case BinaryOperation.GreaterThanOrEqual:	return new LsnValue(rights == lefts || (rights?.Contains(lefts) ?? false));
								case BinaryOperation.Equal:					return new LsnValue(lefts == rights);
								case BinaryOperation.NotEqual:				return new LsnValue(lefts != rights);
								case BinaryOperation.And:					return new LsnValue(new StringValue(new string(lefts.Intersect(rights).ToArray())));
								default:
									throw new InvalidOperationException();
							}
						case BinaryOperationArgTypes.String_Int:
							lefts = (left.Value as StringValue)?.Value ?? "";
							var righti = right.IntValue;
							switch (Operation)
							{
								case BinaryOperation.Product: return new LsnValue(new StringValue(new StringBuilder().Append(lefts, 0, righti).ToString()));
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgTypes.Bool_Bool:
							switch (Operation)
							{
								case BinaryOperation.Equal:		return new LsnValue(left.BoolValue == right.BoolValue);
								case BinaryOperation.NotEqual:	return new LsnValue(left.BoolValue != right.BoolValue);
								case BinaryOperation.And:		return new LsnValue(left.BoolValue && right.BoolValue);
								case BinaryOperation.Or:		return new LsnValue(left.BoolValue || right.BoolValue);
								case BinaryOperation.Xor:		return new LsnValue(left.BoolValue ^ right.BoolValue);
								default:
									throw new InvalidOperationException();
							}
						default:
							throw new InvalidOperationException(ArgumentTypes.ToString());
					}
				}
				switch (Operation)
				{
					case BinaryOperation.Sum:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgTypes.Int_Int:
							case BinaryOperationArgTypes.Int_Double:
								if(left.IntValue == 0)
									return _Right;
								return this;
							case BinaryOperationArgTypes.Double_Double:
							case BinaryOperationArgTypes.Double_Int:
								if (left.DoubleValue < double.Epsilon)
									return _Right;
								return this;
							case BinaryOperationArgTypes.String_String:
								if(string.IsNullOrEmpty((left.Value as StringValue).Value))
									return _Right;
								return this;
						}
						return this;
					case BinaryOperation.Difference:
						break;
					case BinaryOperation.Product:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgTypes.Int_Int:
							case BinaryOperationArgTypes.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								if (leftI == 1)
									return _Right;
								break;
							case BinaryOperationArgTypes.Double_Double:
							case BinaryOperationArgTypes.Double_Int:
								if (Math.Abs(left.DoubleValue) < double.Epsilon)
									return new LsnValue(0.0);
								if (Math.Abs(left.DoubleValue - 1) < double.Epsilon)
									return _Right;
								return this;
						}
						return this;
					case BinaryOperation.Quotient:
					case BinaryOperation.Modulus:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgTypes.Int_Int:
							case BinaryOperationArgTypes.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								return this;
							case BinaryOperationArgTypes.Double_Double:
							case BinaryOperationArgTypes.Double_Int:
								if (Math.Abs(left.DoubleValue) < double.Epsilon)
									return new LsnValue(0.0);
								return this;
						}
						break;
					case BinaryOperation.Power:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgTypes.Int_Int:
							case BinaryOperationArgTypes.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								if (leftI == 1)
									return new LsnValue(1);
								break;
							case BinaryOperationArgTypes.Double_Double:
							case BinaryOperationArgTypes.Double_Int:
								var leftD = left.DoubleValue;
								if (Math.Abs(leftD) < double.Epsilon)
									return new LsnValue(0.0);
								if (Math.Abs(leftD - 1) < double.Epsilon)
									return new LsnValue(1.0);
								break;
						}
						break;
					case BinaryOperation.And:
						if (ArgumentTypes == BinaryOperationArgTypes.Bool_Bool)
						{
							if(!left.BoolValue)
								return new LsnValue(false);
							return _Right;
						}
						break;
					case BinaryOperation.Or:
						if(ArgumentTypes == BinaryOperationArgTypes.Bool_Bool)
						{
							if (left.BoolValue)
								return new LsnValue(true);
							return _Right;
						}
						break;
					default:
						return this;
				}
			}
			else if (rVal) // ToDo: Implement
			{
				switch (Operation)
				{
					case BinaryOperation.Sum:
						break;
					case BinaryOperation.Difference:
						break;
					case BinaryOperation.Product:
						break;
					case BinaryOperation.Quotient:
						break;
					case BinaryOperation.Modulus:
						break;
					case BinaryOperation.Power:
						break;
					case BinaryOperation.LessThan:
						break;
					case BinaryOperation.LessThanOrEqual:
						break;
					case BinaryOperation.GreaterThan:
						break;
					case BinaryOperation.GreaterThanOrEqual:
						break;
					case BinaryOperation.Equal:
						break;
					case BinaryOperation.NotEqual:
						break;
					case BinaryOperation.And:
						break;
					case BinaryOperation.Or:
						break;
					case BinaryOperation.Xor:
						break;
					default:
						break;
				}
			}
			return this;
		}

		public override bool IsReifyTimeConst()
			=> Left.IsReifyTimeConst() && Right.IsReifyTimeConst();


		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (_Left.Equals(oldExpr))
				_Left = newExpr;
			else
				_Left.Replace(oldExpr, newExpr);

			if (_Right.Equals(oldExpr))
				_Right = newExpr;
			else
				_Right.Replace(oldExpr, newExpr);
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.BinaryExpression);
			writer.Write((byte)(((byte)ArgumentTypes << 4) | ((byte)Operation)));
			Left.Serialize(writer, resourceSerializer);
			Right.Serialize(writer, resourceSerializer);
		}

		public static BinaryOperationArgTypes GetArgTypes(TypeId left, TypeId right)
		{
			switch (left.Name)
			{
				case "int":
					switch (right.Name)
					{
						case "int":		return BinaryOperationArgTypes.Int_Int;
						case "double":	return BinaryOperationArgTypes.Int_Double;
					}
					break;
				case "double":
					switch (right.Name)
					{
						case "int":		return BinaryOperationArgTypes.Double_Int;
						case "double":	return BinaryOperationArgTypes.Double_Double;
					}
					break;
				case "string":
					switch (right.Name)
					{
						case "string":	return BinaryOperationArgTypes.String_String;
						case "int":		return BinaryOperationArgTypes.String_Int;
					}
					break;
				case "bool":
				default:
					return BinaryOperationArgTypes.Bool_Bool;
				
			}

			throw new InvalidOperationException();
		}

	}

}
