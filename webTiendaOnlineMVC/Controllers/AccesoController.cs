using Microsoft.AspNetCore.Mvc;
using System.Data;
using webTiendaOnlineMVC.Models;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;




namespace webTiendaOnlineMVC.Controllers
{
    public class AccesoController : Controller
    {

        public readonly IConfiguration? _configuration;

        public AccesoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Denegado()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Registro registro)
        {
            if (!ModelState.IsValid)
            {
                return View(registro);
            }

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("RegistrarUsuario", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Nombre", registro.Nombre);
                    command.Parameters.AddWithValue("@Apellido", registro.Apellido);
                    command.Parameters.AddWithValue("@Email", registro.CorreoElectronico);
                    command.Parameters.AddWithValue("@Contraseña", registro.Contraseña);
                    command.Parameters.AddWithValue("@ConfirmarContraseña", registro.ConfirmarContraseña);

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
                        case "Success":
                            TempData["Message"] = "Registro exitoso";
                            ModelState.Clear(); // Vaciar el estado del modelo
                            registro = new Registro(); // Crear un nuevo objeto Registro
                            break;
                        case "EmailExists":
                            TempData["Message"] = "Correo existente, prueba con otro";
                            break;
                        case "PasswordMismatch":
                            TempData["Message"] = "La contraseña y la confirmación no coinciden";
                            break;
                        default:
                            // Mensaje de error genérico en caso de que ocurra algo inesperado
                            TempData["Message"] = "Error en el registro";
                            break;
                    }
                }
            }

            return RedirectToAction("Registrar", "Acceso");
        }


        //public IActionResult Registrar(Registro registro)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(registro);
        //    }

        //    string connectionString = _configuration.GetConnectionString("cn");
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        using (SqlCommand command = new SqlCommand("RegistrarUsuario", connection))
        //        {
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.AddWithValue("@Nombre", registro.Nombre);
        //            command.Parameters.AddWithValue("@Apellido", registro.Apellido);
        //            command.Parameters.AddWithValue("@Email", registro.CorreoElectronico);
        //            command.Parameters.AddWithValue("@Contraseña", registro.Contraseña);
        //            command.Parameters.AddWithValue("@ConfirmarContraseña", registro.ConfirmarContraseña);

        //            SqlParameter outputParameter = new SqlParameter();
        //            outputParameter.ParameterName = "@OutputMessage";
        //            outputParameter.SqlDbType = System.Data.SqlDbType.NVarChar;
        //            outputParameter.Size = 100;
        //            outputParameter.Direction = System.Data.ParameterDirection.Output;
        //            command.Parameters.Add(outputParameter);

        //            command.ExecuteNonQuery();

        //            string outputMessage = outputParameter.Value.ToString();

        //            switch (outputMessage)
        //            {
        //                case "Success":
        //                    TempData["Message"] = "Registro exitoso";
        //                    break;
        //                case "EmailExists":
        //                    TempData["Message"] = "Correo existente, prueba con otro";
        //                    break;
        //                case "PasswordMismatch":
        //                    TempData["Message"] = "La contraseña y la confirmación no coinciden";
        //                    break;
        //                default:
        //                    // Mensaje de error genérico en caso de que ocurra algo inesperado
        //                    TempData["Message"] = "Error en el registro";
        //                    break;
        //            }
        //        }
        //    }

        //    return RedirectToAction("Registrar", "Acceso");
        //}



        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.usuario = 0;
            return View();
        }


        [HttpPost]
        public IActionResult Login(Login login)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", login);
            }

            //int roleId = ObtenerRoleId(login.CorreoElectronico, login.Contraseña);
            
            int roleId, usuarioId;
            (roleId, usuarioId) = ObtenerUsuarioIdyRol(login.CorreoElectronico, login.Contraseña);



            if (roleId == 1) // Verificar si el roleId es igual a 1 para el rol de cliente
            {
                // Guardar el rol en la sesión
                HttpContext.Session.SetInt32("Rol", 1);
                HttpContext.Session.SetInt32("UsuarioId", usuarioId);
                ViewBag.usuario = usuarioId;
                return RedirectToAction("ListaProductosVender", "Vistas");
            }
            else if (roleId == 2) // Verificar si el roleId es igual a 2 para el rol de administrador
            {
                // Guardar el rol en la sesión
                HttpContext.Session.SetInt32("Rol", 2);
                HttpContext.Session.SetInt32("UsuarioId", usuarioId);
                ViewBag.usuario = usuarioId;
                return RedirectToAction("ListarProductos", "Gestion");
            }
            else
            {
                HttpContext.Session.SetInt32("UsuarioId", 0);
                ViewBag.usuario = 0;
                TempData["Message"] = "Credenciales inválidas";
                return View("Login", login);
            }
        }


        public IActionResult CambiarUsuarioSesion()
        {
            HttpContext.Session.SetInt32("UsuarioId", 0);
            ViewBag.usuario = HttpContext.Session.GetInt32("UsuarioId");

            return RedirectToAction("Acceso", "Login");
        }






        private (int roleId, int usuarioId) ObtenerUsuarioIdyRol(string correoElectronico, string contraseña)
        {
            int roleId = 0; // Valor predeterminado para el roleId
            int usuarioId = 0; // Valor predeterminado para el usuarioId

            string connectionString = _configuration.GetConnectionString("cn");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("ObtenerUsuarioyRol", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", correoElectronico);
                    command.Parameters.AddWithValue("@Contraseña", contraseña);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            roleId = Convert.ToInt32(reader["roleId"]);
                            usuarioId = Convert.ToInt32(reader["usuarioId"]);
                        }
                    }
                }
            }

            return (roleId, usuarioId);
        }





    }
}
