## MyLab.ApiClient - Контракт сервиса

Чтобы начать описание сервиса, объявите его контракт в виде интерфейса.

Используйте `ApiAttribute` чтобы отметить интерфейс-контракт сервиса:

```C#
[Api]
public interface IService
{
    //...
}
```

В этом атрибуте можно указать базовый путь к сервису, который будет использоваться как базовый для формирования полного адреса запроса с учётом относительных путей конечных точек (методов):

```C#
[Api("orders/v1")]
public interface IService
{
    //...
}
```
