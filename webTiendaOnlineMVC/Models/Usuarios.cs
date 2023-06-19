using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace webTiendaOnlineMVC.Models
{
    public class Usuarios
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        public string Contraseña { get; set; }

        public int RolId { get; set; }
    }
}
