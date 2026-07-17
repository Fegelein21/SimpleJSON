/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Changelog now external. See Changelog.txt
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2022 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SimpleJSON
{
	public enum JSONNodeType
	{
		Array = 1,
		Object = 2,
		String = 3,
		Number = 4,
		NullValue = 5,
		Boolean = 6,
		None = 7,
		Custom = 0xFF,
	}
	public enum JSONTextMode
	{
		Compact,
		Indent
	}

	public abstract partial class JSONNode : IEnumerable<KeyValuePair<string, JSONNode>>
	{
		#region Enumerators

		public virtual IEnumerator<KeyValuePair<string, JSONNode>> GetEnumerator() { yield break; }
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public virtual IEnumerable<string> Keys { get { yield break; } }
		public virtual IEnumerable<JSONNode> Values { get { yield break; } }

		#endregion Enumerators

		#region common interface

		public static bool forceASCII = false; // Use Unicode by default
		public static bool longAsString = false; // lazy creator creates a JSONString instead of JSONNumber
		public static bool allowLineComments = true; // allow "//"-style comments at the end of a line
		public static bool AllowDuplicateKeys = false; // disallow duplicate keys in dictionary by default
		public static char IndentChar = ' '; // Use 'space' by default

		public abstract JSONNodeType Tag { get; }

		public virtual JSONNode this[int aIndex] { get { return null; } set { } }

		public virtual JSONNode this[string aKey] { get { return null; } set { } }

		public virtual string Value { get { return ""; } set { } }

		public virtual int Count => 0;

		public virtual bool IsNumber => false;
		public virtual bool IsString => false;
		public virtual bool IsBoolean => false;
		public virtual bool IsNull => false;
		public virtual bool IsArray => false;
		public virtual bool IsObject => false;

		public virtual bool Inline { get { return false; } set { } }

		public virtual void Add(string aKey, JSONNode aItem)
		{
		}
		public virtual void Add(JSONNode aItem)
		{
			Add("", aItem);
		}

		public virtual JSONNode Remove(string aKey)
		{
			return null;
		}

		public virtual JSONNode Remove(int aIndex)
		{
			return null;
		}

		public virtual JSONNode Remove(JSONNode aNode)
		{
			return aNode;
		}
		public virtual void Clear() { }

		public virtual JSONNode Clone()
		{
			return null;
		}

		public virtual IEnumerable<JSONNode> Children
		{
			get { yield break; }
		}

		public IEnumerable<JSONNode> DeepChildren
		{
			get
			{
				foreach (var C in Children)
					foreach (var D in C.DeepChildren)
						yield return D;
			}
		}

		public virtual bool HasKey(string aKey)
		{
			return false;
		}

		public virtual JSONNode GetValueOrDefault(string aKey, JSONNode aDefault)
		{
			return aDefault;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, 0, JSONTextMode.Compact);
			return sb.ToString();
		}

		public virtual string ToString(int aIndent)
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, aIndent, JSONTextMode.Indent);
			return sb.ToString();
		}

		internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode);

		#endregion common interface

		#region typecasting properties


		public virtual double AsDouble
		{
			get
			{
				double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v);
				return v;
			}
			set
			{
				Value = value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public virtual int AsInt
		{
			get { return (int)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual float AsFloat
		{
			get { return (float)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual bool AsBool
		{
			get
			{
				if (bool.TryParse(Value, out bool v))
					return v;
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				Value = value ? "true" : "false";
			}
		}

		public virtual long AsLong
		{
			get
			{
				long.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long val);
				return val;
			}
			set
			{
				Value = value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public virtual ulong AsULong
		{
			get
			{
				ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong val);
				return val;
			}
			set
			{
				Value = value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public virtual JSONArray AsArray => this as JSONArray;

		public virtual JSONObject AsObject => this as JSONObject;


		#endregion typecasting properties

		#region operators

		public static implicit operator JSONNode(string s)
		{
			return (s == null) ? (JSONNode)JSONNull.CreateOrGet() : new JSONString(s);
		}
		public static implicit operator string(JSONNode d)
		{
			return d?.Value;
		}

		public static implicit operator JSONNode(double n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator double(JSONNode d)
		{
			return (d == null) ? 0 : d.AsDouble;
		}

		public static implicit operator JSONNode(float n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator float(JSONNode d)
		{
			return (d == null) ? 0 : d.AsFloat;
		}

		public static implicit operator JSONNode(int n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator int(JSONNode d)
		{
			return (d == null) ? 0 : d.AsInt;
		}

		public static implicit operator JSONNode(long n)
		{
			if (longAsString)
				return new JSONString(n.ToString(CultureInfo.InvariantCulture));
			return new JSONNumber(n);
		}
		public static implicit operator long(JSONNode d)
		{
			return (d == null) ? 0L : d.AsLong;
		}

		public static implicit operator JSONNode(ulong n)
		{
			if (longAsString)
				return new JSONString(n.ToString(CultureInfo.InvariantCulture));
			return new JSONNumber(n);
		}
		public static implicit operator ulong(JSONNode d)
		{
			return (d == null) ? 0 : d.AsULong;
		}

		public static implicit operator JSONNode(bool b)
		{
			return new JSONBool(b);
		}
		public static implicit operator bool(JSONNode d)
		{
			return d != null && d.AsBool;
		}

		public static implicit operator JSONNode(KeyValuePair<string, JSONNode> aKeyValue)
		{
			return aKeyValue.Value;
		}

		public static bool operator ==(JSONNode a, object b)
		{
			if (ReferenceEquals(a, b))
				return true;
			if (a is JSONNull || a is null || a is JSONLazyCreator)
				return b is JSONNull || b is null || b is JSONLazyCreator;
			return a.Equals(b);
		}

		public static bool operator !=(JSONNode a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion operators

		[ThreadStatic]
		private static StringBuilder m_EscapeBuilder;
		internal static StringBuilder EscapeBuilder
		{
			get
			{
				if (m_EscapeBuilder == null)
					m_EscapeBuilder = new StringBuilder();
				return m_EscapeBuilder;
			}
		}
		internal static string Escape(string aText)
		{
			var sb = EscapeBuilder;
			sb.Length = 0;
			if (sb.Capacity < aText.Length + aText.Length / 10)
				sb.Capacity = aText.Length + aText.Length / 10;
			foreach (char c in aText)
			{
				switch (c)
				{
					case '\\':
						sb.Append("\\\\");
						break;
					case '\"':
						sb.Append("\\\"");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					default:
						if (c < ' ' || (forceASCII && c > 127))
						{
							ushort val = c;
							sb.Append("\\u").Append(val.ToString("X4"));
						}
						else
							sb.Append(c);
						break;
				}
			}
			string result = sb.ToString();
			sb.Length = 0;
			return result;
		}

		private static JSONNode ParseElement(string token, bool quoted)
		{
			if (quoted)
				return token;
			if (token.Length <= 5)
			{
				string tmp = token.ToLower();
				switch (tmp)
				{
					case "false":
					case "true":
						return tmp == "true";
					case "null":
						return JSONNull.CreateOrGet();
				}
			}
			if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
				return val;
			else
				return token;
		}

		public static JSONNode Parse(string aJSON)
		{
			Stack<JSONNode> stack = new Stack<JSONNode>();
			JSONNode ctx = null;
			int i = 0;
			StringBuilder Token = new StringBuilder();
			string TokenName = "";
			bool QuoteMode = false;
			bool TokenIsQuoted = false;
			bool HasNewlineChar = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
					case '{':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}
						stack.Push(new JSONObject());
						ctx?.Add(TokenName, stack.Peek());
						TokenName = "";
						Token.Length = 0;
						ctx = stack.Peek();
						HasNewlineChar = false;
						break;

					case '[':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}

						stack.Push(new JSONArray());
						ctx?.Add(TokenName, stack.Peek());
						TokenName = "";
						Token.Length = 0;
						ctx = stack.Peek();
						HasNewlineChar = false;
						break;

					case '}':
					case ']':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}
						if (stack.Count == 0)
							throw new Exception("JSON Parse: Too many closing brackets");

						stack.Pop();
						if (Token.Length > 0 || TokenIsQuoted)
							ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
						if (ctx != null)
							ctx.Inline = !HasNewlineChar;
						TokenIsQuoted = false;
						TokenName = "";
						Token.Length = 0;
						if (stack.Count > 0)
							ctx = stack.Peek();
						break;

					case ':':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}
						TokenName = Token.ToString();
						Token.Length = 0;
						TokenIsQuoted = false;
						break;

					case '"':
						QuoteMode ^= true;
						TokenIsQuoted |= QuoteMode;
						break;

					case ',':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}
						if (Token.Length > 0 || TokenIsQuoted)
							ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
						TokenName = "";
						Token.Length = 0;
						TokenIsQuoted = false;
						break;

					case '\r':
					case '\n':
						if (QuoteMode)
						{
							Token.Append(aJSON[i]);
							break;
						}
						HasNewlineChar = true;
						break;

					case ' ':
					case '\t':
						if (QuoteMode)
							Token.Append(aJSON[i]);
						break;

					case '\\':
						++i;
						if (QuoteMode)
						{
							char C = aJSON[i];
							switch (C)
							{
								case 't':
									Token.Append('\t');
									break;
								case 'r':
									Token.Append('\r');
									break;
								case 'n':
									Token.Append('\n');
									break;
								case 'b':
									Token.Append('\b');
									break;
								case 'f':
									Token.Append('\f');
									break;
								case 'u':
									string s = aJSON.Substring(i + 1, 4);
									Token.Append((char)int.Parse(s, NumberStyles.AllowHexSpecifier));
									i += 4;
									break;
								default:
									Token.Append(C);
									break;
							}
						}
						break;
					case '/':
						if (allowLineComments && !QuoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
						{
							while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r') ;
							break;
						}
						Token.Append(aJSON[i]);
						break;
					case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
						break;

					default:
						Token.Append(aJSON[i]);
						break;
				}
				++i;
			}
			if (QuoteMode)
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");

			return ctx ?? ParseElement(Token.ToString(), TokenIsQuoted);
		}
	}
	// End of JSONNode

	public partial class JSONArray : JSONNode
	{
		private readonly List<JSONNode> m_List = new List<JSONNode>();
		private bool inline = false;
		public override bool Inline
		{
			get { return inline; }
			set { inline = value; }
		}

		public override JSONNodeType Tag => JSONNodeType.Array;
		public override bool IsArray => true;

		public override IEnumerator<KeyValuePair<string, JSONNode>> GetEnumerator() => m_List.Select(i => new KeyValuePair<string, JSONNode>(string.Empty, i)).GetEnumerator();
		public override IEnumerable<JSONNode> Values => m_List;
		public override IEnumerable<JSONNode> Children => m_List;

		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_List.Count)
					return new JSONLazyCreator(this);
				return m_List[aIndex];
			}
			set
			{
				if (value == null)
					value = JSONNull.CreateOrGet();
				if (aIndex < 0 || aIndex >= m_List.Count)
					m_List.Add(value);
				else
					m_List[aIndex] = value;
			}
		}

		public override JSONNode this[string aKey]
		{
			get { return new JSONLazyCreator(this); }
			set { m_List.Add(value ?? JSONNull.CreateOrGet()); }
		}

		public override int Count => m_List.Count;

		public override void Add(string aKey, JSONNode aItem)
		{
			m_List.Add(aItem ?? JSONNull.CreateOrGet());
		}

		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
				return null;
			JSONNode tmp = m_List[aIndex];
			m_List.RemoveAt(aIndex);
			return tmp;
		}

		public override JSONNode Remove(JSONNode aNode)
		{
			m_List.Remove(aNode);
			return aNode;
		}

		public override void Clear()
		{
			m_List.Clear();
		}

		public override JSONNode Clone()
		{
			var node = new JSONArray();
			node.m_List.Capacity = m_List.Capacity;
			foreach (var n in m_List)
				node.Add(n?.Clone());
			return node;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('[');
			int count = m_List.Count;
			if (inline)
				aMode = JSONTextMode.Compact;
			for (int i = 0; i < count; i++)
			{
				if (i > 0)
					aSB.Append(',');
				if (aMode == JSONTextMode.Indent)
					aSB.AppendLine().Append(IndentChar, aIndent + aIndentInc);
				m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JSONTextMode.Indent)
				aSB.AppendLine().Append(IndentChar, aIndent);
			aSB.Append(']');
		}
	}
	// End of JSONArray

	public partial class JSONObject : JSONNode
	{
		private readonly Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>(AllowDuplicateKeys ? DuplicateKeysComparer.Default : null);

		private bool inline = false;
		public override bool Inline
		{
			get { return inline; }
			set { inline = value; }
		}

		public override JSONNodeType Tag => JSONNodeType.Object;
		public override bool IsObject => true;

		public override IEnumerator<KeyValuePair<string, JSONNode>> GetEnumerator() => m_Dict.GetEnumerator();
		public override IEnumerable<string> Keys => m_Dict.Keys;
		public override IEnumerable<JSONNode> Values => m_Dict.Values;
		public override IEnumerable<JSONNode> Children => m_Dict.Values;

		public override JSONNode this[string aKey]
		{
			get
			{
				if (m_Dict.TryGetValue(aKey, out JSONNode outJsonNode))
					return outJsonNode;
				else
					return new JSONLazyCreator(this, aKey);
			}
			set { m_Dict[aKey] = value ?? JSONNull.CreateOrGet(); }
		}

		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return null;
				return m_Dict.ElementAt(aIndex).Value;
			}
			set
			{
				if (value == null)
					value = JSONNull.CreateOrGet();
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return;
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}

		public override int Count => m_Dict.Count;

		public override void Add(string aKey, JSONNode aItem)
		{
			if (aItem == null)
				aItem = JSONNull.CreateOrGet();

			if (aKey != null)
				m_Dict[aKey] = aItem;
			else
				m_Dict.Add(Guid.NewGuid().ToString(), aItem);
		}

		public override JSONNode Remove(string aKey)
		{
			if (m_Dict.TryGetValue(aKey, out JSONNode tmp))
				m_Dict.Remove(aKey);
			return tmp;
		}

		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return null;
			var item = m_Dict.ElementAt(aIndex);
			m_Dict.Remove(item.Key);
			return item.Value;
		}

		public override JSONNode Remove(JSONNode aNode)
		{
			var item = m_Dict.FirstOrDefault(k => k.Value == aNode);
			if (item.Key != null)
			{
				m_Dict.Remove(item.Key);
				return aNode;
			}
			return null;
		}

		public override void Clear()
		{
			m_Dict.Clear();
		}

		public override JSONNode Clone()
		{
			var node = new JSONObject();
			foreach (var n in m_Dict)
			{
				node.Add(n.Key, n.Value.Clone());
			}
			return node;
		}

		public override bool HasKey(string aKey)
		{
			return m_Dict.ContainsKey(aKey);
		}

		public override JSONNode GetValueOrDefault(string aKey, JSONNode aDefault)
		{
			if (m_Dict.TryGetValue(aKey, out JSONNode res))
				return res;
			return aDefault;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('{');
			bool first = true;
			if (inline)
				aMode = JSONTextMode.Compact;
			foreach (var k in m_Dict)
			{
				if (!first)
					aSB.Append(',');
				first = false;
				if (aMode == JSONTextMode.Indent)
					aSB.AppendLine().Append(IndentChar, aIndent + aIndentInc);
				aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
				if (aMode == JSONTextMode.Compact)
					aSB.Append(':');
				else
					aSB.Append(": ");
				k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JSONTextMode.Indent)
				aSB.AppendLine().Append(IndentChar, aIndent);
			aSB.Append('}');
		}

		sealed class DuplicateKeysComparer : IEqualityComparer<string>
		{
			public static DuplicateKeysComparer Default => comparer ?? (comparer = new DuplicateKeysComparer());
			public static DuplicateKeysComparer comparer;

			DuplicateKeysComparer() { }

			public bool Equals(string x, string y) => false;
			public int GetHashCode(string obj) => obj.GetHashCode();
		}
	}
	// End of JSONObject

	public partial class JSONString : JSONNode
	{
		private string m_Data;

		public override JSONNodeType Tag => JSONNodeType.String;
		public override bool IsString => true;

		public override string Value
		{
			get { return m_Data; }
			set
			{
				m_Data = value;
			}
		}

		public JSONString(string aData)
		{
			m_Data = aData;
		}
		public override JSONNode Clone()
		{
			return new JSONString(m_Data);
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
		}
		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;
			if (obj is string s)
				return m_Data == s;
			JSONString s2 = obj as JSONString;
			if (s2 != null)
				return m_Data == s2.m_Data;
			return false;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
		public override void Clear()
		{
			m_Data = "";
		}
	}
	// End of JSONString

	public partial class JSONNumber : JSONNode
	{
		private double m_Data;

		public override JSONNodeType Tag => JSONNodeType.Number;
		public override bool IsNumber => true;

		public override string Value
		{
			get { return m_Data.ToString(CultureInfo.InvariantCulture); }
			set
			{
				if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
					m_Data = v;
			}
		}

		public override double AsDouble
		{
			get { return m_Data; }
			set { m_Data = value; }
		}
		public override int AsInt
		{
			get { return (int)m_Data; }
			set { m_Data = value; }
		}
		public override float AsFloat
		{
			get { return (float)m_Data; }
			set { m_Data = value; }
		}
		public override long AsLong
		{
			get { return (long)m_Data; }
			set { m_Data = value; }
		}
		public override ulong AsULong
		{
			get { return (ulong)m_Data; }
			set { m_Data = value; }
		}

		public JSONNumber(double aData)
		{
			m_Data = aData;
		}

		public JSONNumber(string aData)
		{
			Value = aData;
		}

		public override JSONNode Clone()
		{
			return new JSONNumber(m_Data);
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append(Value.ToString(CultureInfo.InvariantCulture));
		}
		private static bool IsNumeric(object value)
		{
			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return true;
			}
			return false;
		}
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (base.Equals(obj))
				return true;
			if (obj is JSONNumber s2)
				return m_Data == s2.m_Data;
			if (IsNumeric(obj))
				return Convert.ToDouble(obj) == m_Data;
			return false;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
		public override void Clear()
		{
			m_Data = 0;
		}
	}
	// End of JSONNumber

	public partial class JSONBool : JSONNode
	{
		private bool m_Data;

		public override JSONNodeType Tag => JSONNodeType.Boolean;
		public override bool IsBoolean => true;

		public override string Value
		{
			get { return m_Data.ToString(); }
			set
			{
				if (bool.TryParse(value, out bool v))
					m_Data = v;
			}
		}
		public override bool AsBool
		{
			get { return m_Data; }
			set { m_Data = value; }
		}

		public JSONBool(bool aData)
		{
			m_Data = aData;
		}

		public JSONBool(string aData)
		{
			Value = aData;
		}

		public override JSONNode Clone()
		{
			return new JSONBool(m_Data);
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append(m_Data ? "true" : "false");
		}
		public override bool Equals(object obj)
		{
			return obj is bool v && m_Data == v;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
		public override void Clear()
		{
			m_Data = false;
		}
	}
	// End of JSONBool

	public partial class JSONNull : JSONNode
	{
		static readonly JSONNull m_StaticInstance = new JSONNull();
		public static bool reuseSameInstance = true;
		public static JSONNull CreateOrGet()
		{
			return reuseSameInstance ? m_StaticInstance : new JSONNull();
		}
		private JSONNull() { }

		public override JSONNodeType Tag => JSONNodeType.NullValue;
		public override bool IsNull => true;

		public override string Value
		{
			get { return "null"; }
			set { }
		}
		public override bool AsBool
		{
			get { return false; }
			set { }
		}

		public override JSONNode Clone()
		{
			return CreateOrGet();
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is JSONNull;
		}
		public override int GetHashCode()
		{
			return 0;
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONNull

	internal partial class JSONLazyCreator : JSONNode
	{
		private JSONNode m_Node = null;
		private readonly string m_Key = null;
		public override JSONNodeType Tag => JSONNodeType.None;

		public JSONLazyCreator(JSONNode aNode)
		{
			m_Node = aNode;
			m_Key = null;
		}

		public JSONLazyCreator(JSONNode aNode, string aKey)
		{
			m_Node = aNode;
			m_Key = aKey;
		}

		private T Set<T>(T aVal) where T : JSONNode
		{
			if (m_Key == null)
				m_Node.Add(aVal);
			else
				m_Node.Add(m_Key, aVal);
			m_Node = null; // Be GC friendly.
			return aVal;
		}

		public override JSONNode this[int aIndex]
		{
			get { return new JSONLazyCreator(this); }
			set { Set(new JSONArray()).Add(value); }
		}

		public override JSONNode this[string aKey]
		{
			get { return new JSONLazyCreator(this, aKey); }
			set { Set(new JSONObject()).Add(aKey, value); }
		}

		public override void Add(JSONNode aItem)
		{
			Set(new JSONArray()).Add(aItem);
		}

		public override void Add(string aKey, JSONNode aItem)
		{
			Set(new JSONObject()).Add(aKey, aItem);
		}

		public static bool operator ==(JSONLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return ReferenceEquals(a, b);
		}

		public static bool operator !=(JSONLazyCreator a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return true;
			return ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override int AsInt
		{
			get { Set(new JSONNumber(0)); return 0; }
			set { Set(new JSONNumber(value)); }
		}

		public override float AsFloat
		{
			get { Set(new JSONNumber(0)); return 0.0f; }
			set { Set(new JSONNumber(value)); }
		}

		public override double AsDouble
		{
			get { Set(new JSONNumber(0)); return 0.0; }
			set { Set(new JSONNumber(value)); }
		}

		public override long AsLong
		{
			get
			{
				if (longAsString)
					Set(new JSONString("0"));
				else
					Set(new JSONNumber(0));
				return 0L;
			}
			set
			{
				if (longAsString)
					Set(new JSONString(value.ToString(CultureInfo.InvariantCulture)));
				else
					Set(new JSONNumber(value));
			}
		}

		public override ulong AsULong
		{
			get
			{
				if (longAsString)
					Set(new JSONString("0"));
				else
					Set(new JSONNumber(0));
				return 0L;
			}
			set
			{
				if (longAsString)
					Set(new JSONString(value.ToString(CultureInfo.InvariantCulture)));
				else
					Set(new JSONNumber(value));
			}
		}

		public override bool AsBool
		{
			get { Set(new JSONBool(false)); return false; }
			set { Set(new JSONBool(value)); }
		}

		public override JSONArray AsArray => Set(new JSONArray());

		public override JSONObject AsObject => Set(new JSONObject());
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONLazyCreator
}
