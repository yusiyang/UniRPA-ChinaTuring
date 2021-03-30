using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 发送邮件活动
    /// </summary>
    public class SendOutlookMailActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "发送邮件";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var sendMailActivity = new MailActivity.SendMail();
            ((System.Activities.Activity)sendMailActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(sendMailActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(sendMailActivity.DisplayName):
                            ((System.Activities.Activity)sendMailActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(sendMailActivity.ContinueOnError):
                            sendMailActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(sendMailActivity.From):
                            sendMailActivity.From = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.Email):
                            sendMailActivity.Email = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.MailTopic):
                            sendMailActivity.MailTopic = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) }; ;
                            break;
                        case nameof(sendMailActivity.MailBody):
                            sendMailActivity.MailBody = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) }; ;
                            break;
                        case nameof(sendMailActivity.IsBodyHtml):
                            sendMailActivity.IsBodyHtml = (bool)value;
                            break;
                        case nameof(sendMailActivity.AttachFiles):
                            sendMailActivity.AttachFiles = new InArgument<string[]> { Expression = new VisualBasicValue<string[]>(CreateVariable<string[]>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.Server):
                            sendMailActivity.Server = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.Port):
                            sendMailActivity.Port = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) }; ;
                            break;
                        case nameof(sendMailActivity.Name):
                            sendMailActivity.Name = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) }; ;
                            break;
                        case nameof(sendMailActivity.Password):
                            sendMailActivity.Password = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) }; ;
                            break;
                        case nameof(sendMailActivity.Receivers_Bcc):
                            sendMailActivity.Receivers_Bcc = new InArgument<string[]> { Expression = new VisualBasicValue<string[]>(CreateVariable<string[]>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.Receivers_Cc):
                            sendMailActivity.Receivers_Cc = new InArgument<string[]> { Expression = new VisualBasicValue<string[]>(CreateVariable<string[]>(variables, key, value)) };
                            break;
                        case nameof(sendMailActivity.Receivers_To):
                            sendMailActivity.Receivers_To = new InArgument<string[]> { Expression = new VisualBasicValue<string[]>(CreateVariable<string[]>(variables, key, value)) };
                            break;
                    }
                }
            }
            return sendMailActivity;
        }
    }
}
