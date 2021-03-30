using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.ViewState;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Extensions
{
    public static class ModelServiceExtensions
    {
        public static ModelItem FindActivityById(this ModelService modelService, string activityId)
        {
            var activities = modelService.Find(modelService.Root, typeof(Activity));
            return activities?.FirstOrDefault(a => a.Get<Activity>()?.Id == activityId);
        }

        public static ModelItem FindActivityByIdRef(this ModelService modelService, string idRef)
        {
            var activities = modelService.Find(modelService.Root, typeof(Activity));
            return activities?.FirstOrDefault(a => WorkflowViewState.GetIdRef(a.GetCurrentValue()) == idRef);
        }

        public static ModelItem FindArgument(this ModelService modelService,string argumentName)
        {
            var arguments = modelService.Root.Properties["Properties"]?.Collection;
            return arguments?.FirstOrDefault(a => (string)a.Properties["Name"].ComputedValue == argumentName);
        }

        public static ModelItem FindVariable(this ModelService modelService, string activityId, string variableName)
        {
            var activity = modelService.FindActivityById(activityId);
            var variables = activity?.Properties["Variables"]?.Collection;
            return variables?.FirstOrDefault(a => (string)a.Properties["Name"].ComputedValue == variableName);
        }

        public static ModelItem FindVariableByIdRef(this ModelService modelService, string idRef, string variableName)
        {
            var activity = modelService.FindActivityByIdRef(idRef);
            var variables = activity?.Properties["Variables"]?.Collection;
            return variables?.FirstOrDefault(a => (string)a.Properties["Name"].ComputedValue == variableName);
        }

        public static ModelItem FindImport(this ModelService modelService, string importName)
        {
            var imports = modelService.Root.Properties["Imports"]?.Collection;
            return imports?.FirstOrDefault(a => (string)a.Properties["Namespace"].ComputedValue == importName);
        }

        public static ModelProperty FindProperty(this ModelService modelService, string activityId, string propName)
        {
            var activity = modelService.FindActivityById(activityId);
            return activity?.Properties?.FirstOrDefault(a => (string)a.Name == propName);
        }

        public static ModelProperty FindPropertyByIdRef(this ModelService modelService, string idRef, string propName)
        {
            var activity = modelService.FindActivityByIdRef(idRef);
            return activity?.Properties?.FirstOrDefault(a => (string)a.Name == propName);
        }

        public static List<ModelItem> FindAllActivities(this ModelService modelService,Predicate<ModelItem> predicate=null)
        {
            var activities = modelService.Find(modelService.Root, typeof(Activity));
            if(predicate!=null)
            {
                activities = activities.Where(a => predicate(a));
            }
            return activities.ToList();
        }
    }
}
