using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 数据初始化
    /// </summary>
    public class DbDataInitializer
    {

        #region Fields

        private SqlSugarClient _client;
        private List<ActionKeyword> _actionKeywordEntities = new List<ActionKeyword>();
        private List<ObjectKeyword> _objectKeywordEntities = new List<ObjectKeyword>();

        #endregion

        #region Ctor

        public DbDataInitializer(SqlSugarClient client)
        {
            _client = client;
        }

        #endregion

        #region Initialize

        public void Initialize()
        {
            //ClearDb();

            //ActivityInit();
            //ActionKeywordInit();
            //ObjectKeywordInit();

            //Other Initial

            //HotKeyVirtualKeyInit();
        }

        #endregion

        #region Clear Db
        private void ClearDb()
        {
            _client.Deleteable<Activity>().ExecuteCommand();
            _client.Deleteable<ActivityProperty>().ExecuteCommand();
            _client.Deleteable<Behavior>().ExecuteCommand();
            _client.Deleteable<ActionKeyword>().ExecuteCommand();
            _client.Deleteable<ObjectKeyword>().ExecuteCommand(); 
            _client.Deleteable<ActionKeywordActivityMapping>().ExecuteCommand();
            _client.Deleteable<ObjectKeywordActivityMapping>().ExecuteCommand();
        }
        #endregion

        #region HotKeyVirtualKeyInit

        public void HotKeyVirtualKeyInit()
        {
            foreach (var virtualKey in Enum.GetValues(typeof(VirtualKey)))
            {
                var name = Enum.GetName(typeof(VirtualKey), virtualKey);
                var value = ((short)(VirtualKey)virtualKey).ToString();

                if (name == "SHIFT" || name == "ALT")
                {
                    continue;
                }

                var entity = new ParameterKeyword
                {
                    Name = name,
                };

                var mapping = new ParameterKeywordActivityPropertyMapping
                {
                    ParameterKeywordId = entity.Id,
                    ActivityPropertyId = "bb79617c-92fc-4ca2-8031-048aed4d5f7d"
                };
                _client.Insertable(entity).ExecuteCommand();
                _client.Insertable(mapping).ExecuteCommand();
            }
        }

        #endregion

        #region ActivityInit

        private void ActivityInit()
        {
            //Collection
            CellSetActivityInit();
            ExcelCreateActivityInit();
            ExtractDataActivityInit();
            FilterDataTableActivityInit();
            ForEachRowActivityInit();

            //ControlFlow
            ForEachActivityInit();
            IfActivityInit();
            WhileActivityInit();

            //File
            CopyFileActivityInit();
            EleExistsActivityInit();
            PathExistsActivityInit();
            GetTextActivityInit();
            MoveFileActivityInit();
            ReadTextFileActivityInit();
            WriteTextFileActivityInit();
            ReadRangeActivityInit();
            WriteRangeActivityInit();

            //General
            ClickActivityInit();
            GetDateTimeActivityInit();
            HotKeyActivityInit();
            OpenBrowserActivityInit();
            TypeIntoActivityInit();

            //Messaging
            SendOutlookMailActivityInit();

            //Primitives
            AssignActivityInit();
            DelayActivityInit();
            //WriteLineActivityInit();

            //Process
            OpenApplicationActivityInit();
            CloseApplicationActivityInit();
            StartProcessActivityInit();
            KillProcessActivityInit();
        }

        #endregion

        #region Collection

        #region CellSetActivity Init

        /// <summary>
        /// 单元格活动
        /// </summary>
        private void CellSetActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "CellSetActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,

                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,

                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Cell",
                        Required=false,

                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="CellContent",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetIndex",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="设置",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="单元格",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region ExcelCreateActivity Init

        /// <summary>
        /// Excel活动
        /// </summary>
        private void ExcelCreateActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ExcelCreateActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Body",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="IsExit",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="IsVisible",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="NewDoc",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="PathUrl",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="打开",
                    KeywordType=KeywordTypeEnum.Word,

                },
                new ActionKeyword{
                    Name="保存",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="Excel文档",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region ExtractDataActivity Init

        /// <summary>
        /// 提取结构化数据活动
        /// </summary>
        private void ExtractDataActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ExtractDataActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DataTable",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ExtractMetadata",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="MaxNumber",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="NextSelector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SendMessage",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SimulateClick",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="抓取",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="数据",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region FilterDataTableActivity Init

        /// <summary>
        /// 过滤数据表活动
        /// </summary>
        private void FilterDataTableActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "FilterDataTableActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DataTable",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="FilterRowsMode",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Filters",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="OutDataTable",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectColumns",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="过滤",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="数据表",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region ForEachRowActivity Init

        /// <summary>
        /// 单行操作活动
        /// </summary>
        private void ForEachRowActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ForEachRowActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Body",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="CurrentIndex",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DataTable",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="对于...表中的每一行",
                    KeywordType=KeywordTypeEnum.Foreach,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }
            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region ControlFlow

        #region ForEachActivity Init

        /// <summary>
        /// 循环遍历活动
        /// </summary>
        private void ForEachActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ForEachActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Body",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Values",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="对于...中的每一个",
                    KeywordType=KeywordTypeEnum.Foreach,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region IfActivity Init

        /// <summary>
        /// IF条件活动
        /// </summary>
        private void IfActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "IfActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Condition",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Then",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Else",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="如果...那么...否则",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region WhileActivity Init

        /// <summary>
        /// While循环活动
        /// </summary>
        private void WhileActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "WhileActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Condition",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Body",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="当...时",
                    KeywordType=KeywordTypeEnum.While,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region File

        #region CopyFileActivity Init

        /// <summary>
        /// 复制文件活动
        /// </summary>
        private void CopyFileActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "CopyFileActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Destination",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="NewFileName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Overwrite",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Path",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="复制",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="文件",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region EleExistsActivity Init

        /// <summary>
        /// UI元素存在性活动
        /// </summary>
        private void EleExistsActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "EleExistsActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Element",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="IsExist",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Timeout",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="判断...是否存在",
                    KeywordType=KeywordTypeEnum.IfElse,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="元素",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region FileExistsActivity Init

        /// <summary>
        /// 文件存在性活动
        /// </summary>
        private void PathExistsActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "FileExistsActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Exists",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="PathType",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Path",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="判断...是否存在",
                    KeywordType=KeywordTypeEnum.IfElse,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="文件",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region GetTextActivity Init

        /// <summary>
        /// 获取文本活动
        /// </summary>
        private void GetTextActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "GetTextActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Element",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Value",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Timeout",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="获取",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region MoveFileActivity Init

        /// <summary>
        /// 移动文件活动
        /// </summary>
        private void MoveFileActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "MoveFileActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="NewFileName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Overwrite",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Path",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Destination",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="移动",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="文件",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region ReadTextFileActivity Init

        /// <summary>
        /// 读取文本文件活动
        /// </summary>
        private void ReadTextFileActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ReadTextFileActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Content",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Encoding",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="FileName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="读取",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="文本文件",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region WriteTextFileActivity Init

        /// <summary>
        /// 写入文本文件活动
        /// </summary>
        private void WriteTextFileActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "WriteTextFileActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Text",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Encoding",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="FileName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="将...写入",
                    KeywordType=KeywordTypeEnum.Word,
                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="文本文件",
                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region ReadRangeActivity Init

        /// <summary>
        /// 读取区域内容活动
        /// </summary>
        private void ReadRangeActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ReadRangeActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="CellRange",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DataTable",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetIndex",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="读取",
                    KeywordType=KeywordTypeEnum.Word,

                },
                new ActionKeyword{
                    Name="获取",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="Excel文档",

                },
                new ObjectKeyword{
                    Name="内容",

                },
                new ObjectKeyword{
                    Name="信息",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region WriteRangeActivity Init

        /// <summary>
        /// 写入区域活动
        /// </summary>
        private void WriteRangeActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "WriteRangeActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="CellBegin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DataTable",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetIndex",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SheetName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="写入",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="Excel文档",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region General

        #region ClickActivity Init

        /// <summary>
        /// 单击活动
        /// </summary>
        private void ClickActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "ClickActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ClickType",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Top",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Right",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Bottom",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Left",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Element",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ElementPosition",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="KeyModifiers",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="MouseButton",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="offsetX",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="offsetY",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SendWindowMessage",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SimulateSingleClick",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Timeout",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="UsePoint",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="点击",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region GetDateTimeActivity Init

        /// <summary>
        /// 单击活动
        /// </summary>
        private void GetDateTimeActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "GetDateTimeActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="时间",
                        Required=false,

                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="获取",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="日期",

                },
                new ObjectKeyword{
                    Name="时间",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region HotKeyActivity Init

        /// <summary>
        /// 热键活动
        /// </summary>
        private void HotKeyActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "HotKeyActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="键值",
                        Required=false,

                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="发送",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="按键",

                },
                new ObjectKeyword{
                    Name="快捷键",

                },
                new ObjectKeyword{
                    Name="热键",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region OpenBrowserActivity Init

        /// <summary>
        /// 打开浏览器活动
        /// </summary>
        private void OpenBrowserActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "OpenBrowserActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="浏览器类型",
                        Required=false,

                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="URL",
                        Required=false,

                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="打开",
                    KeywordType=KeywordTypeEnum.Word,

                },
                new ActionKeyword{
                    Name="导航至",
                    KeywordType=KeywordTypeEnum.Word,

                },
                new ActionKeyword{
                    Name="导航打开",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="浏览器",

                },
                new ObjectKeyword{
                    Name="网址",

                },
                new ObjectKeyword{
                    Name="网站",

                },new ObjectKeyword{
                    Name="网页",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region TypeIntoActivity Init

        /// <summary>
        /// 输入文本活动
        /// </summary>
        private void TypeIntoActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "TypeIntoActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="选择器",
                        Required=false,

                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="文本",
                        Required=false,

                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="输入",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region Messaging

        #region SendOutlookMailActivity Init

        /// <summary>
        /// 发送邮件活动
        /// </summary>
        private void SendOutlookMailActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "SendOutlookMailActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="AttachFiles",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Email",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="From",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="IsBodyHtml",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="MailBody",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="MailTopic",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Name",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Password",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Port",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Receivers_Bcc",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Receivers_Cc",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Receivers_To",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Server",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="发送",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="邮件",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region Primitives

        #region AssignActivity Init

        /// <summary>
        /// 赋值活动
        /// </summary>
        private void AssignActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "AssignActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Value",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="To",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="赋值",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region DelayActivity Init

        /// <summary>
        /// 延迟活动
        /// </summary>
        private void DelayActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "DelayActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Duration",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="延迟",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>();

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region WriteLineActivity Init

        /// <summary>
        /// WriteLine活动
        /// </summary>
        private void WriteLineActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "WriteLineActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Text",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="输出",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword> { };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region Process

        #region OpenApplicationActivity Init

        /// <summary>
        /// 打开应用程序活动
        /// </summary>
        private void OpenApplicationActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "OpenApplicationActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Arguments",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Body",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Timeout",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="打开",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="应用程序",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();
            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region CloseApplicationActivity Init

        /// <summary>
        /// 关闭应用程序活动
        /// </summary>
        private void CloseApplicationActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "CloseApplicationActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ProcessName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Selector",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SelectorOrigin",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="SourceImgPath",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="关闭",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="应用程序",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region StartProcessActivity Init

        /// <summary>
        /// 启动进程活动
        /// </summary>
        private void StartProcessActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "StartProcessActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Arguments",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="FileName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="WorkingDirectory",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="启动",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="进程",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #region KillProcessActivity Init

        /// <summary>
        /// 杀死进程活动
        /// </summary>
        private void KillProcessActivityInit()
        {
            var activityEntity = new Activity
            {
                Name = "KillProcessActivity"
            };
            var activityPropertyEntities = new List<ActivityProperty> {
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="DisplayName",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ContinueOnError",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="Processes",
                        Required=false,
                    },
                    new ActivityProperty{
                        ActivityId=activityEntity.Id,
                        Name ="ProcessName",
                        Required=false,
                    }
            };

            var actionKeywordEntities = new List<ActionKeyword>
            {
                new ActionKeyword{
                    Name="杀死",
                    KeywordType=KeywordTypeEnum.Word,

                }
            };

            var objectKeywordEntities = new List<ObjectKeyword>
            {
                new ObjectKeyword{
                    Name="进程",

                }
            };

            var actionKeywordActivityMappingEntities = new List<ActionKeywordActivityMapping>();
            var objectKeywordActivityMappingEntities = new List<ObjectKeywordActivityMapping>();

            foreach (var item in actionKeywordEntities)
            {
                if (_actionKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _actionKeywordEntities.First(a => a.Name == item.Name).Id; } else { _actionKeywordEntities.Add(item); }
                actionKeywordActivityMappingEntities.Add(new ActionKeywordActivityMapping { ActionKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            foreach (var item in objectKeywordEntities)
            {
                if (_objectKeywordEntities.Any(a => a.Name == item.Name)) { item.Id = _objectKeywordEntities.First(a => a.Name == item.Name).Id; } else { _objectKeywordEntities.Add(item); }
                objectKeywordActivityMappingEntities.Add(new ObjectKeywordActivityMapping { ObjectKeywordId = item.Id, ActivityId = activityEntity.Id });
            }

            _client.Insertable(activityEntity).ExecuteCommand();
            _client.Insertable(activityPropertyEntities.ToArray()).ExecuteCommand();


            if (actionKeywordActivityMappingEntities.Count > 0) { _client.Insertable(actionKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
            if (objectKeywordActivityMappingEntities.Count > 0) { _client.Insertable(objectKeywordActivityMappingEntities.ToArray()).ExecuteCommand(); }
        }

        #endregion

        #endregion

        #region Action Keyword Init

        public void ActionKeywordInit()
        {
            if (_actionKeywordEntities.Count > 0)
            {
                _client.Insertable(_actionKeywordEntities.ToArray()).ExecuteCommand();
            }
        }

        #endregion

        #region Action Keyword Init

        public void ObjectKeywordInit()
        {
            if (_objectKeywordEntities.Count > 0)
            {
                _client.Insertable(_objectKeywordEntities.ToArray()).ExecuteCommand();
            }
        }

        #endregion

    }
}
