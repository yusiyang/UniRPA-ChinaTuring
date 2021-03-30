using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlActivity
{
    public abstract class InterruptibleLoopBase : NativeActivity
    {
        protected bool breakRequested;

        protected sealed override bool CanInduceIdle => true;

        protected virtual void OnContinue(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.CancelChildren();
            Bookmark bookmark2;
            if ((bookmark2 = (value as Bookmark)) != null)
            {
                context.ResumeBookmark(bookmark2, value);
            }
        }

        protected virtual void OnBreak(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.CancelChildren();
            breakRequested = true;
            Bookmark bookmark2;
            if ((bookmark2 = (value as Bookmark)) != null)
            {
                context.ResumeBookmark(bookmark2, value);
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            breakRequested = false;
            Bookmark property = context.CreateBookmark(OnBreak, BookmarkOptions.NonBlocking);
            context.Properties.Add("BreakBookmark", property);
            Bookmark property2 = context.CreateBookmark(OnContinue, BookmarkOptions.MultipleResume | BookmarkOptions.NonBlocking);
            context.Properties.Add("ContinueBookmark", property2);
            StartLoop(context);
        }

        protected abstract void StartLoop(NativeActivityContext context);
	}
}
