using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Expressions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
	public enum BinaryOperation : byte { Sum, Difference, Product, Quotient, Modulus, Power, LessThan, LessThanOrEqual, GreaterThan,GreaterThanOrEqual,Equal,NotEqual,And,Or,Xor}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
	public enum BinaryOperationArgsType : byte { Int_Int, Int_Double,Double_Double,Double_Int,String_String,String_Int,Bool_Bool}

	public sealed class BinaryExpression : Expression
	{
		/// <summary>
		/// The left hand side of this expression.
		/// </summary>
		public IExpression Left { get; set; }

		/// <summary>
		/// The right hand side of this expression.
		/// </summary>
		public IExpression Right { get; set; }

		public override bool IsPure => Left.IsPure && Right.IsPure;

		public readonly BinaryOperation Operation;
		public readonly BinaryOperationArgsType ArgumentTypes;

		public BinaryExpression(IExpression left,IExpression right, BinaryOperation operation, BinaryOperationArgsType argTypes)
		{
			Left = left; Right = right; Operation = operation; ArgumentTypes = argTypes;
			switch (operation)
			{
				case BinaryOperation.LessThan:
				case BinaryOperation.LessThanOrEqual:
				case BinaryOperation.GreaterThan:
				case BinaryOperation.GreaterThanOrEqual:
				case BinaryOperation.Equal:
				case BinaryOperation.NotEqual:
					Type = LsnType.Bool_.Id;
					break;
				default:
					switch (argTypes)
					{
						case BinaryOperationArgsType.Int_Int:
							Type = LsnType.int_.Id;
							break;
						case BinaryOperationArgsType.Int_Double:
						case BinaryOperationArgsType.Double_Double:
						case BinaryOperationArgsType.Double_Int:
							Type = LsnType.double_.Id;
							break;
						case BinaryOperationArgsType.String_String:
						case BinaryOperationArgsType.String_Int:
							Type = LsnType.string_.Id;
							break;
						case BinaryOperationArgsType.Bool_Bool:
							Type = LsnType.Bool_.Id;
							break;
					}
					break;
			}
			
		}

#if CORE
		public override LsnValue Eval(IInterpreter i)
		{
			var left = _Left.Eval(i);
			LsnValue right;
			switch (ArgumentTypes)
			{
				case BinaryOperationArgsType.Int_Int:
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
				case BinaryOperationArgsType.Int_Double:
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
				case BinaryOperationArgsType.Double_Double:
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
				case BinaryOperationArgsType.Double_Int:
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
				case BinaryOperationArgsType.String_String:
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
				case BinaryOperationArgsType.String_Int:
					right = _Right.Eval(i);
					lefts = (left.Value as StringValue)?.Value ?? "";
					var righti = right.IntValue;
					switch (Operation)
					{
						case BinaryOperation.Product:		return new LsnValue(new StringValue(new StringBuilder().Append(lefts, 0, righti).ToString()));
						case BinaryOperation.Sum:			return new LsnValue(new StringValue(lefts + right.IntValue));
						default:
							throw new InvalidOperationException(Operation.ToString());
					}
				case BinaryOperationArgsType.Bool_Bool:
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
#endif

		public override IExpression Fold()
		{
			Right = Right.Fold();
			Left = Left.Fold();
			var lVal = typeof(LsnValue).IsAssignableFrom(Left.GetType());
			var rVal = typeof(LsnValue).IsAssignableFrom(Right.GetType());
			if (lVal)
			{
				var left = (LsnValue)Left;
				if (lVal && rVal)
				{
					var right = (LsnValue)Right;
					switch (ArgumentTypes)
					{
						case BinaryOperationArgsType.Int_Int:
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
						case BinaryOperationArgsType.Int_Double:
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
						case BinaryOperationArgsType.Double_Double:
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
						case BinaryOperationArgsType.Double_Int:
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
						case BinaryOperationArgsType.String_String:
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
						case BinaryOperationArgsType.String_Int:
							lefts = (left.Value as StringValue)?.Value ?? "";
							var righti = right.IntValue;
							switch (Operation)
							{
								case BinaryOperation.Product: return new LsnValue(new StringValue(new StringBuilder().Append(lefts, 0, righti).ToString()));
								default:
									throw new InvalidOperationException(Operation.ToString());
							}
						case BinaryOperationArgsType.Bool_Bool:
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
							case BinaryOperationArgsType.Int_Int:
							case BinaryOperationArgsType.Int_Double:
								if (left.IntValue == 0)
									return Right;
								return this;
							case BinaryOperationArgsType.Double_Double:
							case BinaryOperationArgsType.Double_Int:
								if (left.DoubleValue < double.Epsilon)
									return Right;
								return this;
							case BinaryOperationArgsType.String_String:
								if (string.IsNullOrEmpty((left.Value as StringValue).Value))
									return Right;
								return this;
							default:
								break;
						}
						return this;
					case BinaryOperation.Difference:
						break;
					case BinaryOperation.Product:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgsType.Int_Int:
							case BinaryOperationArgsType.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								if (leftI == 1)
									return Right;
								break;
							case BinaryOperationArgsType.Double_Double:
							case BinaryOperationArgsType.Double_Int:
								if (Math.Abs(left.DoubleValue) < double.Epsilon)
									return new LsnValue(0.0);
								if (Math.Abs(left.DoubleValue - 1) < double.Epsilon)
									return Right;
								return this;
							default:
								break;
						}
						return this;
					case BinaryOperation.Quotient:
					case BinaryOperation.Modulus:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgsType.Int_Int:
							case BinaryOperationArgsType.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								return this;
							case BinaryOperationArgsType.Double_Double:
							case BinaryOperationArgsType.Double_Int:
								if (Math.Abs(left.DoubleValue) < double.Epsilon)
									return new LsnValue(0.0);
								return this;
							default:
								break;
						}
						break;
					case BinaryOperation.Power:
						switch (ArgumentTypes)
						{
							case BinaryOperationArgsType.Int_Int:
							case BinaryOperationArgsType.Int_Double:
								var leftI = left.IntValue;
								if (leftI == 0)
									return new LsnValue(0);
								if (leftI == 1)
									return new LsnValue(1);
								break;
							case BinaryOperationArgsType.Double_Double:
							case BinaryOperationArgsType.Double_Int:
								var leftD = left.DoubleValue;
								if (Math.Abs(leftD) < double.Epsilon)
									return new LsnValue(0.0);
								if (Math.Abs(leftD - 1) < double.Epsilon)
									return new LsnValue(1.0);
								break;
							default:
								break;
						}
						break;
					case BinaryOperation.And:
						if (ArgumentTypes == BinaryOperationArgsType.Bool_Bool)
						{
							if(!left.BoolValue)
								return new LsnValue(false);
							return Right;
						}
						break;
					case BinaryOperation.Or:
						if(ArgumentTypes == BinaryOperationArgsType.Bool_Bool)
						{
							if (left.BoolValue)
								return new LsnValue(true);
							return Right;
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
			if (Left.Equals(oldExpr))
				Left = newExpr;
			else
				Left.Replace(oldExpr, newExpr);

			if (Right.Equals(oldExpr))
				Right = newExpr;
			else
				Right.Replace(oldExpr, newExpr);
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.BinaryExpression);
			writer.Write((byte)(((byte)ArgumentTypes << 4) | ((byte)Operation)));
			Left.Serialize(writer, resourceSerializer);
			Right.Serialize(writer, resourceSerializer);
		}

		public static BinaryOperationArgsType GetArgTypes(TypeId left, TypeId right)
		{
			switch (left.Name)
			{
				case "int":
					switch (right.Name)
					{
						case "int": return BinaryOperationArgsType.Int_Int;
						case "double": return BinaryOperationArgsType.Int_Double;
						default:
							break;
					}
					break;
				case "double":
					switch (right.Name)
					{
						case "int": return BinaryOperationArgsType.Double_Int;
						case "double": return BinaryOperationArgsType.Double_Double;
						default:
							break;
					}
					break;
				case "string":
					switch (right.Name)
					{
						case "string": return BinaryOperationArgsType.String_String;
						case "int": return BinaryOperationArgsType.String_Int;
						default:
							break;
					}
					break;
				case "bool":
					/*switch (right.Name)
					{
						case "bool": return BinaryOperationArgTypes.Bool_Bool;
						default:
							break;
					}
					break;*/
					return BinaryOperationArgsType.Bool_Bool;
				default:
					return BinaryOperationArgsType.Bool_Bool;
			}

			throw new InvalidOperationException();
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return Left;
			foreach (var expr in Left.SelectMany(e => e))
				yield return expr;
			yield return Right;
			foreach (var expr in Right.SelectMany(e => e))
				yield return expr;
		}
	}
}
