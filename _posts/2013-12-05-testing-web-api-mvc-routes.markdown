---
layout: post
title: "Testing Web API and MVC routes altogether"
date: 2013-12-05
comments: true
categories: [code,csharp,testing,webapi,mvc]
---
In my new job at Electronic Arts we are fortunate enough to work with Microsoft's latest technologies. We are developing web applications and, as every good developer out there I like to test as many things as it is possible and practical.

In the last sprint I found myself with the problem of testing the routing configuration of a Web API REST service and also the routing configuration of some MVC controllers.

A DuckDuckGo search returned a great [article](http://www.strathweb.com/2012/08/testing-routes-in-asp-net-web-api/) at [StrathWeb](http://www.strathweb.com) that solved the Web API side.

Searching then for the MVC side, I found that, unsurprisingly, the famous library [MvcContrib](https://mvccontrib.codeplex.com/) has already a solution, but using a very different, fluent approach.

I already had my Web API tests written with Philip's class (Philip is the author of StrathWeb), so I thought I would create a similar class that solved the problem, borrowing the necessary code from MvcContrib test helpers.

The two classes were VERY similar, so I ended refactoring the common code in an abstract base class. Following is the code of the three classes and two sample tests that show how to build and use each one.

```csharp
public class Test {
}
```