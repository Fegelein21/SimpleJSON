
# 简介

原仓库地址：https://github.com/Bunny83/SimpleJSON  
本仓库中的代码进行了如下改动：

对 `SimpleJSON.cs` 中的代码进行了翻新和改善，C#语言版本为7.3。

JSON文本中的字符串值允许在引号范围内直接换行。示例如下：  
```json
{
	// "NewlineText": "Line 1\r\nLine 2\r\nLine 3"
	"NewlineText": "Line 1
Line 2
Line 3",
	"Text": "Text"
}
```

---

新增 `JSONNode.AllowDuplicateKeys` 配置项。当 `JSONNode.AllowDuplicateKeys = true` 时，`JSONObject` 将会保留相同键的键值对，用户可通过遍历的方式来获取所有内容。  
该配置项的默认值为 `false`。

示例：  
```json
{
	"Config": "配置",
	"Config": {
		"AllowDuplicateKeys": "允许重复键",
		"IndentChar": "缩进字符"
	}
}
```

```c#
JSONNode.AllowDuplicateKeys = true;
var root = JSONNode.Parse(text);
foreach (var item in root)
	Console.WriteLine($"{item.Key} --- JSONNodeType.{item.Value.Tag}");
// Config --- JSONNodeType.String
// Config --- JSONNodeType.Object
```

---

新增 `JSONNode.IndentChar` 配置项。用于控制 `JSONNode` 输出JSON文本时所使用的缩进字符。  
该配置项的默认值为半宽空格 `' '`。

示例：  
```c#
var root = JSONNode.Parse("{\"Config\":{\"AllowDuplicateKeys\":\"允许重复键\",\"IndentChar\":\"缩进字符\"}}");
JSONNode.IndentChar = '\t';
Console.WriteLine(jsonRoot.ToString(1));
```

```json
{
	"Config": {
		"AllowDuplicateKeys": "允许重复键",
		"IndentChar": "缩进字符"
	}
}
```

# 快速使用

```C#
// 通过代码来构建JSON节点
JSONNode root = new JSONObject();

// 添加基础类型键值对
root.Add("WindowMode", "FullScreen");
root.Add("Version", 10);
root.Add("Volume", 0.5);
root.Add("Mute", false);
root.Add("ApiKey", null);

// 添加列表
JSONNode modList = new JSONArray();
modList.Add(null, "mod1");
modList.Add(null, "mod2");
modList.Add(null, "mod3");
root.Add("ModList", modList);

// 添加子节点
JSONNode uiText = new JSONObject();
uiText.Add("StartGame", "开始游戏");
uiText.Add("LoadSave", "加载存档");
uiText.Add("ExitGame", "退出游戏");
root.Add("UIText", uiText);

// JSON节点输出带换行和空格缩进的json文本
string jsonText = root.ToString(2);

// 将json文本保存到文件中
File.WriteAllText("Test.json", jsonText);

// 从文件中读取json文本
jsonText = File.ReadAllText("Test.json");

// 将json文本解析为JSON节点
root = JSONNode.Parse(jsonText);
```

可用配置项：  
| 配置名                      | 功能 | 默认值 |
| :---:                       | ---  | ---   |
| JSONNode.forceASCII         | 输出JSON文本时，将超出ASCII字符范围的字符转换为`\uxxxx`格式的字符串 | false |
| JSONNode.longAsString       | 向JSON节点添加64位整数值时将该值转换为字符串 | false |
| JSONNode.allowLineComments  | 解析JSON文本时，自动跳过以"//"开头的注释 | true |
| JSONNode.AllowDuplicateKeys | 解析JSON文本时，`JSONObject`保留相同键的键值对 | false |
| JSONNode.IndentChar         | 该变量将作为输出JSON文本时所使用的缩进字符 | 半宽空格 `' '` |
