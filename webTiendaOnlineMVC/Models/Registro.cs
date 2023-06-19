using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace webTiendaOnlineMVC.Models
{
    public class Registro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El Apellido es requerido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contraseña { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Contraseña", ErrorMessage = "La confirmación de contraseña no coincide")]
        public string ConfirmarContraseña { get; set; }

        public int RolId { get; set; }
    }

}
