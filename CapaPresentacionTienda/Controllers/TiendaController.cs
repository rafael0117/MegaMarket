using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacionTienda.Controllers
{
    public class TiendaController : Controller
    {
        // GET: Tienda
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ListaCategorias() { 
        List<Categoria> lista = new List<Categoria>();
            lista = new CN_Categoria().Listar();
            return Json(new{data = lista},JsonRequestBehavior.AllowGet);
        } 
        [HttpPost]
        public JsonResult ListarMarcaxCategoria(int idcategoria) { 
        List<Marca> lista = new List<Marca>();
            lista = new CN_Marca().ListarMarcaxCategoria(idcategoria);
            return Json(new{data = lista},JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ListarProducto(int idcategoria,int idmarca) 
        { 
        List<Producto> lista = new List<Producto>();
            bool conversion;
            lista = new CN_Producto().Listar().Select(p => new Producto()
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                oMarca = p.oMarca,
                oCategoria = p.oCategoria,
                Precio = p.Precio,
                Stock = p.Stock,
                RutaImagen = p.RutaImagen,
                Base64 = CN_Recursos.ConvertirBase64(Path.Combine(p.RutaImagen,p.NombreImagen),out conversion),
                Extension = Path.GetExtension(p.Extension),
                Activo = p.Activo
            }).Where(p=>
            p.oCategoria.IdCategoria==(idcategoria==0? p.oCategoria.IdCategoria:idcategoria)&&
            p.oMarca.IdMarca == (idmarca==0?p.oMarca.IdMarca:idmarca)&&
            p.Stock > 0 && p.Activo == true
            ).ToList();
            var jsonresult = Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            jsonresult.MaxJsonLength = int.MaxValue;
            return jsonresult;
        }

    }
}