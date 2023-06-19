using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using System.Reflection;
using webTiendaOnlineMVC.Models;


namespace webTiendaOnlineMVC.Controllers
{
    //[Authorize]
    public class GestionController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly UrlShortener _urlShortener;

        public GestionController(IConfiguration configuration, UrlShortener urlShortener)
        {
            _configuration = configuration;
            _urlShortener = urlShortener;
        }

        //public IActionResult Index()
        //{
        //    // Obtener el rol desde la sesión
        //    int? rol = HttpContext.Session.GetInt32("Rol");

        //    ViewBag.rol = rol;

        //    return View();
        //}

        public IActionResult ListarVentas()
        {
           
            string connectionString = _configuration.GetConnectionString("cn");

            List<Venta> ventas = new List<Venta>();


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SP_ListarVentas", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Venta venta = new Venta
                            {
                                VentaId = (int)reader["VentaId"],
                                UsuarioId = (int)reader["UsuarioId"],
                                FechaVenta = (DateTime)reader["FechaVenta"],
                                MetPagoId = (int)reader["MetPagoId"],
                                EstadoId = (int)reader["EstadoId"],
                                Total = (decimal)reader["Total"]
                            };

                            ventas.Add(venta);
                        }
                    }
                }
            }

        
            return View(ventas);
        }


        public IActionResult ListarProductos()
        {
            string connectionString = _configuration.GetConnectionString("cn");
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;


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
                            ViewBag.usuario = usuarioId;
                            productos.Add(producto);
                        }
                    }
                }
            }

            // Pasa el objeto PagedList a la vista
            return View(productos);
        }






        public IActionResult MergeProductos()
        {
            

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> MergeProductos(Productos producto)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            // Obtener el rol desde la sesión
            int? rol = HttpContext.Session.GetInt32("Rol") ?? 0;
            int? UsuarioId = 2;
            ViewBag.UsuarioId = UsuarioId;


            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("AgregarActualizarProducto", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductoId", producto.ProductoId);
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    command.Parameters.AddWithValue("@Precio", producto.Precio);
                    command.Parameters.AddWithValue("@Stock", producto.Stock);

                    // Acortar la URL de la imagen antes de guardarla en la base de datos
                    string shortenedUrl = await _urlShortener.ShortenUrl(producto.ImagenUrl);
                    command.Parameters.AddWithValue("@Imagen", shortenedUrl ?? producto.ImagenUrl);

                    command.Parameters.AddWithValue("@AdministradorId", rol);

                    SqlParameter outputParameter = new SqlParameter();
                    outputParameter.ParameterName = "@OutputMessage";
                    outputParameter.SqlDbType = System.Data.SqlDbType.NVarChar;
                    outputParameter.Size = 100;
                    outputParameter.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    command.ExecuteNonQuery();

                    string outputMessage = outputParameter.Value.ToString();

                    switch (outputMessage)
                    {
                        case "ProductUpdated":
                            TempData["Message"] = "Producto actualizado exitosamente";
                            break;
                        case "ProductRegistered":
                            TempData["Message"] = "Producto registrado exitosamente";
                            break;
                        case "InvalidAction":
                            TempData["Message"] = "Acción inválida. No tienes los permisos necesarios.";
                            break;
                        default:
                            // Mensaje de error genérico en caso de que ocurra algo inesperado
                            TempData["Message"] = "Error al agregar o actualizar el producto";
                            break;
                    }
                }
            }

            return RedirectToAction("ListarProductos", "Gestion");
        }



        public IActionResult EditarProducto(int id)
        {
            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Productos WHERE ProductoId = @ProductoId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductoId", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Productos producto = new Productos
                            {
                                ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                Nombre = reader["Nombre"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Precio = Convert.ToDecimal(reader["Precio"]),
                                Stock = Convert.ToInt32(reader["Stock"]),
                                ImagenUrl = reader["Imagen"].ToString()
                            };

                            return View(producto);
                        }
                    }
                }
            }

            return RedirectToAction("ListarProductos", "Gestion");
        }



        [HttpPost]
        public async Task<IActionResult> EditarProducto(Productos producto)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            // Obtener el rol desde la sesión
            int? rol = HttpContext.Session.GetInt32("Rol");

            string connectionString = _configuration.GetConnectionString("cn");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("AgregarActualizarProducto", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductoId", producto.ProductoId);
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    command.Parameters.AddWithValue("@Precio", producto.Precio);
                    command.Parameters.AddWithValue("@Stock", producto.Stock);

                    // Acortar la URL de la imagen antes de guardarla en la base de datos
                    string shortenedUrl = await _urlShortener.ShortenUrl(producto.ImagenUrl);
                    command.Parameters.AddWithValue("@Imagen", shortenedUrl ?? producto.ImagenUrl);

                    command.Parameters.AddWithValue("@AdministradorId", rol);

                    SqlParameter outputParameter = new SqlParameter();
                    outputParameter.ParameterName = "@OutputMessage";
                    outputParameter.SqlDbType = System.Data.SqlDbType.NVarChar;
                    outputParameter.Size = 100;
                    outputParameter.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    command.ExecuteNonQuery();

                    string outputMessage = outputParameter.Value.ToString();

                    switch (outputMessage)
                    {
                        case "ProductUpdated":
                            TempData["Message"] = "Producto actualizado exitosamente";
                            break;
                        case "ProductRegistered":
                            TempData["Message"] = "Producto registrado exitosamente";
                            break;
                        case "InvalidAction":
                            TempData["Message"] = "Acción inválida. No tienes los permisos necesarios.";
                            break;
                        default:
                            // Mensaje de error genérico en caso de que ocurra algo inesperado
                            TempData["Message"] = "Error al agregar o actualizar el producto";
                            break;
                    }
                }
            }

            return RedirectToAction("ListarProductos", "Gestion");
        }


        public IActionResult EliminarProducto(int id)
        {
            string connectionString = _configuration.GetConnectionString("cn");

            // Obtener el rol desde la sesión
            int? rol = HttpContext.Session.GetInt32("Rol");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("EliminarProducto", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductoId", id);
                    command.Parameters.AddWithValue("@AdministradorId", rol);

                    SqlParameter outputParameter = new SqlParameter();
                    outputParameter.ParameterName = "@OutputMessage";
                    outputParameter.SqlDbType = System.Data.SqlDbType.NVarChar;
                    outputParameter.Size = 100;
                    outputParameter.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    command.ExecuteNonQuery();

                    string outputMessage = outputParameter.Value.ToString();

                    switch (outputMessage)
                    {
                        case "ProductDeleted":
                            TempData["Message"] = "Producto eliminado exitosamente";
                            break;
                        case "ProductNotFound":
                            TempData["Message"] = "No se encontró el producto";
                            break;
                        case "InvalidAction":
                            TempData["Message"] = "Acción inválida. No tienes los permisos necesarios.";
                            break;
                        case "RequiredAccount":
                            TempData["Message"] = "Se requiere una cuenta para realizar esta acción.";
                            break;
                        case "DeniedAction":
                            TempData["Message"] = "Accion denegada, el producto mantiene registro en otro campo";
                            break;
                        default:
                            // Mensaje de error genérico en caso de que ocurra algo inesperado
                            TempData["Message"] = "Error al eliminar el producto";
                            break;
                    }
                }
            }

            return RedirectToAction("ListarProductos", "Gestion");
        }




    }
}
