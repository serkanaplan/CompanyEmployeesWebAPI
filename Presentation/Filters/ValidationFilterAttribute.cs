//  Filters:
// Authorization filters: Kimlik doğrulama ve yetkilendirme işlemlerini gerçekleştirir.
// Resource filters: Kaynak yönetimi ve önbellekleme için kullanılır.
// Action filters: Eylem metodları çalıştırılmadan önce ve sonra işlemler yapar.
// Exception filters: Hata yönetimi ve işleme için kullanılır.
// Result filters: Eylem sonuçları üretilmeden önce ve sonra çalışır.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.Presentation.Filters;

public class ValidationFilterAttribute : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // our code before action executes 
        var action = context.RouteData.Values["action"];
        var controller = context.RouteData.Values["controller"];
        var param = context.ActionArguments.SingleOrDefault(x => x.Value.ToString().Contains("Dto")).Value;
        if (param is null)
        {
        context.Result = new BadRequestObjectResult($"Object is null. Controller: { controller }, action: { action}"); 
        return;
        }
        if (!context.ModelState.IsValid) context.Result = new UnprocessableEntityObjectResult(context.ModelState);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // our code after action executes 
    }
}

// VEYA ASENKRON OLAN YÖNTEMİ TERCİH EDEBİLİRSİN
// public class AsyncActionFilterExample : IAsyncActionFilter
// {
//     public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
//     {
//         // execute any code before the action executes 
//         var result = await next();
//         // execute any code after the action executes 
//     }
// }
