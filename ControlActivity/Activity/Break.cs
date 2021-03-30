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
    [Designer(typeof(BreakDesigner))]
    public class Break : NativeActivity
    {
        #region 属性分类：杂项

        [Browsable(false)]
        public string IcoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/System/Break.png";
            }
        }

        #endregion


        private class BreakConstraint : LoopChildConstraint
        {
        }

        internal const string BreakBookmark = "BreakBookmark";

        protected override bool CanInduceIdle => true;


		public Break()
        {
            DelegateInArgument<Break> argument = new DelegateInArgument<Break>
            {
                Name = "constraintArg"
            };
            DelegateInArgument<ValidationContext> delegateInArgument = new DelegateInArgument<ValidationContext>
            {
                Name = "validationContext"
            };
            base.Constraints.Add(new Constraint<Break>
            {
                Body = new ActivityAction<Break, ValidationContext>
                {
                    Argument1 = argument,
                    Argument2 = delegateInArgument,
                    Handler = new BreakConstraint
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
            Bookmark bookmark = (Bookmark)context.Properties.Find("BreakBookmark");
            if (bookmark != null)
            {
                Bookmark value = context.CreateBookmark();
                context.ResumeBookmark(bookmark, value);
            }
        }
	}
}
