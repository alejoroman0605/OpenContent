﻿using Satrabel.OpenContent.Components.Datasource.search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.JPList
{
    public static class JplistQueryBuilder
    {
        public static Select MergeJpListQuery(FieldConfig config, Select select, List<StatusDTO> statuses)
        {
            var query = select.Query;
            foreach (StatusDTO status in statuses)
            {
                switch (status.action)
                {
                    case "paging":
                        {
                            int number = 100000;
                            //  string value (it could be number or "all")
                            int.TryParse(status.data.number, out number);
                            select.PageSize = number;
                            select.PageIndex = status.data.currentPage;
                            break;
                        }
                    case "filter":
                        {
                            if (status.type == "textbox" && status.data != null && !string.IsNullOrEmpty(status.name) && !string.IsNullOrEmpty(status.data.value))
                            {
                                var names = status.name.Split(',');
                                if (names.Length == 1)
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        FieldOperator = OperatorEnum.START_WITH,
                                        Value = new StringRuleValue(status.data.value),
                                    });
                                }
                                else
                                {
                                    var group = new FilterGroup() { Condition = ConditionEnum.OR };
                                    foreach (var n in names)
                                    {
                                        group.AddRule(new FilterRule()
                                        {
                                            Field = n,
                                            FieldOperator = OperatorEnum.START_WITH,
                                            Value = new StringRuleValue(status.data.value),
                                        });
                                    }
                                    query.FilterGroups.Add(group);
                                }
                            }
                            else if ((status.type == "checkbox-group-filter" || status.type == "button-filter-group" || status.type == "combined")
                                        && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "pathGroup" && status.data.pathGroup != null && status.data.pathGroup.Count > 0)
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        FieldOperator = OperatorEnum.IN,
                                        MultiValue = status.data.pathGroup.Select(s => new StringRuleValue(s)),
                                    });
                                }
                            }
                            else if (status.type == "filter-select" && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "path" && !string.IsNullOrEmpty(status.data.path))
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        Value = new StringRuleValue(status.data.path),
                                    });
                                }
                            }
                            else if (status.type == "autocomplete" && status.data != null && !string.IsNullOrEmpty(status.data.path) && !string.IsNullOrEmpty(status.data.value))
                            {
                                var names = status.data.path.Split(',');
                                if (names.Length == 1)
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.data.path,
                                        FieldOperator = OperatorEnum.START_WITH,
                                        Value = new StringRuleValue(status.data.value),
                                    });
                                }
                                else
                                {
                                    var group = new FilterGroup() { Condition = ConditionEnum.OR };
                                    foreach (var n in names)
                                    {
                                        group.AddRule(new FilterRule()
                                        {
                                            Field = n,
                                            FieldOperator = OperatorEnum.START_WITH,
                                            Value = new StringRuleValue(status.data.value),
                                        });
                                    }
                                    query.FilterGroups.Add(group);
                                }
                            }
                            break;
                        }

                    case "sort":
                        {
                            select.Sort.Clear();
                            select.Sort.Add(new SortRule()
                            {
                                Field = status.data.path,
                                Descending = status.data.order == "desc",
                                FieldType = Sortfieldtype(config, status.data.path)
                                // todo :
                            });
                            break;
                        }
                }
            }
            return select;
        }


        private static FieldTypeEnum Sortfieldtype(FieldConfig IndexConfig, string fieldName)
        {
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey(fieldName))
            {
                var config = IndexConfig.Items == null ? IndexConfig.Fields[fieldName] : IndexConfig.Items;
                if (config.IndexType == "datetime" || config.IndexType == "date" || config.IndexType == "time")
                {
                    return FieldTypeEnum.DATETIME;
                }
                else if (config.IndexType == "boolean")
                {
                    return FieldTypeEnum.BOOLEAN;
                }
                else if (config.IndexType == "int")
                {
                    return FieldTypeEnum.INTEGER;
                }
                else if (config.IndexType == "long")
                {
                    return FieldTypeEnum.LONG;
                }
                else if (config.IndexType == "float" || config.IndexType == "double")
                {
                    return FieldTypeEnum.FLOAT;
                }
                
                else if (config.IndexType == "key")
                {
                    return FieldTypeEnum.KEY;
                }
                else if (config.IndexType == "text")
                {
                    return FieldTypeEnum.TEXT;
                }
                else if (config.IndexType == "html")
                {
                    return FieldTypeEnum.HTML;
                }
                else
                {
                    return FieldTypeEnum.STRING;
                }
            }
            return FieldTypeEnum.STRING;
        }

    }
}