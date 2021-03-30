using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Extensions
{
    public static class ModelItemExtensions
    {
        private static Type VariablesCollectionType = typeof(Collection<Variable>);

        private static Type CodeActivityType = typeof(CodeActivity);

        private static Type GenericCodeActivityType = typeof(CodeActivity<>);

        private static Type AsyncCodeActivityType = typeof(AsyncCodeActivity);

        private static Type GenericAsyncCodeActivityType = typeof(AsyncCodeActivity<>);

        public static ModelItemCollection GetVariableCollection(this ModelItem element)
        {
            if (element != null)
            {
                Type itemType = element.ItemType;
                if (!CodeActivityType.IsAssignableFrom(itemType) && !GenericAsyncCodeActivityType.IsAssignableFrom(itemType) && !AsyncCodeActivityType.IsAssignableFrom(itemType) && !GenericAsyncCodeActivityType.IsAssignableFrom(itemType))
                {
                    ModelProperty modelProperty = element.Properties["Variables"];
                    if (modelProperty != null && modelProperty.PropertyType == VariablesCollectionType)
                    {
                        return modelProperty.Collection;
                    }
                }
            }
            return null;
        }

        public static ModelItem GetVariableScopeElement(this ModelItem element)
        {
            while (element != null && element.GetVariableCollection() == null)
            {
                element = element.Parent;
            }
            return element;
        }

        public static bool IsSequence(this ModelItem modelItem)
        {
            return modelItem.ItemType == typeof(Sequence);
        }

        public static bool IsFlowchart(this ModelItem modelItem)
        {
            return modelItem.ItemType == typeof(Flowchart);
        }

        public static bool IsStateMachine(this ModelItem modelItem)
        {
            return modelItem.ItemType == typeof(StateMachine);
        }

        public static ModelItem GetParent(this ModelItem modelItem,Predicate<ModelItem> predicate = null)
        {
            ModelItem parentModelItem = modelItem?.Parent;
            while (parentModelItem != null &&!predicate(parentModelItem))
            {
                parentModelItem = parentModelItem.Parent;
            }
            return parentModelItem;
        }

        public static bool CanAddItem(this ModelItem modelItem, object item)
        {
            if (item == null)
            {
                return false;
            }
            if (item is Activity||item is IActivityTemplateFactory)
            {
                if (!modelItem.IsSequence())
                {
                    return modelItem.IsFlowchart();
                }
                return true;
            }
            if (item is State || item is FinalState)
            {
                return modelItem.IsStateMachine();
            }
            if (item is FlowNode)
            {
                return modelItem.IsFlowchart();
            }
            return false;
        }

        public static int IndexOf(this ModelItem parentModelItem, ModelItem childItem)
        {
            if (childItem != null)
            {
                if (parentModelItem.IsSequence())
                {
                    return parentModelItem.Properties["Activities"].Collection.IndexOf(childItem);
                }
                if (parentModelItem.IsFlowchart())
                {
                    return parentModelItem.Properties["Nodes"].Collection.IndexOf(childItem);
                }
            }
            return -1;
        }

        public static bool IsContainer(this ModelItem modelItem)
        {
            return modelItem.IsFlowchart() || modelItem.IsSequence() || modelItem.IsStateMachine();
        }

        public static ModelItem AddItem(this ModelItem modelItem, object item, int index = -1)
        {
            if (!modelItem.CanAddItem(item))
            {
                return null;
            }
            ModelItem result = null;
            if (modelItem.IsSequence())
            {
                Activity activity = item.GetActivity();
                if (activity != null)
                {
                    ModelItemCollection collection = modelItem.Properties["Activities"].Collection;
                    result = ((index == -1) ? collection.Add(activity) : collection.Insert(index, activity));
                }
            }
            else if (modelItem.IsFlowchart())
            {
                FlowNode flowNode = item as FlowNode;
                if (flowNode == null)
                {
                    Activity activity = item.GetActivity();
                    if (activity != null)
                    {
                        flowNode = new FlowStep
                        {
                            Action = activity
                        };
                    }
                }
                if (flowNode != null)
                {
                    ModelItemCollection collection = modelItem.Properties["Nodes"].Collection;
                    result = ((index == -1) ? collection.Add(flowNode) : collection.Insert(index, flowNode));
                }
            }
            else if (modelItem.IsStateMachine() && (item is State || item is FinalState))
            {
                State state = (item as State) ?? new State
                {
                    IsFinal = true,
                    DisplayName = "FinalState"
                };
                if(string.IsNullOrWhiteSpace(state.DisplayName))
                {
                    state.DisplayName = "State1";
                }
                ModelItemCollection collection = modelItem.Properties["States"].Collection;
                result = ((index == -1) ? collection.Add(state) : collection.Insert(index, state));
            }
            return result;
        }

        public static IEnumerable<ModelItem> GetImports(this ModelItem modelItem)
        {
            IEnumerable<ModelItem> enumerable = modelItem?.Properties["Imports"]?.Collection;
            return enumerable ?? Enumerable.Empty<ModelItem>();
        }

        public static IEnumerable<ModelItem> GetProperties(this ModelItem modelItem)
        {
            IEnumerable<ModelItem> enumerable = modelItem?.Properties["Properties"]?.Collection;
            return enumerable ?? Enumerable.Empty<ModelItem>();
        }

        /// <summary>
        /// 遍历所有的Activities，然后执行相应的操作，目前暂未使用
        /// </summary>
        /// <param name="rootItem"></param>
        /// <param name="action"></param>
        public static void ProcessActivities(this ModelItem rootItem, Action<ModelItem> action)
        {
            if (rootItem == null)
            {
                return;
            }

            action(rootItem);

            foreach (var modelProperty in rootItem.Properties)
            {
                if (typeof(Activity).IsAssignableFrom(modelProperty.PropertyType) ||
                  typeof(FlowNode).IsAssignableFrom(modelProperty.PropertyType))
                {
                    ProcessActivities(modelProperty.Value, action);
                }
                else if (modelProperty.PropertyType.IsGenericType &&
                  modelProperty.PropertyType.GetGenericTypeDefinition() == typeof(Collection<>) &&
                  modelProperty.Collection != null)
                {
                    foreach (var activityModel in modelProperty.Collection)
                    {
                        ProcessActivities(activityModel, action);
                    }
                }
            }
        }

        public static T Get<T>(this ModelItem modelItem)
        {
            try
            {
                return (T)modelItem.GetCurrentValue();
            }
            catch
            {
                return default;
            }
        }

        public static Predicate<Type> AllActivitiesPredicate => type => (typeof(Activity).IsAssignableFrom(type) ^ (typeof(ITextExpression).IsAssignableFrom(type) || typeof(IValueSerializableExpression).IsAssignableFrom(type))) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FlowSwitch<>)) || type == typeof(FlowDecision);

        public static List<ModelItem> GetActivities(this ModelService modelService, ModelItem startingPoint)
        {
            List<ModelItem> list = new List<ModelItem>();
            if (((modelService?.Root.GetCurrentValue() as ActivityBuilder)?.Implementation ?? (modelService?.Root.GetCurrentValue() as Activity)) != null)
            {
                if (startingPoint == null)
                {
                    startingPoint = modelService.Root;
                }
                list.AddRange(modelService.Find(startingPoint, AllActivitiesPredicate));
            }
            return list;
        }
        private static readonly string[] s_updatableProperties = new string[3]
        {
            "Expression",
            "Condition",
            "CompletionCondition"
        };
        public static void WalkModelItemUpdateableProperties(this ModelItem modelItem, Action<ModelItem> propertyAction)
        {
            if (modelItem == null)
            {
                return;
            }
            string[] array = s_updatableProperties;
            foreach (string name in array)
            {
                if (!(modelItem.Properties.Find(name) == null))
                {
                    ModelItem value = modelItem.Properties[name].Value;
                    propertyAction(value);
                }
            }
        }

        public static void RenameTokenInPropertyExpression(this ModelItem modelItem, Func<ITextExpression, string> replaceTokenInExpression)
        {
            modelItem.WalkModelItemUpdateableProperties(delegate (ModelItem propertyModelItem)
            {
                ITextExpression textExpression = propertyModelItem?.GetCurrentValue() as ITextExpression;
                if (textExpression != null)
                {
                    try
                    {
                        string text = replaceTokenInExpression(textExpression);
                        ModelProperty modelProperty = propertyModelItem.Properties.Find("ExpressionText");
                        if (text != null && modelProperty != null)
                        {
                            modelProperty.ComputedValue = text;
                        }
                    }
                    catch (Exception exception)
                    {
                        //exception.Trace();
                    }
                }
            });
        }

        public static void RenameVariable(this ModelItem modelItem, Variable variable, string oldVariableName)
        {
            modelItem.RenameTokenInPropertyExpression((ITextExpression expression) => variable.RenameLocationInExpression(expression, oldVariableName, (Variable x) => x == variable));
        }

        public static string RenameLocationInExpression<T>(this T location, ITextExpression expression, string oldVariableName, Func<T, bool> comparer) where T : LocationReference
        {
            expression = (expression ?? throw new ArgumentNullException("expression"));
            location = (location ?? throw new ArgumentNullException("location"));
            string expressionText = expression.ExpressionText;
            if (!location.IsUsedIn(expression, comparer) || expressionText == null)
            {
                return null;
            }
            return ProcessExpressionText(expressionText, oldVariableName, location.Name);
        }
        public static void RenameArgument(this ModelItem modelItem, LocationReference location, string oldArgumentName)
        {
            RuntimeArgument argument = location as RuntimeArgument;
            if (argument != null)
            {
                modelItem.RenameTokenInPropertyExpression((ITextExpression expression) => argument.RenameLocationInExpression(expression, oldArgumentName, comparer1));
                return;
            }
            DelegateInArgument delegateArgument = location as DelegateInArgument;
            if (delegateArgument != null)
            {
                modelItem.RenameTokenInPropertyExpression((ITextExpression expression) => delegateArgument.RenameLocationInExpression(expression, oldArgumentName, comparer2));
            }
            bool comparer1(RuntimeArgument x)
            {
                if (x.Name == oldArgumentName && x.Type == argument.Type)
                {
                    return x.Direction == argument.Direction;
                }
                return false;
            }
            bool comparer2(LocationReference x)
            {
                return x.Type == location.Type;
            }
        }

        internal class ConstantNodeExpressionVisitor<T> : ExpressionVisitor
        {
            private ICollection<T> _listOfNodes;

            public ConstantNodeExpressionVisitor(ICollection<T> listOfNodes)
            {
                _listOfNodes = listOfNodes;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                object obj = node?.Value;
                if (obj is T)
                {
                    T item = (T)obj;
                    _listOfNodes.Remove(item);
                    _listOfNodes.Add(item);
                }
                return base.VisitConstant(node);
            }
        }

        public static bool IsUsedIn<T>(this T variable, ITextExpression expression, Func<T, bool> comparer) where T : LocationReference
        {
            try
            {
                List<T> list = new List<T>();
                new ConstantNodeExpressionVisitor<T>(list).Visit(expression.GetExpressionTree());
                if (!list.Any(comparer))
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                //exception.Trace();
            }
            return true;
        }

        public static string ReplaceToken(this string expressionText, string oldToken, string newToken)
        {
            bool flag = false;
            string[] array = Regex.Split(expressionText, "(\\.)|(=)|(\\+)|(-)|(\\*)|(<)|(>)|(=)|(&)|(\\s)|(\\()|(\\))");
            string text = string.Empty;
            string[] array2 = array;
            foreach (string text2 in array2)
            {
                if (string.Compare(text2, oldToken, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    text += newToken;
                    flag = true;
                }
                else
                {
                    text += text2;
                }
            }
            if (!flag)
            {
                return null;
            }
            return text;
        }

        public static string ProcessExpressionText(string expressionText, string oldValue, string newValue)
        {
            MatchCollection matchCollection = new Regex("\\\"[^\\\"]*\\\"", RegexOptions.IgnoreCase).Matches(expressionText);
            if (matchCollection.Count == 0)
            {
                return expressionText.ReplaceToken(oldValue, newValue);
            }
            bool hasReplacements = false;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < matchCollection.Count; i++)
            {
                Match match = matchCollection[i];
                if (i == 0)
                {
                    string text = expressionText.Substring(0, match.Index);
                    string newSubString2 = text.ReplaceToken(oldValue, newValue);
                    ProcessSubString(text, newSubString2);
                }
                result.Append(match.Value);
                Match match2 = match.NextMatch();
                int length = match2.Success ? (match2.Index - (match.Index + match.Length)) : (expressionText.Length - (match.Index + match.Length));
                string text2 = expressionText.Substring(match.Index + match.Length, length);
                string newSubString3 = text2.ReplaceToken(oldValue, newValue);
                ProcessSubString(text2, newSubString3);
            }
            if (!hasReplacements)
            {
                return null;
            }
            return result.ToString();
            void ProcessSubString(string oldSubstring, string newSubString)
            {
                if (newSubString != null)
                {
                    hasReplacements = true;
                    result.Append(newSubString);
                }
                else
                {
                    result.Append(oldSubstring);
                }
            }
        }



        public static bool IsExpandableProperty(this Type propertyType)
        {
            try
            {
                TypeConverterAttribute typeConverterAttribute = TypeDescriptor.GetAttributes(propertyType).OfType<TypeConverterAttribute>().FirstOrDefault();
                if (typeConverterAttribute == null)
                {
                    return false;
                }
                Type type = Type.GetType(typeConverterAttribute.ConverterTypeName);
                if (type != null)
                {
                    return type.IsAssignableFrom(typeof(ExpandableObjectConverter)) || type.IsSubclassOf(typeof(ExpandableObjectConverter));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            return false;
        }

        public static void WalkRecursive(this ModelItem modelItem, Action<ModelItem> action, HashSet<ModelItem> visitedItems = null)
        {
            if (modelItem != null)
            {
                if (visitedItems == null)
                {
                    visitedItems = new HashSet<ModelItem>();
                }
                if (!visitedItems.Contains(modelItem))
                {
                    action(modelItem);
                    visitedItems.Add(modelItem);
                    foreach (ModelProperty property in modelItem.Properties)
                    {
                        if (property.IsSet)
                        {
                            if (property.IsCollection)
                            {
                                foreach (ModelItem item in property.Collection)
                                {
                                    item.WalkRecursive(action, visitedItems);
                                }
                            }
                            else if (property.IsDictionary)
                            {
                                foreach (ModelItem value in property.Dictionary.Values)
                                {
                                    value.WalkRecursive(action, visitedItems);
                                }
                            }
                            else
                            {
                                Type baseType = property.PropertyType.BaseType;
                                if (typeof(Argument).IsAssignableFrom(baseType) || typeof(ActivityWithResult).IsAssignableFrom(baseType))
                                {
                                    property.Value.WalkRecursive(action, visitedItems);
                                }
                                else if (property.PropertyType.IsExpandableProperty())
                                {
                                    property.Value.WalkRecursive(action, visitedItems);
                                }
                                action(property.Value);
                            }
                        }
                    }
                }
            }
        }

        public static ModelItem GetParentContainer(this ModelItem targetItem)
        {
            ModelItem modelItem = targetItem?.Parent;
            while (modelItem != null && !modelItem.IsActivitiesContainer() && !modelItem.IsSequence() && !modelItem.IsFlowchart() && !modelItem.IsStateMachine())
            {
                modelItem = modelItem.Parent;
            }
            return modelItem;
        }
        public static ModelItemCollection GetActivityContainerCollection(ModelItem modelItem)
        {
            return ((modelItem?.View?.GetType().GetProperty("ContainerInfo"))?.GetValue(modelItem.View) as ModelItemContainerInfo)?.GetCollection();
        }
        public static bool IsActivitiesContainer(this ModelItem modelItem)
        {
            try
            {
                return GetActivityContainerCollection(modelItem) != null;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
        }
        public static Argument CreateArgument(this Variable variable, Type expressionType, ArgumentDirection direction)
        {
            Argument argument = Argument.Create(variable.Type, direction);
            object obj = Activator.CreateInstance(expressionType, variable.Name);
            argument.Expression = (obj as ActivityWithResult);
            return argument;
        }

        internal static bool IsType<T>(this ModelItem modelItem)
        {
            return typeof(T).IsAssignableFrom(modelItem.ItemType);
        }
        public static bool IsOutArgument(this ModelItem modelItem)
        {
            return modelItem.IsType<OutArgument>();
        }
    }
    public abstract class ModelItemContainerInfo
    {
        public const string PropertyName = "ContainerInfo";

        public abstract ModelItemCollection GetCollection();
    }
}
