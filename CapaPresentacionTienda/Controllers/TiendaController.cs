using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CapaEntidad;
using CapaEntidad.Paypal;
using CapaNegocio;
using CapaPresentacionTienda.Filter;

namespace CapaPresentacionTienda.Controllers
{
    public class TiendaController : Controller
    {
        // GET: Tienda
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DetalleProducto(int idproducto = 0) {
            Producto oProducto = new Producto();
            bool conversion;
            oProducto = new CN_Producto().Listar().Where(p => p.IdProducto == idproducto).FirstOrDefault();

            if (oProducto != null) {
                oProducto.Base64 = CN_Recursos.ConvertirBase64(Path.Combine(oProducto.RutaImagen, oProducto.NombreImagen), out conversion);
                oProducto.Extension=Path.GetExtension(oProducto.NombreImagen); 
            }
            return View(oProducto); 
            
        
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


        [HttpPost]
        public JsonResult AgregarCarrito(int idproducto)
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            bool existe = new CN_Carrito().ExisteCarrito(idcliente, idproducto);
            bool respuesta = false;
            string mensaje = string.Empty;
            if (existe)
            {
                mensaje = "El producto ya existe en el carrito";

            }
            else {
                respuesta = new CN_Carrito().OperacionCarrito(idcliente, idproducto, true, out mensaje);

            }
            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CantidadEnCarrito() {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            int cantidad = new CN_Carrito().CantidadEnCarrito(idcliente);
            return Json(new { cantidad = cantidad },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarProductosCarrito() { 
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            List<Carrito> olista = new List<Carrito>();
            bool conversion;
            olista = new CN_Carrito().ListarProducto(idcliente).Select(oc => new Carrito() {
                oProducto = new Producto() {
                    IdProducto = oc.oProducto.IdProducto,
                    Nombre = oc.oProducto.Nombre,
                    oMarca = oc.oProducto.oMarca,
                    Precio = oc.oProducto.Precio,
                    RutaImagen = oc.oProducto.RutaImagen,
                    Base64 =CN_Recursos.ConvertirBase64(Path.Combine(oc.oProducto.RutaImagen,oc.oProducto.NombreImagen),out conversion),
                    Extension =Path.GetExtension(oc.oProducto.NombreImagen)
            },
                Cantidad = oc.Cantidad
            }).ToList();
            return Json(new { data = olista }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult OperacionCarrito(int idproducto,bool sumar)
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            bool respuesta = false;
            string mensaje = string.Empty;
         
                respuesta = new CN_Carrito().OperacionCarrito(idcliente, idproducto, true, out mensaje);

         
            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarCarrito(int idproducto) { 
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Carrito().EliminarCarrito(idcliente,idproducto);
            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult ObtenerDepartamento()
        { 
            List<Departamento>  olista = new List<Departamento>();
            olista = new CN_Ubicacion().ObtenerDepartamento();
            return Json(new { lista = olista },JsonRequestBehavior.AllowGet);
        
        }
        [HttpPost]
        public JsonResult ObtenerProvincia(string IdDepartamento)
        { 
            List<Provincia>  olista = new List<Provincia>();
            olista = new CN_Ubicacion().ObtenerProvincia(IdDepartamento);
            return Json(new { lista = olista },JsonRequestBehavior.AllowGet);
        
        }
        [HttpPost]
        public JsonResult ObtenerDistrito(string IdDepartamento,string IdProvincia)
        { 
            List<Distrito> olista = new List<Distrito>();
            olista = new CN_Ubicacion().ObtenerDistrito(IdDepartamento,IdProvincia);
            return Json(new { lista = olista },JsonRequestBehavior.AllowGet);
        }

        [ValidarSession]
        [Authorize]
        public ActionResult Carrito() { 
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ProcesarPago(List<Carrito> oListaCarrito, Venta oVenta)
        {
            decimal total = 0;
            DataTable detalle_venta = new DataTable();
            detalle_venta.Locale = new CultureInfo("es-PE");
            detalle_venta.Columns.Add("IdProducto", typeof(string));
            detalle_venta.Columns.Add("Cantidad", typeof(int));
            detalle_venta.Columns.Add("Total", typeof(decimal));
            List<Item> oListaItem = new List<Item>();

            foreach (Carrito oCarrito in oListaCarrito)
            {
                decimal subtotal = oCarrito.Cantidad * oCarrito.oProducto.Precio;
                total += subtotal;
                oListaItem.Add(new Item()
                {
                    name = oCarrito.oProducto.Nombre,
                    quantity = oCarrito.Cantidad.ToString(),
                    unit_amount = new UnitAmount()
                    {
                        currency_code = "USD",
                        value = oCarrito.oProducto.Precio.ToString("F2", CultureInfo.InvariantCulture)
                    }
                });

                detalle_venta.Rows.Add(new object[]
                {
            oCarrito.oProducto.IdProducto,
            oCarrito.Cantidad,
            subtotal
                });
            }

            PurchaseUnit purchaseUnit = new PurchaseUnit()
            {
                amount = new Amount()
                {
                    currency_code = "USD",
                    value = total.ToString("F2", CultureInfo.InvariantCulture),
                    breakdown = new Breakdown()
                    {
                        item_total = new ItemTotal()
                        {
                            currency_code = "USD",
                            value = total.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                description = "Compra de artículo de mi tienda",
                items = oListaItem
            };

            Checkout_Order oCheckOutOrder = new Checkout_Order()
            {
                intent = "CAPTURE",
                purchase_units = new List<PurchaseUnit>() { purchaseUnit },
                application_context = new ApplicationContext()
                {
                    brand_name = "MiTienda.com",
                    landing_page = "NO_PREFERENCE",
                    user_action = "PAY_NOW",
                    return_url = "https://localhost:44312/Tienda/PagoEfectuado",
                    cancel_url = "https://localhost:44312/Tienda/Carrito"
                }
            };

            oVenta.MontoTotal = total;
            oVenta.IdCliente = ((Cliente)Session["Cliente"]).IdCliente;
            TempData["Venta"] = oVenta;
            TempData["DetalleVenta"] = detalle_venta;

            CN_Paypal opaypal = new CN_Paypal();
            Response_Paypal<Response_Checkout> response_paypal;
            try
            {
                response_paypal = await opaypal.CrearSolicitud(oCheckOutOrder);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(response_paypal, JsonRequestBehavior.AllowGet);
        }



        [ValidarSession]
        [Authorize]
        public async Task<ActionResult> PagoEfectuado() {
            string token = Request.QueryString["token"];

            CN_Paypal opaypal = new CN_Paypal();
            Response_Paypal<Response_Capture> response_paypal = new Response_Paypal<Response_Capture>();
            response_paypal = await opaypal.AprobarPago(token);

            ViewData["Status"] = response_paypal.Status;
            if (response_paypal.Status) {
                Venta oVenta = (Venta)TempData["Venta"];
                DataTable detalle_venta = (DataTable)TempData["DetalleVenta"];
                oVenta.IdTransaccion = response_paypal.Response.purchase_units[0].payments.captures[0].id ;
                string mensaje = string.Empty;
                bool respuesta = new CN_Venta().Registrar(oVenta, detalle_venta, out mensaje);
                ViewData["IdTransaccion"] = oVenta.IdTransaccion;
            }
            return View();
        }


        [ValidarSession]
        [Authorize]
        [HttpGet]
        public ActionResult MisCompras()
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            List<DetalleVenta> olista = new List<DetalleVenta>();
            bool conversion;
            olista = new CN_Venta().ListarCompras(idcliente).Select(oc => new DetalleVenta()
            {
                oProducto = new Producto()
                {
                    Nombre = oc.oProducto.Nombre,
                    Precio = oc.oProducto.Precio,
                    Base64 = CN_Recursos.ConvertirBase64(Path.Combine(oc.oProducto.RutaImagen, oc.oProducto.NombreImagen), out conversion),
                    Extension = Path.GetExtension(oc.oProducto.NombreImagen)
                },
                Cantidad = oc.Cantidad,
                Total = oc.Total,
                IdTransaccion = oc.IdTransaccion
            }).ToList();
            return View(olista);

        }

    }
}