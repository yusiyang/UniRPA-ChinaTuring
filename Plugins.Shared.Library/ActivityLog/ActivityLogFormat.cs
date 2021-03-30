using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.ActivityLog
{
    public class ActivityLogFormat : ICustomFormatter, IFormatProvider
    {
		private static string _separateChars= "[]{}【】(),;\r\n ";

		private const string ActivityName = "活动名称";

		private const string ActivityType = "活动类型";

		private const char PairSeparator = ':';

		private const char TypeSeparator = '\0';

		private const string Parameters = "参数";

		private const char ParameterPairSeparator = ':';

		private readonly static string _oneLine = Environment.NewLine;

		public const string ParameterSeparator = "$&&$";

		public object GetFormat(Type formatType)
        {
            if(formatType==typeof(ICustomFormatter)||formatType==typeof(ActivityLog))
            {
                return this;
            }
            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
			if (arg == null)
			{
				throw new ArgumentNullException("arg");
			}
			string result = null;
			if (arg.GetType() == typeof(IFormattable))
			{
				result = ((IFormattable)arg).ToString(format, formatProvider);
			}
			else if (!string.IsNullOrEmpty(format))
			{
				var activityLog = arg as ActivityLog;
				if (activityLog != null)
				{
					var stringBuilder = new StringBuilder();
					for (int i = 0; i < format.Length; i++)
					{
						string text = Format(format[i], activityLog);
						stringBuilder.Append(text);
					}
					result = stringBuilder.ToString();
				}
			}
			return result;
		}

		private static string Format(char c, ActivityLog activityLog)
		{
			if(_separateChars.IndexOf(c)>-1)
			{
				return c.ToString();
			}
			switch (c)
			{
				case 'N':
				case 'n':
					return $"{ActivityName}{PairSeparator}{activityLog.ActivityName}{TypeSeparator}";
				case 'T':
				case 't':
					return $"{ActivityType}{PairSeparator}{activityLog.ActivityType}{TypeSeparator}";
				case 'P':
				case 'p':
					return $"{Parameters}{PairSeparator}{TranslateParameters(activityLog.Parameters)}{TypeSeparator}";
			}
			throw new FormatException();
		}

		private static string TranslateParameters(IDictionary<string, object> parameters)
		{
			if(parameters==null||parameters.Count==0)
			{
				return "无";
			}
			var sb = new StringBuilder();
			foreach(var parameter in parameters)
			{
				sb.Append($"{parameter.Key}{ParameterPairSeparator}{TranslateValue(parameter.Value)}{ParameterSeparator}");
			}
			if(sb.Length>0)
			{
				sb = sb.Remove(sb.Length - ParameterSeparator.Length, ParameterSeparator.Length);
			}
			return sb.ToString();
		}

		private static string TranslateValue(object value)
		{
			if (value == null)
			{
				return "null";
			}

			if (value is string)
			{
				return string.Format("\"{0}\"", value.ToString());
			}

			if (value is Array)
			{
				var arr = value as Array;

				var childrenStr = "";
				int index = 0;
				foreach (var element in arr)
				{
					childrenStr += "  " + TranslateValue(element);

					if (index < arr.Length - 1)
					{
						childrenStr += ",";
					}
					else
					{
						childrenStr += " ";
					}

					childrenStr += _oneLine;
					index++;
				}
				
				return childrenStr;
			}

			return value.ToString();
		}

		public static bool TryParse(string activityStr,out ActivityLog activityLog)
		{
			if(string.IsNullOrWhiteSpace(activityStr))
			{
				throw new ArgumentNullException(nameof(activityStr));
			}

			var token = string.Empty;
			var value = string.Empty;
			var tokens = new List<string> { ActivityName, ActivityType, Parameters };

			var isToken = true;
			var hasToken = false;
			activityLog = new ActivityLog();

			for(var i=0;i<activityStr.Length;i++)
			{
				if (!hasToken && _separateChars.Contains(activityStr[i]))
				{
					continue;
				}

				if (isToken)
				{
					if (TryGetName(activityStr, ref i, out var name)&& tokens.Contains(name))
					{
						tokens.Remove(name);
						hasToken = true;
						if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(value))
						{
							SetValue(activityLog, token, value);
						}
						value = string.Empty;
						token = name;
						isToken = false;
					}
				}
				else
				{
					if(TypeSeparator == activityStr[i])
					{
						var j = i+1;
						if (TryGetName(activityStr, ref j, out var name))
						{
							isToken = true;
							continue;
						}
						else if(j<activityStr.Length)
						{
							return false;
						}
						else
						{
							break;
						}
					}
					else
					{
						value += activityStr[i];
					}
				}
			}

			if(tokens.Count==3)
			{
				return false;
			}

			if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(value))
			{
				SetValue(activityLog, token, value);
			}

			return true;
		}

		private static bool TryGetName(string activityStr,ref int position,out string name)
		{
			name = null;
			if(position>=activityStr.Length)
			{
				return false;
			}
			for(var i=position;i<activityStr.Length;i++)
			{
				if (_separateChars.Contains(activityStr[i]))
				{
					position += 1;
				}
				else
				{
					break;
				}
			}

			var subStr = activityStr.Substring(position);
			if(subStr.StartsWith(ActivityName + PairSeparator))
			{
				position += (ActivityName + PairSeparator).Length-1;
				name = ActivityName;
				return true;
			}

			if (subStr.StartsWith(ActivityType + PairSeparator))
			{
				position += (ActivityType + PairSeparator).Length - 1;
				name = ActivityType;
				return true;
			}

			if (subStr.StartsWith(Parameters + PairSeparator))
			{
				position += (Parameters + PairSeparator).Length - 1;
				name = Parameters;
				return true;
			}

			return false;
		}

		private static void SetValue(ActivityLog activityLog, string name,string value)
		{
			switch(name)
			{
				case ActivityName:
					activityLog.ActivityName = value;
					return;
				case ActivityType:
					activityLog.ActivityType = value;
					return;
				case Parameters:
					activityLog.ParameterStr = value;
					return;
			}
		}
	}
}
