## Release 1.0.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
Mccj004 | AutoCodeGenerator | Warning | AutoCodeClassModes 缺少或为空的 Id
Mccj005 | AutoCodeGenerator | Warning | 属性 Id 找不到对应 ClassMode
Mccj006 | AutoCodeGenerator | Warning | 生成属性名重复被跳过
Mccj007 | AutoFilesGenerator | Warning | entity.json 属性缺 name/type
Mccj008 | AutoCodeGenerator | Warning | 有 AutoCodeProperty 但无有效 ClassMode
Mccj009 | AutoCodeGenerator | Warning | InheritStr 非法类型名
Mccj010 | AutoCodeGenerator | Warning | Attributes 字符串无法解析为特性
Mccj011 | AutoCodeGenerator | Warning | InterfaceTypes 成员未生成属性
Mccj012 | AutoCodeGenerator | Info | 建议改用 AutoCodeCrudModes（含 CodeFix）
Mccj013 | AutoCodeGenerator | Warning | 索引列未出现在 Entity 投影
Mccj014 | AutoCodeGenerator | Warning | AutoCodeRelation 缺少 TargetName

## Release 1.0.0.17

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
Mccj001 | AutoCodeGenerator | Error | 代码生成异常
Mccj002 | AutoCodeGenerator | Error | 属性修饰符冲突
Mccj003 | AutoFilesGenerator | Error | entity.json 代码生成失败
