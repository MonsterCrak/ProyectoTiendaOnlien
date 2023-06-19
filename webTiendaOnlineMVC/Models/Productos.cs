using System.ComponentModel.DataAnnotations;

namespace webTiendaOnlineMVC.Models
{
    public class Productos
    {
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El campo Nombre es requerido")]
        [StringLength(100, ErrorMessage = "El campo Nombre debe tener como máximo {1} caracteres")]
        public string Nombre { get; set; }

        [StringLength(255, ErrorMessage = "El campo Descripcion debe tener como máximo {1} caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El campo Precio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El campo Precio debe ser un valor positivo")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El campo Stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El campo Stock debe ser un valor positivo")]
        public int Stock { get; set; }

        public int AdministradorId { get; set; }

        [Required(ErrorMessage = "Ingrese un link de imagen")]
        public string ImagenUrl { get; set; }

    }
}
