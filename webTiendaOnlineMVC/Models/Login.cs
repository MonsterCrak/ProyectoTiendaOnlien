using System.ComponentModel.DataAnnotations;

namespace webTiendaOnlineMVC.Models
{
    public class Login
    {
        [Required(ErrorMessage = "El campo Correo Electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese una dirección de correo electrónico válida.")]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es obligatorio.")]
        [DataType(DataType.Password)]
        public string Contraseña { get; set; }
    }
}
