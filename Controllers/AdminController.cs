using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using MVCConAzure.cs;
using System;
using System.Drawing;

namespace MVCConAzure.Controllers
{
    public class AdminController : Controller
    {
        //Aqui agregamores la referencia de la configuracion y la referencia de la conexion a la BDs para listar los datos
        private cosmosdb DB;
        public AdminController(IConfiguration conf)
        {
            DB = new cosmosdb(conf);
        }
        public async Task<ActionResult<List<user>>> Index()
        {
            //Este primer metodo traera la info
            var rows = await DB.get_user_list();
            return View(rows);
        }
        public IActionResult NewUserForm()
        {
            return View();
        }
        public IActionResult SaveNewUser(user u)
        {
            String action = "Index";
            ViewBag.Message = "";
            if (u != null)
            {
                var status = DB.add_item(u);
                if (status)
                    action = "Index";
                else
                {
                    ViewBag.Message = "Something was wrong";
                    action = "Create";
                }
            }
            else
            {
                ViewBag.Message = "Something was wrong";
                action = "Create";
            }
            return RedirectToAction(action);
        }
        //Ahora vamos a hacer el Edit
        public async Task<ActionResult<user>> Edit(String id)//Vamos a crear la vista con el wizard
        {
            var usr = await DB.read_item(id);
            return View(usr);
        }
        //Este metodo es para el guardado de los datos
        public async Task<ActionResult> Update(user u)
        {
            ViewBag.Message = "";
            //Si el valor de u es nulo, regreso al formulario de Edit sino, procedo al guardado de la info
            if (u != null)
            {
                //Si el guardado es exitoso, regreso al listado, sino, regreso al formulario de Edit
                bool status = await DB.update_item(u);
                if (status)
                    return RedirectToAction("Index");
                else
                {
                    ViewBag.Message = "Something was wrong";
                    return RedirectToAction("Edit", "Admin", u.id);
                }
            }
            else
            {
                ViewBag.Message = "Something was wrong";
                return RedirectToAction("Edit", "Admin", u.id);
            }
        }
        //Vamos a realizar el borrado de un registro, esta parte no tiene vista, solo es el borrado y actualizar la lista
        public async Task<ActionResult> Delete(String id)
        {
            user? u = new user { id = id };
            ViewBag.Message = "";
            //Aqui verificamos q el registro id no venga vacio, si trae info, entonces procedemos al borrado
            if (u != null)
            {
                bool status = await DB.delete_item(u);
                if (status)
                    return RedirectToAction("Index");
                else
                {
                    ViewBag.Message = "Something was wrong";
                    return RedirectToAction("Edit", "Admin", u.id);
                }
            }
            else
            {
                ViewBag.Message = "Something was wrong";
                return RedirectToAction("Edit", "Admin", u.id);
            }
        }
        //  crearemos la vista de details
        public async Task<ActionResult<user>> Detail(String id)
        {
            var usr = await DB.read_item(id);
            return View(usr);
        }
    }
}
