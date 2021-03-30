using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniStudio.Search.Enums;

namespace UniStudio.Search.Operations
{
    public class LocateOperationFactory
    {
        private static Dictionary<CommonDataType, Type> _commonDataTypeOperationTypeDic;

        private static Dictionary<CommonDataType, ILocateOperation> _commonDataTypeOperationDic;

        static LocateOperationFactory()
        {
            _commonDataTypeOperationTypeDic = new Dictionary<CommonDataType, Type>
            {
                {CommonDataType.Variable,typeof(VariableLocateOperation)},
                {CommonDataType.Property,typeof(PropertyLocateOperation)},
                {CommonDataType.InArgument|CommonDataType.OutArgument|CommonDataType.InOutArgument|CommonDataType.PropertyArgument,typeof(ArgumentLocateOperation)},
                {CommonDataType.DesignerActivity,typeof(DesignerActivityLocateOperation)},
                {CommonDataType.Activity,typeof(ActivityLocateOperation)},
                {CommonDataType.Import,typeof(ImportLocateOperation)},
                {CommonDataType.ProjectFile,typeof(ProjectFileLocateOperation)},
                {CommonDataType.Dependency,typeof(DependencyLocateOperation)},
                {CommonDataType.Snippet,typeof(SnippetLocateOperation)}
            };
            _commonDataTypeOperationDic = new Dictionary<CommonDataType, ILocateOperation>();
        }

        public static ILocateOperation Create(CommonDataType commonDataType)
        {
            return _commonDataTypeOperationDic.Locking(d =>
            {
                if (!TryGetOperation(commonDataType, out var operation))
                {
                    if(!TryGetOperationType(commonDataType, out var openrationType))
                    {
                        throw new Exception($"没有找到{commonDataType.ToString()}对应的操作类");
                    }
                    operation = Activator.CreateInstance(openrationType) as ILocateOperation;
                    d.Add(commonDataType, operation);
                }
                return operation;
            });
        }

        private static bool TryGetOperationType(CommonDataType commonDataType, out Type operationType)
        {
            var key = _commonDataTypeOperationTypeDic.Keys.FirstOrDefault(k => (k & commonDataType) > 0);
            if (key > 0)
            {
                operationType = _commonDataTypeOperationTypeDic[key];
                return true;
            }
            operationType = null;
            return false;
        }

        private static bool TryGetOperation(CommonDataType commonDataType,out ILocateOperation operation)
        {
            var key = _commonDataTypeOperationDic.Keys.FirstOrDefault(k => (k & commonDataType) > 0);
            if(key>0)
            {
                operation = _commonDataTypeOperationDic[key];
                return true;
            }
            operation = null;
            return false;
        }
    }
}
