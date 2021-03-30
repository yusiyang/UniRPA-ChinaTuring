using ControlActivity.Designer;
using System;
using System.Activities;
using System.Activities.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlActivity
{
    [Designer(typeof(ContinueDesigner))]
    public class Continue : NativeActivity
    {
        #region 属性分类：杂项

        [Browsable(false)]
        public string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/System/Continue.png";
            }
        }

        #endregion


        private class ContinueConstraint : LoopChildConstraint
        {
        }
        internal const string ContinueBookmark = "ContinueBookmark";

        protected override bool CanInduceIdle => true;

        public Continue()
        {
            DelegateInArgument<Continue> argument = new DelegateInArgument<Continue>
            {
                Name = "constraintArg"
            };
            DelegateInArgument<ValidationContext> delegateInArgument = new DelegateInArgument<ValidationContext>
            {
                Name = "validationContext"
            };
            base.Constraints.Add(new Constraint<Continue>
            {
                Body = new ActivityAction<Continue, ValidationContext>
                {
                    Argument1 = argument,
                    Argument2 = delegateInArgument,
                    Handler = new ContinueConstraint
                    {
                        ParentChain = new GetParentChain
                        {
                            ValidationContext = delegateInArgument
                        }
                    }
                }
            });
        }

        protected override void Execute(NativeActivityContext context)
        {
            Bookmark bookmark = (Bookmark)context.Properties.Find("ContinueBookmark");
            if (bookmark != null)
            {
                Bookmark value = context.CreateBookmark();
                context.ResumeBookmark(bookmark, value);
            }
        }
    }
}
