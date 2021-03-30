using System;
using System.Activities;
using System.Activities.Presentation.Annotations;
using System.Activities.Presentation.Model;
using UniCompiler.WorkFlowServices;

namespace UniCompiler.Utilities
{
    internal static class WorkflowInformationHelper
    {
		[Serializable]
		private class WorkflowXamlInformation
		{
			public int Version
			{
				get;
				set;
			}

			public string HelpLink
			{
				get;
				set;
			}

			public string InitialTooltip
			{
				get;
				set;
			}
		}

		[Serializable]
		private class ActivityTypeInformation : IActivityTypeInformation
		{
			public int Version
			{
				get;
				set;
			}

			public string Tooltip
			{
				get;
				set;
			}

			public string InitialTooltip
			{
				get;
				set;
			}

			public string HelpLink
			{
				get;
				set;
			}
		}

		public static void AttachInformation(WorkflowInformation info, ActivityBuilder root, ModelTreeManager modelTree = null)
		{
			if (root != null)
			{
				ModelItem modelItem = modelTree?.GetModelItem(root.Implementation, shouldExpandModelTree: false);
				if (modelItem != null)
				{
					((dynamic)modelItem).AnnotationText = (string.IsNullOrEmpty(info?.Tooltip) ? null : info.Tooltip);
				}
				else if (root.Implementation != null)
				{
					Annotation.SetAnnotationText(root.Implementation, info?.Tooltip);
				}
				string serializedXamlInformation = GetSerializedXamlInformation(info);
				ModelItem modelItem2 = modelTree?.GetModelItem(root, shouldExpandModelTree: false);
				if (modelItem2 != null)
				{
					((dynamic)modelItem2).AnnotationText = serializedXamlInformation;
				}
				else
				{
					Annotation.SetAnnotationText(root, serializedXamlInformation);
				}
			}
		}

		private static string GetSerializedXamlInformation(WorkflowInformation info)
		{
			if (info != null)
			{
				return Base64SerializationHelper.Serialize(CreateWorkflowXamlInfo(info));
			}
			return null;
		}

		private static WorkflowXamlInformation CreateWorkflowXamlInfo(WorkflowInformation info)
		{
			return new WorkflowXamlInformation
			{
				Version = 1,
				HelpLink = info?.HelpLink,
				InitialTooltip = info.Tooltip
			};
		}

		public static WorkflowInformation ExtractInformation(ActivityBuilder activity)
		{
			if (activity == null)
			{
				return null;
			}
			WorkflowXamlInformation workflowXamlInformation = Base64SerializationHelper.Deserialize<WorkflowXamlInformation>(Annotation.GetAnnotationText(activity));
			string text = ((activity.Implementation == null) ? null : Annotation.GetAnnotationText(activity.Implementation)) ?? workflowXamlInformation?.InitialTooltip;
			if (workflowXamlInformation != null || text != null)
			{
				return new WorkflowInformation
				{
					Tooltip = text,
					HelpLink = workflowXamlInformation?.HelpLink
				};
			}
			return null;
		}

		public static IActivityTypeInformation DeserializeTypeInformation(string serializedInfo)
		{
			return Base64SerializationHelper.Deserialize<ActivityTypeInformation>(serializedInfo);
		}

		public static string SerializeAsTypeInformation(WorkflowInformation info)
		{
			if (info == null)
			{
				return null;
			}
			return Base64SerializationHelper.Serialize(CreateActivityTypeInfo(info));
		}

		private static ActivityTypeInformation CreateActivityTypeInfo(WorkflowInformation info)
		{
			return new ActivityTypeInformation
			{
				Version = 1,
				HelpLink = info.HelpLink,
				Tooltip = info.Tooltip
			};
		}

		public static void UpdateAttachedInformation(ActivityBuilder root, ModelItem implementation)
		{
			WorkflowXamlInformation workflowXamlInformation = Base64SerializationHelper.Deserialize<WorkflowXamlInformation>(Annotation.GetAnnotationText(root));
			if (workflowXamlInformation != null)
			{
				string text = ((dynamic)implementation)?.AnnotationText;
				if (implementation != null)
				{
					((dynamic)implementation).AnnotationText = null;
				}
				workflowXamlInformation.InitialTooltip = (text ?? workflowXamlInformation.InitialTooltip);
				AttachInformation(new WorkflowInformation
				{
					HelpLink = workflowXamlInformation.HelpLink,
					Tooltip = (text ?? workflowXamlInformation.InitialTooltip)
				}, root);
			}
		}
	}
}
