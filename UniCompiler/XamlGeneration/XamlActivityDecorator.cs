using System;
using System.Activities;
using System.Activities.Statements;
using Microsoft.VisualBasic.Activities;

namespace UniCompiler.XamlGeneration
{
    public static class XamlActivityDecorator
    {
        private const string CwdVariableName = "____currentDirectory____";

        internal static Activity DecorateWithCurrentDirectoryResolver(this Activity activity, string typeName)
        {
            if (activity == null)
            {
                return null;
            }
            Variable variable = Variable.Create("____currentDirectory____", typeof(string), VariableModifiers.None);
            TryCatch tryCatch = new TryCatch
            {
                Try = new Sequence
                {
                    Activities =
                    {
                        (Activity)new Assign
                        {
                            To = new OutArgument<string>(variable),
                            Value = new InArgument<string>(new VisualBasicValue<string>("System.Environment.CurrentDirectory"))
                        },
                        (Activity)new Assign
                        {
                            To = new OutArgument<string>(new VisualBasicReference<string>("System.Environment.CurrentDirectory")),
                            Value = new InArgument<string>(new VisualBasicValue<string>("new FileInfo(Type.GetType(\"" + typeName + "\").Assembly.Location).Directory.FullName"))
                        },
                        activity
                    }
                },
                Finally = new Sequence
                {
                    Activities =
                    {
                        (Activity)new Assign
                        {
                            To = new OutArgument<string>(new VisualBasicReference<string>("System.Environment.CurrentDirectory")),
                            Value = new InArgument<string>(new VisualBasicValue<string>("____currentDirectory____"))
                        }
                    }
                }
            };
            tryCatch.Catches.Add(new Catch<Exception>
            {
                Action = new ActivityAction<Exception>
                {
                    Handler = new Sequence
                    {
                        Activities =
                        {
                            (Activity)new Assign
                            {
                                To = new OutArgument<string>(new VisualBasicReference<string>("System.Environment.CurrentDirectory")),
                                Value = new InArgument<string>(new VisualBasicValue<string>("____currentDirectory____"))
                            },
                            (Activity)new Rethrow()
                        }
                    }
                }
            });
            return new Sequence
            {
                Variables =
                {
                    variable
                },
                Activities =
                {
                    (Activity)tryCatch
                }
            };
        }
	}
}
