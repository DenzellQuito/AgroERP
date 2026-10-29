using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers
{
    [Authorize] // Bloquea el acceso a cualquiera que no haya iniciado sesión
    public class LogisticaController : Controller
    {
        // Esta acción intercepta la URL: /Logistica/Dashboard
        public IActionResult Dashboard()
        {
            // Busca automáticamente el archivo Views/Logistica/Dashboard.cshtml
            return View();
        }
    }
}