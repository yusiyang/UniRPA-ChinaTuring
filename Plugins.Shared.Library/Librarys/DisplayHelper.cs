using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public static class DisplayHelper
    {
		public static string Display(this string field,int length= 10000)
		{
			if (field.Length <= length)
			{
				return field;
			}
			return new StringBuilder().Append(field, 0, length).Append("...").ToString();
		}
	}
}
