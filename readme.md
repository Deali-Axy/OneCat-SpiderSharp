# CatSpider# 开发笔记

（PS：我这里用了#号代替了Sharp这个单词）

`CatSpider`是毕设里的数据采集模块，本来爬虫类的应用肯定使用python来开发嘛，不过用`request_html`做解析的时候，python的动态类型真的把我恶心到了，而且感觉这个库也不是很成熟，`html5lib`也不好用，也没心思去深入了，之前看到有大佬用`.net core`平台做爬虫，于是我也来试试，没想到效果贼好，特别是配合LinqPad，写个代码段然后直接Dump做数据展示超级方便。

代码测试没问题之后直接写到项目里面，用了轻量级的ORM写入数据库，美滋滋。不过有些网站的采集比较麻烦，有反爬机制，这方面就不如python了，因为python的轮子很多，我直接找别人做的整合一下就好了，毕竟爬虫不是本项目的主要内容，不能浪费太多时间和精力。那么怎么把C#和python的模块整合在一起呢，emmm当然是RPC了，不过在python爬虫里面加个`Flask`来调用也行，不过数据交换性能就要打很大折扣了。

有点偏题了，继续记录`.net core`爬虫~

## 网络请求

如果是python的话，那么我觉得`requests`库是唯一最佳选择，在NetCore里面，现在有个很好用的库`HttpClient`，和`requests`不同，这个是官方的，做得非常好用，调用是全部异步的。

官方推荐使用单例模式，所以我做了个`HttpHelper`静态类来使用~

```c#
public static class HttpHelper
{
    private static readonly HttpClientHandler handler;
    private static readonly HttpClient client;

    public static HttpClientHandler Handler { get => handler; }
    public static HttpClient Client { get => client; }

    static HttpHelper()
    {
        handler = new HttpClientHandler();
        client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
    }
}
```

使用起来很简单：

```c#
var data = await HttpHelper.Client.GetStringAsync("http://example.com");
```

## 解析html文档

解析同样很方便，通过`AngleSharp`库，可以像CSS选择器那样快速定位网页元素，比python常用的`BeautifulSoup`好用很多。

为了使用方便，我封装了一个方法快速解析文档：

```c#
public static async Task<IHtmlDocument> GetHtmlDocument(string url)
{
    var html = await client.GetStringAsync(url);
    return new HtmlParser().ParseDocument(html);
}

public static async Task<IHtmlDocument> GetHtmlDocument(string url, string charset)
{
    var res = await client.GetAsync(url);
    var resBytes = await res.Content.ReadAsByteArrayAsync();
    var resStr = Encoding.GetEncoding(charset).GetString(resBytes);
    return new HtmlParser().ParseDocument(resStr);
}
```

第二个方法带有一个`charset`参数，可以指定文档的编码，有些国内老的网站不是用utf-8，也不标明什么编码，这样下载下来中文都是乱码的，得处理一下。

## 开始采集数据

这里放一些简单的例子，爬取列表：

*用了linq感觉世界真美好，哈哈哈*

```c#
public static async Task<List<ListArticle>> CrawlHotList()
{
    var dom = await HttpHelper.GetHtmlDocument("https://www.com/");
    var links = dom.QuerySelectorAll(".hot-list ul li a");
    var hotList = links.Select((elem, result) => 
    new ListArticle{
		Title = elem.TextContent,
        Link = elem.GetAttribute("href"),
        Source = "来源"});
    return new List<ListArticle>(hotList);
}
```

采集文章内容，代码特别简单：

```c#
public static async Task<Article> CrawlArticle(string url)
{
    var dom = await HttpHelper.GetHtmlDocument(url);
    var data = new Article
    {
        Title = dom.QuerySelector("#cb_post_title_url").TextContent,
        Source = "来源",
        Content = dom.QuerySelector(".postBody").TextContent,
        Link = url,
        PublishTime = DateTime.Parse(dom.QuerySelector("#post-date").TextContent),
        AddTime = DateTime.Now,
        Author = dom.QuerySelector(".postDesc a").TextContent
    };
    return data;
}
```

还有遇到大量数据的时候怎么办呀，这时候就要上并行任务了，C#对比python高性能的优势就体现出来了，上代码：

```c#
public static async Task<List<CnBlogListArticle>> CrawlList2(int page = 10)
{
    var http = HttpHelper.Client;
    var parser = new HtmlParser();
    var data = await Task.WhenAny(
        Enumerable.Range(1, page)
        .Select(async page =>
        {
        	string pageData = await http.GetStringAsync($"https://www.cnblogs.com/sitehome/p/{page}");
            IHtmlDocument doc = await parser.ParseDocumentAsync(pageData);
            return doc.QuerySelectorAll(".post_item").Select(tag => new CnBlogListArticle
            {
                Title = tag.QuerySelector(".titlelnk").TextContent,                     Page = page,
                UserName = tag.QuerySelector(".post_item_foot .lightblue").TextContent,
                PublishTime = DateTime.Parse(Regex.Match(tag.QuerySelector(".post_item_foot").ChildNodes[2].TextContent, @"(\d{4}\-\d{2}\-\d{2}\s\d{2}:\d{2})", RegexOptions.None).Value),
                CommentCount = int.Parse(tag.QuerySelector(".post_item_foot .article_comment").TextContent.Trim()[3..^1]),
                ViewCount = int.Parse(tag.QuerySelector(".post_item_foot .article_view").TextContent[3..^1]),
                BriefContent = tag.QuerySelector(".post_item_summary").TextContent.Trim(),
            });
})).ConfigureAwait(true);
    return new List<CnBlogListArticle>(await data);
}
```

还可以利用`IEnumerable`的`AsParallel()`方法将LINQ并行化。不展开了。

### 其他参考

-  [手把手教你用.NET Core写爬虫 - 知乎 ](https://zhuanlan.zhihu.com/p/24151412) 
-  [1. 第一个简单的爬虫 · dotnetcore/DotnetSpider Wiki](https://github.com/dotnetcore/DotnetSpider/wiki/1.-%E7%AC%AC%E4%B8%80%E4%B8%AA%E7%AE%80%E5%8D%95%E7%9A%84%E7%88%AC%E8%99%AB)
-   [DotNet应用之爬虫入门系列（一）：从.Net走进爬虫世界 - 知乎 ](https://zhuanlan.zhihu.com/p/77599246) 
-   [DotNet应用之爬虫入门系列（二）：HttpClient的前世今生 - 知乎 ](https://zhuanlan.zhihu.com/p/78688112) 
-   [DotNet应用之爬虫入门系列（三）：常见文本结构的处理 - 知乎 ](https://zhuanlan.zhihu.com/p/81779032) 

## 数据持久化

数据持久化这能搞，`.net core`平台有很多好用的ORM，比如微软官方的`EF Core`，比如`SqlSugar`，比如`Dapper`这些，不过EF Core感觉比较重，而且我做这个的时候，还没学怎么单独使用。

关于ORM选择：

- https://www.cnblogs.com/kuangliwen/p/10210638.html
- https://www.cnblogs.com/VAllen/p/Object-Relational-Mapping-Open-Source-Projects-On-Github.html

然后我找了个国人做的轻量级ORM，`Chloe`，看文档使用很简单，于是就试试，文档： http://www.52chloe.com/Wiki/Document/3325155467776229376

模型代码：

```c#
[Table("ListArticles")]
public class ListArticle
{
    [Column("Id", IsPrimaryKey = true)]
    [AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Source { get; set; }
    public string Link { get; set; }
}
```

这个orm需要在模型类上加上属性，定义主键和表名什么的。EF Core这种就不用，完全按照约定来的，这点不如EF Core方便。

而且它不能自动生成表，我只好手动创建表，差评。

接下来常规操作，创建`DBContext`，大部分ORM都差不多：

```c#
public class SQLiteConnectionFactory : IDbConnectionFactory
{
    /// <summary>
    /// 数据库连接字符串，如下
    /// Data Source=dapperTest.db
    /// </summary>
    string _connString = null;
    public SQLiteConnectionFactory(string connString)
    {
        this._connString = connString;
    }
    public IDbConnection CreateConnection()
    {
        // 得先安装Sqlite的驱动
        // Microsoft.Data.Sqlite
        // System.Data.Sqlite
        SQLiteConnection conn = new SQLiteConnection(_connString);
        return conn;
    }
}
```

对了，要先配置连接：

```c#
public static class SQLiteContextFactory
{
    public static SQLiteContext GetContext()
    {
        string connString = "Data Source=CatSpider.db";
        return new SQLiteContext(new SQLiteConnectionFactory(connString));
    }
}
```

使用很简单：

```c#
var context = SQLiteContextFactory.GetContext();
obj = context.Insert(obj);
```

更多操作看文档去，本文就不展开了

## 提供HTTP接口

基本功能实现了，之前考虑到和其他语言或者模块的互操作，觉得可以用HTTP接口来交互，（~~虽然现在觉得不是最佳方案~~）

这个很简单，只要找一个轻量级的服务器框架就行了，我找到一个叫`Nancy`的，听起来像人名，结果居然是Web框架。

参考：

-  [(6条消息)C#最全最详细Nancy框架学习（常见报错，控制台应用，添加到现有ASP.Net MVC站点，ajax）_qq_37791451的博客-CSDN博客 ](https://blog.csdn.net/qq_37791451/article/details/82688684) 
-  [ASP.NET Core开发-使用Nancy框架 - LineZero - 博客园 ](https://www.cnblogs.com/linezero/p/5672772.html) 

使用很简单，直接启动：

```c#
private string host = "http://localhost";
private int port = 50010;
private NancyHost nancy;

public Program()
{
    var uri = new Uri($"{host}:{port}/");
    nancy = new NancyHost(uri);
}
public void Start()
{
    nancy.Start();
    logger.Debug($"nancy server started at {host}:{port}");
    Console.ReadKey();
    nancy.Stop();
}
static void Main(string[] args)
{
    new Program().Start();
}
```

### 定义接口

这个框架有个`Module`的概念，就和Controller差不多吧，定义很简单，我放测试代码上来，业务代码暂时不放出来：

```c#
public class MainModule : NancyModule
{
    public MainModule()
    {
        Get("/", _ => "hello");
        Get("404", _ => HttpStatusCode.NotFound);
        Get("test", _ =>
            {
                var response = (Response)JsonConvert.SerializeObject(new int[] { 1, 2, 3 });
                response.ContentType = "application/json";
                return response;
            });
        Get("test2", _ => JsonConvert.SerializeObject(new int[] { 1, 2, 3 }));
    }
}
```

## 日志记录

最后说一下日志，我这里用了nlog这个轻量级日志引擎。

首先要配置，`NLog.config`，设置生成时自动复制到目标文件夹：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      throwConfigExceptions="true">
  <targets>
    <!-- 配置说明：https://github.com/NLog/NLog/wiki/ColoredConsole-target -->
    <target name="f1" xsi:type="File" fileName="CatSpiderLog.txt"/>
    <target name="n1" xsi:type="Network" address="tcp://localhost:4001"/>
    <target name="c1" xsi:type="Console" encoding="utf-8"
            error="true"
            detectConsoleAvailable="true" />
    <target name="c2" xsi:type="ColoredConsole" encoding="utf-8"
          useDefaultRowHighlightingRules="true"
          errorStream="true"
          enableAnsiOutput="true"
          detectConsoleAvailable="true"
          DetectOutputRedirected="true">
    </target>
  </targets>

  <rules>
    <logger name="*" maxLevel="Debug" writeTo="c2" />
    <logger name="*" maxLevel="Debug" writeTo="f1" />
    <!--<logger name="*" minLevel="Info" writeTo="f1" />-->
  </rules>
</nlog>
```

官方推荐每个类用一个logger实例：

```c#
private static Logger logger = LogManager.GetCurrentClassLogger();
```

使用：

```c#
logger.Debug($"列表：{obj}");
```

很方便。

大概就这，有空继续写其他的~

## 单元测试

这部分可能需要单独拿出来记录。

参考： [单元测试 - 使用 MSTest 进行 C# 单元测试 - 《.NET Core 指南》 - 书栈网 · BookStack ](https://www.bookstack.cn/read/dotnet/e3580fde327bb465.md) 

这里我用的是MSTest，还有其他的测试框架，不过用起来差不多吧，这里我用MSTest和VS整合的比较好。

编写测试类：

```c#
[TestClass]
public class CnBlogTest
{
    [TestMethod]
    public void TestCrawlList()
    {
        var data = CnBlog.CrawlList(2);
        Assert.IsTrue(data.Count > 0);
    }

    [TestMethod]
    public void TestCrawlList2()
    {
        var data = CnBlog.CrawlList2(2);
        while (!data.IsCompleted) { }
        Assert.IsTrue(data.Result.Count > 0);
    }

	[DataTestMethod]
	[DataRow("https://www.cnblogs.com/dudu/p/11562019.html")]
	[DataRow("https://www.cnblogs.com/JcrLive/p/12235715.html#4490478")]
	[DataRow("https://www.cnblogs.com/leipDao/p/10058144.html#4131620")]
    public void TestCrawlArticle(string url)
    {
        var data = CnBlog.CrawlArticle(url);
        while (!data.IsCompleted) { }
        Assert.IsNotNull(data.Result);
    }
}
```

