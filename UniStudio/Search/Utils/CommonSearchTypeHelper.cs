using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;
using UniStudio.Search.Models;

namespace UniStudio.Search.Utils
{
    public static class CommonSearchTypeHelper
    {
        private static Dictionary<CommonSearchType, CommonDataType> _searchTypeDataTypeDic;

        static CommonSearchTypeHelper()
        {
            _searchTypeDataTypeDic = new Dictionary<CommonSearchType, CommonDataType>
            { 
                {CommonSearchType.CurrentFile, CommonDataType.Variable | CommonDataType.Property | CommonDataType.InArgument | CommonDataType.OutArgument
                    | CommonDataType.InOutArgument | CommonDataType.PropertyArgument | CommonDataType.DesignerActivity | CommonDataType.Import },
                { CommonSearchType.AllFiles,CommonDataType.Variable | CommonDataType.Property | CommonDataType.InArgument | CommonDataType.OutArgument
                    | CommonDataType.InOutArgument | CommonDataType.PropertyArgument | CommonDataType.DesignerActivity | CommonDataType.Import},
                {CommonSearchType.Activities,CommonDataType.Activity },
                {CommonSearchType.Variables,CommonDataType.Variable },
                {CommonSearchType.Arguments,CommonDataType.InArgument | CommonDataType.OutArgument | CommonDataType.InOutArgument | CommonDataType.PropertyArgument },
                {CommonSearchType.Imports,CommonDataType.Import },
                {CommonSearchType.ProjectFiles,CommonDataType.ProjectFile },
                {CommonSearchType.Dependencies,CommonDataType.Dependency },
                {CommonSearchType.Snippets,CommonDataType.Snippet }
            };
        }

        public static CommonDataType GetCommonDataType(this CommonSearchType commonSearchType)
        {
            if (!_searchTypeDataTypeDic.TryGetValue(commonSearchType, out var commonDataType))
            {
                return CommonDataType.Unknown;
            }
            return commonDataType;
        }

        public static IEnumerable<CommonSearchType> GetCommonSearchTypes(this SearchDataUnit searchDataUnit)
        {
            var result = _searchTypeDataTypeDic.Where(d => d.Value.HasFlag(searchDataUnit.CommonDataType));
            if(searchDataUnit.RelativeFilePath!=DocumentContext.Current.RelativeFilePath)
            {
                result = result.Where(d => d.Key != CommonSearchType.CurrentFile);
            }
            return result.Select(d => d.Key);
        }
    }
}
