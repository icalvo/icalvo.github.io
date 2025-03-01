WORKS! In my brand-new job at Electronic Arts we are fortunate enough to work with Microsoft's latest technologies. We are developing web applications and, as every good developer out there I like to test as many things as it is possible and practical.

In the last sprint I found myself with the problem of testing the routing configuration of a Web API REST service and also the routing configuration of some MVC controllers.

A DuckDuckGo search returned a great [article](https://www.strathweb.com/2012/08/testing-routes-in-asp-net-web-api/) at [StrathWeb](https://www.strathweb.com) that solved the Web API side.

Searching then for the MVC side, I found that, unsurprisingly, the famous library [MvcContrib](https://mvccontrib.codeplex.com/) has already a solution, but using a very different, fluent approach.

I already had my Web API tests written with Philip's class (Philip is the author of StrathWeb), so I thought I would create a similar class that solved the problem, borrowing the necessary code from MvcContrib test helpers.

The two classes were VERY similar, so I ended refactoring the common code in an abstract base class with an interface. Following is the code of the interface, the three classes and two sample tests that show how to build and use each one.

```csharp
///
<summary>
    /// Route tester can test a route defined by URL, HTTP method and a lambda expression of a call to an action of a controller.
    /// </summary>
public interface IRouteTester
{
///
<summary>
    /// Tests that an URL with an HTTP method is executed by an action of a controller.
    /// </summary>
/// <typeparam name="TController">Controller type</typeparam>
/// <typeparam name="TActionReturn">Return type of the action</typeparam>
/// <param name="url">The URL to be tested</param>
/// <param name="method">The HTTP method</param>
/// <param name="expression">The lambda expression that defines an action of a controller</param>
void TestRoute
<TController, TActionReturn>(
string url,
HttpMethod method,
Expression<Func
<TController, TActionReturn>> expression);

/// <summary>
/// Tests that an URL with an HTTP method is executed by an action of a controller.
/// </summary>
/// <typeparam name="TController">Controller type</typeparam>
/// <param name="url">The URL to be tested</param>
/// <param name="method">The HTTP method</param>
/// <param name="expression">The lambda expression that defines an action of a controller</param>
void TestRoute
<TController>(
string url,
HttpMethod method,
Expression<Action
<TController>> expression);
}
```

```csharp
public abstract class BaseRouteTester : IRouteTester
{
///
<summary>
    /// Tests that an URL with an HTTP method is executed by an action of a controller.
    /// </summary>
/// <typeparam name="TController">Controller type</typeparam>
/// <typeparam name="TActionReturn">Return type of the action</typeparam>
/// <param name="url">The URL to be tested</param>
/// <param name="method">The HTTP method</param>
/// <param name="expression">The lambda expression that defines an action of a controller</param>
public void TestRoute
<TController, TActionReturn>(string url, HttpMethod method, Expression<Func
<TController, TActionReturn>> expression)
{
this.TestRoute
<TController>(url, method, GetMethodCall(expression));
}

/// <summary>
/// Tests that an URL with an HTTP method is executed by an action of a controller.
/// </summary>
/// <typeparam name="TController">Controller type</typeparam>
/// <param name="url">The URL to be tested</param>
/// <param name="method">The HTTP method</param>
/// <param name="expression">The lambda expression that defines an action of a controller</param>
public void TestRoute
<TController>(string url, HttpMethod method, Expression<Action
<TController>> expression)
{
this.TestRoute
<TController>(url, method, GetMethodCall(expression));
}

/// <summary>
/// Set ups route data and tests that the URL matches with some route.
/// </summary>
/// <param name="url">The URL</param>
/// <param name="method">The HTTP method</param>
protected abstract void SetupAndTestRouteData(string url, HttpMethod method);

/// <summary>
/// Get a route value
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected abstract object GetValue(string key);

/// <summary>
/// Is a route value defined?
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected abstract bool HasValue(string key);

/// <summary>
/// Gets the name of the controller corresponding to the matched route
/// </summary>
/// <returns></returns>
protected abstract string GetControllerName();

/// <summary>
/// Gets the name of the action corresponding to the matched route
/// </summary>
/// <returns></returns>
protected abstract string GetActionName();

/// <summary>
/// Gets the <see cref="MethodCallExpression"/> for a expression corresponding to an <see cref="Action{T}"/> (void method)
/// </summary>
/// <typeparam name="T">Type of the parameters</typeparam>
/// <param name="expression">The expression</param>
/// <returns></returns>
private static MethodCallExpression GetMethodCall
<T>(Expression<Action
<T>> expression)
{
return expression.Body as MethodCallExpression;
}

/// <summary>
/// Gets the <see cref="MethodCallExpression"/> for a expression corresponding to an <see cref="Func{T,U}"/> (method returning a T)
/// </summary>
/// <typeparam name="T">Type of the parameters</typeparam>
/// <typeparam name="U">Return type of the call</typeparam>
/// <param name="expression">The expression</param>
/// <returns></returns>
private static MethodCallExpression GetMethodCall
<T, U>(Expression<Func
<T, U>> expression)
{
return expression.Body as MethodCallExpression;
}

/// <summary>
/// Get the method name of a
/// </summary>
/// <param name="method"></param>
/// <returns></returns>
private string GetMethodName(MethodCallExpression method)
{
if (method != null)
{
return method.Method.Name;
}

throw new ArgumentException("Expression is wrong");
}

/// <summary>
/// Tests that an URL with an HTTP method is executed by an action of a controller.
/// </summary>
/// <typeparam name="TController">Controller type</typeparam>
/// <param name="url">The URL to be tested</param>
/// <param name="method">The HTTP method</param>
/// <param name="methodCall">Method call expression</param>
private void TestRoute
<TController>(string url, HttpMethod method, MethodCallExpression methodCall)
{
// check route
this.SetupAndTestRouteData(url, method);

// check controller
string expectedController = typeof(TController).Name;
string actualController = this.GetControllerName();

if (expectedController != actualController)
{
throw new ControllerNotFoundException(expectedController, actualController);
}

// check action
string actualAction = this.GetActionName();
string expectedAction = this.GetMethodName(methodCall);

if (expectedAction != actualAction)
{
throw new ActionNotFoundException(expectedAction, actualAction);
}

// check parameters
for (int i = 0; i < methodCall.Arguments.Count; i++)
{
ParameterInfo param = methodCall.Method.GetParameters()[i];
bool isReferenceType = !param.ParameterType.IsValueType;
bool isNullable = isReferenceType ||
(param.ParameterType.UnderlyingSystemType.IsGenericType && param.ParameterType.UnderlyingSystemType.GetGenericTypeDefinition() == typeof(Nullable<>));

string controllerParameterName = param.Name;
object actualValue = GetValue(controllerParameterName);
object expectedValue = null;
Expression expressionToEvaluate = methodCall.Arguments[i];

// If the parameter is nullable and the expression is a Convert UnaryExpression,
// we actually want to test against the value of the expression's operand.
if (expressionToEvaluate.NodeType == ExpressionType.Convert
&& expressionToEvaluate is UnaryExpression)
{
expressionToEvaluate = ((UnaryExpression)expressionToEvaluate).Operand;
}

switch (expressionToEvaluate.NodeType)
{
case ExpressionType.Constant:
expectedValue = ((ConstantExpression)expressionToEvaluate).Value;
break;

case ExpressionType.New:
case ExpressionType.MemberAccess:
expectedValue = Expression.Lambda(expressionToEvaluate).Compile().DynamicInvoke();
break;
}

if (isNullable && (string)actualValue == string.Empty && expectedValue == null)
{
// The parameter is nullable so an expected value of '' is equivalent to null;
continue;
}

// HACK: this is only sufficient while System.Web.Mvc.UrlParameter has only a single value.
if (actualValue == UrlParameter.Optional ||
(actualValue != null && actualValue.ToString().Equals("System.Web.Mvc.UrlParameter")))
{
actualValue = null;
}

if (expectedValue is DateTime)
{
actualValue = Convert.ToDateTime(actualValue);
}
else
{
expectedValue = expectedValue == null ? null : expectedValue.ToString();
}

bool isOptional = methodCall.Method.GetParameters()[i].IsOptional;
if ((actualValue == null) && isOptional)
{
return;
}

if (!Equals(actualValue, expectedValue))
{
throw new ValueMismatchException(controllerParameterName, actualValue, expectedValue);
}
}
}
}
```

```csharp
///
<summary>
    /// Class that allows to test a Web API route.
    /// </summary>
public class RestRouteTester : BaseRouteTester
{
private readonly HttpConfiguration config;
private IHttpControllerSelector controllerSelector;
private HttpControllerContext controllerContext;
private IHttpRouteData routeData;
private HttpRequestMessage request;

public RestRouteTester(HttpConfiguration conf)
{
this.config = conf;
}

/// <summary>
/// Gets the name of the action corresponding to the matched route
/// </summary>
/// <returns></returns>
protected override string GetActionName()
{
if (this.controllerContext.ControllerDescriptor == null)
{
this.GetControllerName();
}

var actionSelector = new ApiControllerActionSelector();
var descriptor = actionSelector.SelectAction(this.controllerContext);

return descriptor.ActionName;
}

/// <summary>
/// Gets the name of the controller corresponding to the matched route
/// </summary>
/// <returns></returns>
protected override string GetControllerName()
{
var descriptor = this.controllerSelector.SelectController(this.request);
this.controllerContext.ControllerDescriptor = descriptor;
return descriptor.ControllerType.Name;
}

/// <summary>
/// Set ups route data and tests that the URL matches with some route.
/// </summary>
/// <param name="url">The URL</param>
/// <param name="method">The HTTP method</param>
protected override void SetupAndTestRouteData(string url, HttpMethod method)
{
this.request = new HttpRequestMessage(method, url);
this.routeData = config.Routes.GetRouteData(this.request);
if (this.routeData == null)
{
throw new RouteNotFoundException(method, url);
}

this.request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

this.controllerSelector = new DefaultHttpControllerSelector(config);
this.controllerContext = new HttpControllerContext(config, routeData, this.request);
}

/// <summary>
/// Get a route value
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected override object GetValue(string key)
{
foreach (var routeValueKey in this.routeData.Values.Keys)
{
if (string.Equals(routeValueKey, key, StringComparison.InvariantCultureIgnoreCase))
{
if (this.routeData.Values[routeValueKey] == null)
{
return null;
}

return this.routeData.Values[routeValueKey].ToString();
}
}

return null;
}

/// <summary>
/// Is a route value defined?
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected override bool HasValue(string key)
{
return this.routeData.Values.ContainsKey(key);
}
}
```

```csharp
public class WebRouteTester : BaseRouteTester
{
private readonly RouteCollection routeCollection;
private RouteData routeData;

public WebRouteTester(RouteCollection routeCollection)
{
this.routeCollection = routeCollection;
}

/// <summary>
/// Set ups route data and tests that the URL matches with some route.
/// </summary>
/// <param name="url">The URL</param>
/// <param name="method">The HTTP method</param>
protected override void SetupAndTestRouteData(string url, HttpMethod method)
{
this.routeData = routeCollection.GetRouteData(new FakeHttpContext(url, method.ToString()));

if (this.routeData == null)
{
throw new RouteNotFoundException(method, url);
}
}

/// <summary>
/// Get a route value
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected override object GetValue(string key)
{
foreach (var routeValueKey in this.routeData.Values.Keys)
{
if (string.Equals(routeValueKey, key, StringComparison.InvariantCultureIgnoreCase))
{
if (this.routeData.Values[routeValueKey] == null)
{
return null;
}

if (this.routeData.Values[routeValueKey].GetType().Name == "UrlParameter")
{
return null;
}

return this.routeData.Values[routeValueKey];
}
}

return null;
}

/// <summary>
/// Is a route value defined?
/// </summary>
/// <param name="key"></param>
/// <returns></returns>
protected override bool HasValue(string key)
{
return routeData.Values.ContainsKey(key);
}

/// <summary>
/// Gets the name of the controller corresponding to the matched route
/// </summary>
/// <returns></returns>
protected override string GetControllerName()
{
return this.GetValue("controller") + "Controller";
}

/// <summary>
/// Gets the name of the action corresponding to the matched route
/// </summary>
/// <returns></returns>
protected override string GetActionName()
{
return this.GetValue("action").ToString();
}
}
```

```csharp
[TestClass]
public class WebRoutingTests
{
private RouteCollection routeCollection;

[TestInitialize]
public void Initialize()
{
this.routeCollection = new RouteCollection();
WebRouteConfig.RegisterRoutes(routeCollection);
}

[TestMethod]
public void PlaylistController_GetTracksOffset()
{
TestRoute("~/playlist/45/GetTracks/56", HttpMethod.Get, (PlaylistController tc) => tc.GetTracks(45, 56, 100));
}

#region Private methods

private void TestRoute
<TController, TActionReturn>(string url, HttpMethod method, Expression<Func
<TController, TActionReturn>> expression)
{
this.routeTester.TestRoute(url, method, expression);
}

private void TestRoute
<TController>(string url, HttpMethod method, Expression<Action
<TController>> expression)
{
this.routeTester.TestRoute(url, method, expression);
}

#endregion
}
```