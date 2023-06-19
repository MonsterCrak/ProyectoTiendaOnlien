using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using webTiendaOnlineMVC.Models;

namespace webTiendaOnlineMVC.Controllers
{
    //[Authorize]
    public class VistasController : Controller
    {
        private readonly IConfiguration _configuration;

        public VistasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}



        public IActionResult VistaPago()
        {

            return View();
        }


        public IActionResult ListaProductosVender()
        {
            int? rol = HttpContext.Session.GetInt32("Rol");
            int? Usuario = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            ViewBag.rol = rol;
            ViewBag.Usuario = Usuario;

            string? Mensaje = HttpContext.Session.GetString("Mensaje");

            ViewBag.Mensaje = Mensaje;

            if (Usuario == 0)
            {
                ViewBag.Ocultar = "hidden";
            }
            else { 
                ViewBag.Ocultar = "";
            }

            string connectionString = _configuration.GetConnectionString("cn");

            List<Productos> productos = new List<Productos>();


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("ListaProductos", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Productos producto = new Productos
                            {
                                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Precio = Convert.ToDecimal(reader["Precio"]),
                                Stock = Convert.ToInt32(reader["Stock"]),
                                // Asignar la URL de la imagen desde la base de datos
                                ImagenUrl = reader["Imagen"].ToString()
                            };

                            productos.Add(producto);
                        }
                    }
                }
            }

            // Pasa el objeto PagedList a la vista
            return View(productos);
        }

        //public IActionResult AgregarProductoAlCarrito()
        //{
        //    return View();
        //}

        public async Task<IActionResult> AgregarProductoAlCarrito(int productoId, int cantidad)
        {
            // Obtener el usuario actual desde la sesión
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            ViewBag.Usuario = usuarioId;
            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SP_AgregarProductoAlCarrito", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    command.Parameters.AddWithValue("@ProductoId", productoId);
                    command.Parameters.AddWithValue("@Cantidad", cantidad);

                    SqlParameter outputParameter = new SqlParameter();
                    outputParameter.ParameterName = "@OutputMessage";
                    outputParameter.SqlDbType = SqlDbType.NVarChar;
                    outputParameter.Size = 100;
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    command.ExecuteNonQuery();

                    string outputMessage = outputParameter.Value.ToString();

                    string mensaje;

                    switch (outputMessage)
                    {
                        case "addCartsuccess":
                            mensaje = "Producto agregado al carrito exitosamente";
                            break;
                        case "insufficientStock":
                            mensaje = "No hay suficiente stock del producto";
                            break;
                        case "requiredCant":
                            mensaje = "Cantidad inválida";
                            break;
                        case "InvalidAction":
                            TempData["Message"] = "Acción inválida. No tienes los permisos necesarios.";
                            mensaje = TempData["Message"].ToString();
                            break;
                        case "RequiredAccount":
                            TempData["Message"] = "Inicia sesion para agregar productos a tu carrito";
                            mensaje = TempData["Message"].ToString();
                            break;
                        default:
                            // Mensaje de error genérico en caso de que ocurra algo inesperado
                            TempData["Message"] = "Error al agregar el producto al carrito";
                            mensaje = TempData["Message"].ToString();
                            break;
                    }

                    HttpContext.Session.SetString("Mensaje", mensaje);
                }
            }

            TempData.Peek("Message");

            return RedirectToAction("ListaProductosVender", "Vistas");
        }



        public IActionResult CarritoCompraDetalle()
        {
            int? rol = HttpContext.Session.GetInt32("Rol");
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (!rol.HasValue || !usuarioId.HasValue)
            {
                // Los valores de rol y usuarioId no están disponibles en la sesión
                return RedirectToAction("Login", "Acceso"); // Redirigir a la página de inicio de sesión
            }

            string connectionString = _configuration.GetConnectionString("cn");
            List<DetalleCarrito> detallesCarrito;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string outputMessage;
                detallesCarrito = CarritoCompraDetalle(connection, usuarioId.Value, rol.Value, out outputMessage);

                ViewBag.OutputMessage = outputMessage;
            }

            return View(detallesCarrito);
        }


        [HttpPost]
        private List<DetalleCarrito> CarritoCompraDetalle(SqlConnection connection, int usuarioId, int rol, out string outputMessage)
        {
            outputMessage = string.Empty;
            List<DetalleCarrito> detallesCarrito = new List<DetalleCarrito>();

            using (SqlCommand command = new SqlCommand("ObtenerUltimoCarritoDeCompra", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UsuarioId", usuarioId);
                command.Parameters.AddWithValue("@Rol", rol);

                

                SqlParameter outputParameter = new SqlParameter("@OutputMessage", SqlDbType.VarChar, 100);
                outputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(outputParameter);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DetalleCarrito detalle = new DetalleCarrito
                        {
                            CarritoId = (int)reader["CarritoId"],
                            ProductoId = (int)reader["ProductoId"],
                            Cantidad = (int)reader["CantidadaTotal"],
                            PrecioTotalProducto = (decimal)reader["PrecioTotalProducto"],
                            Producto = new Productos
                            {
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Precio = (decimal)reader["Precio"],
                                ImagenUrl = reader["Imagen"].ToString()
                            }
                        };
                        HttpContext.Session.SetInt32("CarritoId", (int)reader["CarritoId"]);
                        detallesCarrito.Add(detalle);
                    }
                }

                outputMessage = command.Parameters["@OutputMessage"].Value.ToString();
            }

            return detallesCarrito;
        }


        [HttpPost]
        public IActionResult EliminarDetalle(int id)
        {
            try
            {
                // Obtener el UsuarioId y CarritoId de la sesión
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int carritoId = HttpContext.Session.GetInt32("CarritoId") ?? 0;

                // Verificar si los valores de UsuarioId y CarritoId son válidos
                if (usuarioId == 0 || carritoId == 0)
                {
                    return Json(new { success = false, message = "Usuario o carrito inválido." });
                }

                // Obtener la conexión a la base de datos desde la configuración
                string connectionString = _configuration.GetConnectionString("cn");

                // Realizar la conexión a la base de datos y llamar al procedimiento almacenado "EliminarProductoDetalleCarrito"
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("EliminarProductoDetalleCarrito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@ProductoId", id);
                        command.Parameters.AddWithValue("@CarritoId", carritoId);
                        command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        SqlParameter outputParameter = new SqlParameter("@OutputMessage", SqlDbType.VarChar, 100);
                        outputParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outputParameter);

                        command.ExecuteNonQuery();

                        string outputMessage = command.Parameters["@OutputMessage"].Value.ToString();

                        // Verificar el mensaje de salida y devolver una respuesta JSON adecuada
                        if (outputMessage == "Success")
                        {
                            return Json(new { success = true });
                        }
                        else if (outputMessage == "NoDetailsFound")
                        {
                            return Json(new { success = false, message = "No se encontraron detalles de carrito para el producto especificado." });
                        }
                        else if (outputMessage == "InvalidCart")
                        {
                            return Json(new { success = false, message = "El carrito no pertenece al usuario especificado." });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Error al eliminar el producto del carrito." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //

        public List<OpcionPago> ObtenerMetodosPago()
        {
            List<OpcionPago> metodosPago = new List<OpcionPago>();

            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Obtener la cadena de conexión desde la configuración
            string connectionString = _configuration.GetConnectionString("cn");

            // Establecer la conexión a la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrir la conexión
                connection.Open();

                // Crear el comando SQL
                SqlCommand command = new SqlCommand("SP_ListarMetodosPago", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Ejecutar el comando y obtener los datos en un lector de datos
                SqlDataReader reader = command.ExecuteReader();

                // Recorrer los registros y crear objetos opcionpago
                while (reader.Read())
                {
                    OpcionPago metodoPago = new OpcionPago();
                    metodoPago.MetPagoId = Convert.ToInt32(reader["MetPagoId"]);
                    metodoPago.Tipo = reader["Tipo"].ToString();
                    metodoPago.Descripcion = reader["Descripcion"].ToString();
                    metodosPago.Add(metodoPago);
                }
                // Cerrar el lector de datos
                reader.Close();

                // Cerrar la conexión
                connection.Close();
            }

            return metodosPago;
        }




        public IActionResult VenderProducto()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            List<OpcionPago> metodosPago = ObtenerMetodosPago();
            ViewBag.MetodosPago = metodosPago;

            return View();
        }



        public IActionResult accionVender(int? metodoPagoSelect)
        {
            if (metodoPagoSelect.HasValue)
            {
                int? usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                string outputMessage = "";

                // Obtener la cadena de conexión desde la configuración
                string connectionString = _configuration.GetConnectionString("cn");

                // Obtener los métodos de pago y asignarlos a ViewBag.MetodosPago
                ViewBag.MetodosPago = ObtenerMetodosPago();

                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand("RegistrarVenta", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    // Agregar el parámetro del método de pago solo si tiene un valor
                    command.Parameters.AddWithValue("@MetPagoId", metodoPagoSelect.Value);

                    var outputParameter = new SqlParameter("@OutputMessage", SqlDbType.VarChar, 100);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    connection.Open();
                    command.ExecuteNonQuery();

                    outputMessage = outputParameter.Value.ToString();
                }

                // Crear un objeto JSON con el mensaje de salida
                var result = new { message = outputMessage };

                ViewBag.Message = outputMessage;
                // Redirigir a la acción "CarritoCompraDetalle" en lugar de "accionVender"
                return RedirectToAction("VistaPago", "Vistas", new { area = "" });
            }
            else
            {
                // Si el método de pago no tiene valor, retornar un mensaje de error o manejarlo de acuerdo a tus necesidades.
                return RedirectToAction("VenderProducto", "Vistas");
            }
        }



        //
    }
}
