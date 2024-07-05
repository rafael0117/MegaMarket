using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.Mvc;
using CapaEntidad;
using CapaNegocio;
using Newtonsoft.Json;

namespace CapaPresentacionAdmin.Controllers
{
    public class MantenedorController : Controller
    {
        /**++++CATEGORIA++++++*/
        #region CATEGORIA
        public ActionResult Categoria()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ListarCategoria()
        {
            List<Categoria> olista = new List<Categoria>();
            olista = new CN_Categoria().Listar();
            return Json(new { data = olista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarCategoria(Categoria objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.IdCategoria == 0)
            {
                resultado = new CN_Categoria().Registrar(objeto, out mensaje);
            }
            else
            {
                resultado = new CN_Categoria().Editar(objeto, out mensaje);
            }
            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult EliminarCategoria(int id)
        {

            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Categoria().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        #endregion
        /**++++MARCA++++++*/
        #region MARCA
        public ActionResult Marca()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ListarMarca()
        {
            List<Marca> olista = new List<Marca>();
            olista = new CN_Marca().Listar();
            return Json(new { data = olista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarMarca(Marca objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.IdMarca == 0)
            {
                resultado = new CN_Marca().Registrar(objeto, out mensaje);
            }
            else
            {
                resultado = new CN_Marca().Editar(objeto, out mensaje);
            }
            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult EliminarMarca(int id)
        {

            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Marca().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        #endregion
        /**++++PRODUCTO++++++*/
        #region PRODUCTO
        public ActionResult Producto()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ListarProducto()
        {
            List<Producto> olista = new List<Producto>();
            olista = new CN_Producto().Listar();
            return Json(new { data = olista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarProducto(string objeto,HttpPostedFileBase archivoImagen)
        {
         
            string mensaje = string.Empty;
            bool operacion_exitosa = true;
            bool guardar_imagen_exito = true;

            Producto oProducto= new Producto();
            oProducto=JsonConvert.DeserializeObject<Producto>(objeto);


            decimal precio;
            if (decimal.TryParse(oProducto.PrecioTexto, NumberStyles.AllowDecimalPoint, new CultureInfo("es-PE"), out precio))
            {
                oProducto.Precio = precio;
            }
            else 
            {
                return Json(new { operacion_Exitosa = operacion_exitosa, idGenerado = oProducto, mensaje= "El formato del precio debe ser ##.##" }, JsonRequestBehavior.AllowGet);

            }
            if (oProducto.IdProducto == 0)
            {
                int idProductoGenerado = new CN_Producto().Registrar(oProducto, out mensaje);
                if (idProductoGenerado != 0)
                {
                    oProducto.IdProducto = idProductoGenerado;
                }
                else {
                operacion_exitosa = false;
                }
            }
            else
            {
                operacion_exitosa = new CN_Producto().Editar(oProducto, out mensaje);
            }



            if (operacion_exitosa) {
                if (archivoImagen != null) {
                    string ruta_guardar = ConfigurationManager.AppSettings["ServidorFotos"];
                    string extension =Path.GetExtension(archivoImagen.FileName);
                    string nombre_imagen = string.Concat(oProducto.IdProducto.ToString(), extension);
                    try { 
                    archivoImagen.SaveAs(Path.Combine(ruta_guardar,nombre_imagen));
                    }
                    catch (Exception ex){
                    string msg=ex.Message;
                        guardar_imagen_exito = false;
                    }
                    if (guardar_imagen_exito)
                    {
                        oProducto.RutaImagen = ruta_guardar;
                        oProducto.NombreImagen = nombre_imagen;
                        bool rspta = new CN_Producto().GuardarDatosImagen(oProducto, out mensaje);

                    }
                    else {
                        mensaje = "Se guardaron el producto pero hubo problemas con la imagen";
                    }
                }

            }
                return Json(new {operacion_Exitosa=operacion_exitosa,idGenerado=oProducto,mensaje=mensaje= "El formato del precio debe ser ##.##" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult ImagenProducto(int id) {
            bool conversion;
            Producto oproducto=new CN_Producto().Listar().Where(p=>p.IdProducto==id).FirstOrDefault();

            string textBase64 = CN_Recursos.ConvertirBase64(Path.Combine(oproducto.RutaImagen, oproducto.NombreImagen), out conversion);
            return Json(new
            {
                conversion =conversion,
                textobase64=textBase64,
                extension=Path.GetExtension(oproducto.NombreImagen)

            },
            JsonRequestBehavior.AllowGet
            );
      
        }
        public JsonResult EliminarProducto(int id)
        {

            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Producto().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }
}